using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Utility;
using Utility.IO;
using Builders;

namespace Loaders.plugins.com.goldensoftware.surfer
{
    class SurferGrid : Loader, IFactoryProduct
    {
        #region Fields
        private static readonly Regex[] uriPatterns = { new Regex(@"\.grd$", RegexOptions.IgnoreCase) };
        private static readonly string[] fileExtensions = { "grd" };
        private static readonly string description = "Surfer Grid";
        private static readonly Regex blanksPattern = new Regex(@"^\s*$");
        private GridParameters parameters = new GridParameters();
        private IGrid2DBuilder builder = null;
        #endregion

        private enum SubType
        {
            Unknown, SurferAscii, Surfer6Binary, Surfer7Binary
        }

        private SubType subType = SubType.Unknown;

        #region Constructors

        public SurferGrid()
        {
            parameters.ZNull = 1.70141e+38;
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
            DetermineType();
            return subType != SubType.Unknown ? Recognition.Yes : Recognition.No;    
        }

        public override void Open(IGeoModel model)
        {
            Debug.Assert(model != null);
            builder = model.CreateGridBuilder();
            DetermineType();
            switch (subType)
            {
                case SubType.SurferAscii:
                    OpenSurferAscii();
                    break;
                case SubType.Surfer6Binary:
                    OpenSurfer6Binary();
                    break;
                case SubType.Surfer7Binary:
                    OpenSurfer7Binary();
                    break;
                default:
                    throw new OpenException("Not recognised as a Golden Software Surfer Binary grid file", Location);
            }
            builder.Build();
        }
        #endregion

        private void DetermineType()
        {
            if (subType == SubType.Unknown)
            {
                // Look for the first magic four bytes of the file
                // This also allows us to determine whether it is
                // Surfer 6 or Surfer 7 binary format
                using (FileStream stream = new FileStream(Location.LocalPath, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = EndianBinaryReader.CreateForLittleEndianData(stream))
                {
                    Int32 magic = reader.ReadInt32();
                    switch (magic)
                    {
                        case 0x41415344:
                            subType = SubType.SurferAscii;
                            break;
                        case 0x42425344:
                            subType = SubType.Surfer6Binary;
                            break;
                        case 0x42525344:
                            subType = SubType.Surfer7Binary;
                            break;
	                    default:
                            subType = SubType.Unknown;
                            break;
                    }
                }
            }
        }

        #region Surfer ASCII Methods

        private void OpenSurferAscii()
        {
            using (ILineReader reader = new SkippingLineReader(Location.LocalPath, blanksPattern))
            {
                // TODO: Create a wrapper for the StreamReader which
                // skips comment lines and tracks line numbers
                try
                {
                    ReadAsciiHeader(reader);
                    parameters.VerifyAndCompleteParameters();
                    parameters.Build(builder);
                    ReadAsciiBody(reader);
                }
                catch (System.FormatException)
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendFormat("Invalid grid format at line {0}", reader.PhysicalLineNumber);
                    throw new OpenException(message.ToString());
                }
            }
        }

        private void ReadAsciiHeader(ILineReader reader)
        {
            // Line 1 - the identifier
            string header1 = reader.ReadLine();
            if (!header1.StartsWith("DSAA"))
            {
                throw new OpenException("Golden Software ASCII grid must start with 'DSAA'");
            }
            // Line 2 - the number of points along the I and J axes 
            string header2 = reader.ReadLine();
            string[] fields2 = Regex.Split(header2.Trim(), @"\s+");
            parameters.INum = int.Parse(fields2[0]);
            parameters.JNum = int.Parse(fields2[1]);

            // Line 3 - the extent in the I direction
            string header3 = reader.ReadLine();
            string[] fields3 = Regex.Split(header3.Trim(), @"\s+");
            parameters.XOrigin = double.Parse(fields3[0]);
            parameters.XMax = double.Parse(fields3[1]);

            // Line 4 - the extent in the J direction
            string header4 = reader.ReadLine();
            string[] fields4 = Regex.Split(header4.Trim(), @"\s+");
            parameters.YOrigin = double.Parse(fields4[0]);
            parameters.YMax = double.Parse(fields4[1]);

            // Line 4 - the extent in the J direction
            string header5 = reader.ReadLine();
            string[] fields5 = Regex.Split(header5.Trim(), @"\s+");
            parameters.ZMin = double.Parse(fields5[0]);
            parameters.ZMax = double.Parse(fields5[1]);
        }

        private void ReadAsciiBody(ILineReader reader)
        {
            Debug.Assert(parameters.INum.HasValue);
            string line;
            int counter = 0;
            while ((line = reader.ReadLine()) != null)
            {
                string[] fields = Regex.Split(line.Trim(), @"\s+");
                foreach (string field in fields)
                {
                    double z;
                    if (double.TryParse(field, out z))
                    {
                        if (z != parameters.ZNull && z >= parameters.ZMin && z <= parameters.ZMax)
                        {
                            int i = counter % parameters.INum.Value;
                            int j = counter / parameters.INum.Value;
                            builder[i, j] = z;
                        }
                        ++counter;
                    }
                    else
                    {
                        StringBuilder message = new StringBuilder();
                        message.AppendFormat("Bad grid data '{0}' at line {1}", field, reader.PhysicalLineNumber);
                        throw new OpenException(message.ToString());
                    }
                }
            }
        }
        #endregion

        #region Surfer 6 Binary Methods

        private void OpenSurfer6Binary()
        {
            using (FileStream stream = new FileStream(Location.LocalPath, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = EndianBinaryReader.CreateForLittleEndianData(stream))
            {
                try
                {
                    ReadSurfer6BinaryHeader(reader);
                    ReadBinaryBodySingle(reader);
                }
                catch (EndOfStreamException)
                {
                    throw new OpenException("Unexpected end of file", Location);
                }
            }
        }

        private void ReadSurfer6BinaryHeader(BinaryReader reader)
        {
            Int32 magic = reader.ReadInt32();
            if (magic != 0x42425344) // 'DSBB' - little endian
            {
                throw new OpenException("Golden Software Surfer 6 binary file with wrong magic number", Location);
            }
            parameters.INum = reader.ReadInt16();
            parameters.JNum = reader.ReadInt16();
            parameters.XOrigin = reader.ReadDouble();
            parameters.XMax = reader.ReadDouble();
            parameters.YOrigin = reader.ReadDouble();
            parameters.YMax = reader.ReadDouble();
            parameters.ZMin = reader.ReadDouble();
            parameters.ZMax = reader.ReadDouble();
            parameters.VerifyAndCompleteParameters();
            parameters.Build(builder);
        }

        #endregion

        #region Surfer 7 Binary Methods

        private void OpenSurfer7Binary()
        {
            using (FileStream stream = new FileStream(Location.LocalPath, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = EndianBinaryReader.CreateForLittleEndianData(stream))
            {
                try
                {
                    // We keep track of our own length rather than testing for EOF which is
                    // inefficient with FileStream
                    long length = stream.Length;
                    if (length < 4)
                    {
                        throw new OpenException("Surfer 7 Binary File too short");
                    }

                    long position = 0;
                    while (position < length)
                    {
                        ReadRecord(reader, ref position);
                    }
                }
                catch (EndOfStreamException)
                {
                    throw new OpenException("Unexpected end of file");
                }
            }
        }

        private long ReadRecord(BinaryReader reader, ref long position)
        {
            int recordLength = 0;
            int sectionType = ReadTag(reader, ref position, out recordLength);
            switch (sectionType)
            {
                case 0x42525344: // Header
                    ReadHeaderSection(reader, recordLength);
                    break;
                case 0x44495247: // Grid section
                    ReadGridSection(reader, recordLength);
                    break;
                case 0x41544144: // Data section as per observed
                case 0x41544244: // Data section as per manual
                    ReadDataSection(reader, recordLength);
                    break;
                case 0x49544c46: // Fault info section
                    ReadFaultSection(reader, recordLength);
                    break;
                default:
                    // Unknown section type, so skip over it
                    reader.ReadBytes(recordLength);
                    break;
            }
            position += recordLength;
            return position;
        }

        private void ReadHeaderSection(BinaryReader reader, int size)
        {
            int version = reader.ReadInt32();
        }

        private void ReadGridSection(BinaryReader reader, int size)
        {
            parameters.JNum = reader.ReadInt32();
            parameters.INum = reader.ReadInt32();
            parameters.XOrigin = reader.ReadDouble();
            parameters.YOrigin = reader.ReadDouble();
            parameters.IInc = reader.ReadDouble();
            parameters.JInc = reader.ReadDouble();
            parameters.ZMin = reader.ReadDouble();
            parameters.ZMax = reader.ReadDouble();
            reader.ReadDouble(); // Rotation angle not used
            parameters.ZNull = reader.ReadDouble();
            parameters.VerifyAndCompleteParameters();
            parameters.Build(builder);
        }

        private void ReadDataSection(BinaryReader reader, int size)
        {
            ReadBinaryBodyDouble(reader);
        }

        private void ReadFaultSection(BinaryReader reader, int size)
        {
            // TODO: Implement fault centreline reading
            reader.ReadBytes(size);
        }

        /// <summary>
        /// Read a Surfer 7 version 1 tag, consisting of two 32 bit
        /// integers comprising an Id and a Size in bytes.
        /// </summary>
        /// <returns></returns>
        private int ReadTag(BinaryReader reader, ref long position, out int size)
        {
            int id = reader.ReadInt32();
            position += 4;
            size = reader.ReadInt32();
            position += 4;
            return id;
        }

        #endregion

        #region Generic Binary Methods

        private void ReadBinaryBodySingle(BinaryReader reader)
        {
            for (int j = 0; j < parameters.JNum.Value; ++j)
            {
                for (int i = 0 ; i < parameters.INum.Value ; ++i)
                {
                    double z = reader.ReadSingle();
                    if (z != parameters.ZNull && parameters.ZMin <= z && z <= parameters.ZMax)
                    {
                        builder[i, j] = z;
                    }
                }
            }
        }

        private void ReadBinaryBodyDouble(BinaryReader reader)
        {
            for (int j = 0; j < parameters.JNum.Value; ++j)
            {
                for (int i = 0; i < parameters.INum.Value; ++i)
                {
                    double z = reader.ReadDouble();
                    if (z != parameters.ZNull && parameters.ZMin <= z && z <= parameters.ZMax)
                    {
                        builder[i, j] = z;
                    }
                }
            }
        }

        #endregion
        #endregion

        #region IFactoryProduct Members

        public object GetFactoryKey()
        {
            return "Surfer";
        }

        #endregion
    }
}
