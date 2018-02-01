using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Utility;
using Utility.IO;
using Builders;

namespace Loaders.plugins.com.dgi.earthvision
{
    public sealed class EarthVisionBinaryGrid : Loader, IFactoryProduct
    {
        #region Fields
        private static Regex[] uriPatterns = { new Regex(@"\.2grd", RegexOptions.IgnoreCase),
                                               new Regex(@"ev", RegexOptions.IgnoreCase)      };
        private static string[] fileExtensions = { "*", "2grd" };
        private static string description = "EarthVision Binary Grid";
        private GridParameters parameters = new GridParameters();
        private IGrid2DBuilder builder = null;
        #endregion

        #region Constructors
        public EarthVisionBinaryGrid()
        {
            parameters.ZNull = 1e20;
        }
        #endregion

        #region ILoader Members
        #region Properties
        public override Regex[] UriPatterns
        {
            get { return uriPatterns; }
        }

        public override string[] FileExtensions
        {
            get { return fileExtensions; }
        }

        public override string Description
        {
            get { return description; }
        }
        #endregion

        #region Methods
        public override Recognition Recognize()
        {
            bool foundMagic = false;
            using (FileStream stream = new FileStream(Location.LocalPath, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = EndianBinaryReader.CreateForBigEndianData(stream))
            {
                foundMagic = ReadMagic(reader);
            }
            return foundMagic ? Recognition.Yes : Recognition.No;
        }

        public override void Open(IGeoModel model)
        {
            Debug.Assert(model != null);
            builder = model.CreateGridBuilder();
            Debug.Assert(builder != null);
            using (FileStream stream = new FileStream(Location.LocalPath, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = EndianBinaryReader.CreateForBigEndianData(stream))
            {
                // We keep track of our own length rather than testing for EOF which is
                // inefficient with FileStream
                long length = stream.Length;
                if (length < 4)
                {
                    throw new OpenException("EarthVision Binary File too short");
                }
                long position = 0;
                bool foundMagic = ReadMagic(reader);
                if (!foundMagic)
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendFormat("Incorrect EarthVision Binary Grid header");
                    throw new OpenException(message.ToString());
                }
                position += 4;

                while (position < length)
                {
                    ReadRecord(reader, ref position);
                }
            }
            builder.Build();
        }
        #endregion
        #endregion

        #region Methods
        private static bool ReadMagic(BinaryReader reader)
        {
            Int32 magic = reader.ReadInt32();
            if (magic == 0x2d2d2d2d)
            {
                return true;
            }
            return false;
        }

        private void ReadRecord(BinaryReader reader, ref long position)
        {
            Int32 id = reader.ReadInt32();
            position += 4;
            Int32 recordLength = reader.ReadInt32();
            position += 4;
            switch (id)
            {
                case 0x03:
                    parameters.XOrigin = reader.ReadDouble();
                    break;
                case 0x04:
                    parameters.XMax = reader.ReadDouble();
                    break;
                case 0x05:
                    parameters.YOrigin = reader.ReadDouble();
                    break;
                case 0x06:
                    parameters.YMax = reader.ReadDouble();
                    break;
                case 0x09:
                    parameters.INum = reader.ReadInt32();
                    break;
                case 0x0A:
                    parameters.JNum = reader.ReadInt32();
                    break;
                case 0x1E:
                    ReadBody(reader, recordLength);
                    break;
                default:
                    // Unknown field type or fields we ignore
                    reader.ReadBytes(recordLength);
                    break;
            }
            position += recordLength;
        }

        private void ReadBody(BinaryReader reader, int recordLength)
        {
            // First check that we have collected sufficient grid
            // parameters to continue
            parameters.VerifyAndCompleteParameters();
            parameters.Build(builder);
            int numValues = parameters.INum.Value * parameters.JNum.Value;
            if (recordLength != numValues * sizeof(float))
            {
                throw new OpenException("EarthVision Binary Grid data block is wrong size.");
            }
            float fNull = (float) parameters.ZNull.Value;
            for (int counter = 0; counter < numValues; ++counter)
            {
                int i = counter % parameters.INum.Value;
                int j = counter / parameters.INum.Value; 
                float z = reader.ReadSingle();
                if (z != fNull)
                {
                    builder[i, j] = z;
                }
            }
        }
        #endregion

        #region IFactoryProduct Members
        public object GetFactoryKey()
        {
            return "EarthVisionBinaryGrid";
        }
        #endregion
    }
}
