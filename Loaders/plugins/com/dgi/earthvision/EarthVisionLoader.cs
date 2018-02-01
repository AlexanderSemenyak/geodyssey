using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

using Utility;
using Builders;

namespace Loaders.plugins.com.dgi.earthvision
{
    public sealed class EarthVisionLoader : Loader, IFactoryProduct
    {
        private static Regex[] uriPatterns = { new Regex(@"\.2grd", RegexOptions.IgnoreCase),
                                               new Regex(@"ev", RegexOptions.IgnoreCase)      };
        private static string[] fileExtensions = { "*", "2grd" };
        private static string description = "EarthVision ASCII grid";
        private static Regex tokensRegex = new Regex(@"^#\s+(Type|Version|Description|Format|Projection|Zone|Units|Ellipsoid|Field|Units|Grid_size|Grid_X_range|Grid_Y_Range):");
        private GridParameters parameters = new GridParameters();
        private int maxFieldCode = 0; // The highest field code encountered
        private int searchLimit = 50;
        private IGrid2DBuilder builder = null;

        #region Constructors
        public EarthVisionLoader()
        {
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
            // Check for any EarthVision header tokens at the beginning of the file
            // We count success at being at least three lines. 
            using (ILineReader lineReader = new SkippingLineReader(Location.LocalPath))
            {
                int matching = 0;
                for (int i = 0; i < searchLimit ; ++i)
                {
                    string line = lineReader.ReadLine();
                    if (tokensRegex.IsMatch(line))
                    {
                        ++matching;
                        if (matching >= 3)
                        {
                            return Recognition.Yes;
                        }
                    }
                }
                return matching == 0 ? Recognition.No : Recognition.Maybe;
            }
        }

        public override void Open(IGeoModel model)
        {
            Debug.Assert(model != null);
            builder = model.CreateGridBuilder();
            Debug.Assert(builder != null);
            using (ILineReader lineReader = new SkippingLineReader(Location.LocalPath))
            {
                ReadHeader(lineReader);
                BuildGridParameters();
                switch (maxFieldCode)
                {
                    case 3:
                        // XYZ points not supported
                        // TODO: Signal not supported
                        break;
                    case 5:
                        ReadFiveColumnGrid(lineReader);
                        break;
                    case 7:
                        ReadFiveColumnGrid(lineReader); // Ignore the ID and name columns
                        break;
                    default:
                        // Unknown
                        // TODO: Inform user.
                        break;
                }
            }
            builder.Build();
        }

        #endregion
        #endregion

        #region Methods
        private void ReadHeader(ILineReader lineReader)
        {
            string line;
            while ((line = lineReader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith("#"))
                {
                    string[] fields = Regex.Split(line, @"\s+");
                    // TODO: Check we have enough fields
                    string tag = fields[1];
                    if (tag == "Field:")
                    {
                        ParseFieldTag(fields);
                    }
                    else if (tag == "Units:")
                    {
                        ParseUnitsTag(fields);
                    }
                    else if (tag == "Grid_size:")
                    {
                        ParseGridSize(fields);
                    }
                    else if (tag == "Grid_X_range:")
                    {
                        ParseGridXRange(fields);
                    }
                    else if (tag == "Grid_Y_range:")
                    {
                        ParseGridYRange(fields);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void ParseFieldTag(string[] fields)
        {
            string fieldType = fields[2];
            int fieldCode = Int32.Parse(fieldType);
            switch (fieldCode)
            {
                case 1:
                    // First column - ? z units
                    break;
                case 2:
                    // Second column - ? y units?
                    break;
                case 3:
                    // Third column
                    string zUnits = fields[3];
                    break;
                case 4:
                    // Fourth column - ? i?
                    break;
                case 5:
                    // Fifth colum - ? j?
                    break;
                case 6:
                    // ID column
                    string idType = fields[3];
                    break;
                case 7:
                    // Name column
                    string nameType = fields[3];
                    break;
                default:
                    Debug.Assert(false, "Unhandled field code");
                    // TODO: Better to throw an exception here.
                    break;
            }
            // The hightest fieldCode we encounter tells us how
            // many columns of data to expect.
            if (fieldCode > maxFieldCode)
            {
                maxFieldCode = fieldCode;
            }
        }

        private void ParseUnitsTag(string[] fields)
        {
            // TODO: Check fields has sufficient entries
            string units = fields[2];
        }

        private void ParseGridSize(string[] fields)
        {
            parameters.INum = int.Parse(fields[2]);
            parameters.JNum = int.Parse(fields[4]);
        }

        private void ParseGridXRange(string[] fields)
        {
            parameters.XOrigin = double.Parse(fields[2]);
            parameters.XMax = double.Parse(fields[4]);
        }

        private void ParseGridYRange(string[] fields)
        {
            parameters.YOrigin = double.Parse(fields[2]);
            parameters.YMax = double.Parse(fields[4]);
        }

        private void ReadFiveColumnGrid(ILineReader lineReader)
        {
            Debug.Assert(lineReader != null);
            Debug.Assert(builder != null);
            // Read the grid data
            string line;
            while ((line = lineReader.ReadLine()) != null)
            {
                string[] fields = Regex.Split(line, @"\s+");
                if (fields.Length == 5)
                {
                    double z = double.Parse(fields[2]);
                    int i = int.Parse(fields[3]);
                    int j = int.Parse(fields[4]);
                    builder[i, j] = z;
                }
                else
                {
                    break;
                }
            }
        }

        private void BuildGridParameters()
        {
            Debug.Assert(builder != null);
            parameters.VerifyAndCompleteParameters();
            parameters.Build(builder);
        }

        #endregion

        #region IFactoryProduct Members
        public object GetFactoryKey()
        {
            return "EarthVisionAsciiGrid";
        }
        #endregion
    }
}
