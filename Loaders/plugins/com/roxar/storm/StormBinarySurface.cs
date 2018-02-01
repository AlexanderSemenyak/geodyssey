using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Utility;
using Utility.IO;
using Builders;

namespace Loaders.plugins.com.roxar.storm
{
    class StormBinarySurface : Loader, IFactoryProduct
    {
        private readonly static Regex[] uriPatterns = { new Regex(@"storm", RegexOptions.IgnoreCase) };
        private readonly static string[] fileExtensions = { "*" };
        private readonly string description = "STORM Binary Grid";
        private GridParameters parameters = new GridParameters();
        private IGrid2DBuilder builder = null;

        #region Constructors
        public StormBinarySurface()
        {
            parameters.ZNull = -999.0;
        }
        #endregion

        #region ILoader Implementation
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
            using (LineReader lineReader = new LineReader(Location.LocalPath))
            {
                foundMagic = ReadMagic(lineReader);
            }
            return foundMagic ? Recognition.Yes : Recognition.No;
        }

        public override void Open(IGeoModel model)
        {
            Debug.Assert(model != null);
            builder = model.CreateGridBuilder();
            ReadHeader();
            ReadBody();
            builder.Build();
        }
        #endregion
        #endregion

        #region IFactoryProduct Members
        public object GetFactoryKey()
        {
            return "StormBinarySurface";
        }
        #endregion

        #region Methods
        private static bool ReadMagic(LineReader lineReader)
        {
            string line = lineReader.ReadLine();
            if (line != null)
            {
                string magic = line.Trim();
                if (magic == "STORMGRID_BINARY")
                {
                    return true;
                }
            }
            return false;
        }

        private void ReadHeader()
        {
            // Read the first part of the file using a LineReader
            using (StreamReader reader = new StreamReader(Location.LocalPath))
            using (LineReader lineReader = new LineReader(reader))
            {
                bool foundMagic = ReadMagic(lineReader);
                if (!foundMagic)
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendFormat("Incorrect STORM grid header at line {0}", lineReader.PhysicalLineNumber);
                    throw new OpenException(message.ToString());
                }

                // Skip one line
                lineReader.ReadLine();

                // Read the grid dimensions and spacing
                string header1 = lineReader.ReadLine();
                string[] fields1 = Regex.Split(header1.Trim(), @"\s+");
                parameters.INum = Int32.Parse(fields1[0]);
                parameters.JNum = Int32.Parse(fields1[1]);
                parameters.IInc = Double.Parse(fields1[2]);
                parameters.JInc = Double.Parse(fields1[3]);

                // Read xmin xmax ymin ymax
                string header2 = lineReader.ReadLine();
                string[] fields2 = Regex.Split(header2.Trim(), @"\s+");
                parameters.XOrigin = Double.Parse(fields2[0]);
                parameters.XMax = Double.Parse(fields2[1]);
                parameters.YOrigin = Double.Parse(fields2[2]);
                parameters.YMax = Double.Parse(fields2[3]);

                parameters.VerifyAndCompleteParameters();
                parameters.Build(builder);
            }
        }

        private void ReadBody()
        {
            // Read the grid body
            using (FileStream stream = new FileStream(Location.LocalPath, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = EndianBinaryReader.CreateForBigEndianData(stream))
            {
                for (int p = 0; p < 4; ++p)
                {
                    SkipLine(reader);
                }

                for (int j = 0; j < parameters.JNum; ++j)
                {
                    for (int i = 0; i < parameters.INum; ++i)
                    {
                        double z = reader.ReadDouble();
                        if (z != parameters.ZNull)
                        {
                            builder[i, j] = -z; // Storm grids are in depth
                        }
                    }
                }
            }
        }

        private static void SkipLine(BinaryReader reader)
        {
            while (reader.ReadByte() != 0x0A); // Storm files use UNIX line end
        }
        #endregion
    }
}
