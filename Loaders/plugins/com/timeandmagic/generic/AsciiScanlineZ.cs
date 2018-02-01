using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Utility;
using Utility.Text.RegularExpressions;
using Builders;

namespace Loaders.plugins.com.timeandmagic.generic
{
    /// <summary>
    /// A generic reader for scanline z grids. Reads only lists of Z
    /// values, and places them into a predefined grid.
    /// </summary>
    class AsciiScanlineZ : Loader, IFactoryProduct
    {
        #region Fields
        private static Regex[] uriPatterns = { };
        private static string[] fileExtensions = { "*" };
        private static string description = "ASCII Raster Grid";
        private GridParameters parameters = new GridParameters();
        //private IGrid2DBuilder builder = null;
        public readonly static Regex commentBlanksPattern = new Regex(@"^(\s*)|(\s*[!#].*)");
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
            // Search through the file, if it contains only numbers
            // and more than three numbers per line, it might be a z file
            int searchLimit = 50; // Number of lines to test
            Regex searchPattern = new Regex(@"\s*(" + Patterns.CFloat + @"\s+){4,}");
            using (SkippingLineReader lineReader = new SkippingLineReader(Location.LocalPath, commentBlanksPattern))
            {
                do
                {
                    string line = lineReader.ReadLine();
                    if (line != null)
                    {
                        break;
                    }
                    if (!searchPattern.IsMatch(line))
                    {
                        return Recognition.No;
                    }
                }
                while (lineReader.LogicalLineNumber < searchLimit);
            }
            return Recognition.Maybe;
        }

        public override void Open(IGeoModel model)
        {
            // Read all of the Z values into an redimensionable array.
            Regex delimiterPattern = new Regex(@"[\s,;]+");
            using (SkippingLineReader lineReader = new SkippingLineReader(Location.LocalPath, commentBlanksPattern))
            {
                try
                {
                    string line;
                    // TODO: This redimensionable array needs to be a new type
                    //       containing a 1-D array, but which we can view as
                    //       a 2-D array with different sizes.  For now, we use
                    //       a List
                    List<double> redimensionableArray = new List<double>();
                    while ((line = lineReader.ReadLine()) != null)
                    {
                        string[] fields = delimiterPattern.Split(line);
                        foreach (string field in fields)
                        {
                            double value = Double.Parse(field);
                            redimensionableArray.Add(value);
                        }
                    }
                }
                catch (FormatException formatException)
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendFormat("Invalid field value at line {0}", lineReader.PhysicalLineNumber);
                    throw new OpenException(message.ToString(), Location, formatException); 
                }
            }

            // TODO: Write AsciiScanlineZ heuristics

            // Try to guess the null value using statistics and by testing
            // commonly used values. Null value could be the most common. 

            // Factorize the length of the z array into a set of factors
            
            // Try each pair of common factors to determine which produces
            // the smoothest grid: Test each node against those to its north
            // and east - sum the differences. The grid with the smallest
            // total difference is the smoothest.

            // TODO: Write AsciiScanlineZ loader
        }
        #endregion
        #endregion

        #region IFactoryProduct Members

        public object GetFactoryKey()
        {
            return "AsciiScanlineZ";
        }

        #endregion
    }
}
