using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

using Utility;
using Utility.Text.RegularExpressions;
using Builders;

namespace Loaders.plugins.com.slb.charisma
{
    class CharismaSurfaceLoader : ICharismaEntityLoader, IFactoryProduct
    {
        #region Fields
        private static GridParameters parameters = new GridParameters();
        public readonly static Regex commentBlanksPattern = new Regex(@"^(\s*)|(\s*#.*)$");
        private Uri uri = null;
        #endregion

        #region Construction
        public CharismaSurfaceLoader()
        {
            parameters.ZNull = 9999.0;
        }
        #endregion

        #region Properties
        public Uri Location
        {
            get { return uri; }
            set { uri = value; }
        }
        #endregion

        #region ICharismaEntityLoader Members
        public void Open(IGeoModel model)
        {
            Debug.Assert(model != null);
            using (SkippingLineReader lineReader = new SkippingLineReader(uri.LocalPath, commentBlanksPattern))
            {
                int numSurfaces = ReadHeader(lineReader);

                // Read the body
                string line;
                while ((line = lineReader.ReadLine()) != null)
                {
                    // Split the fields in the line
                    string name      = line.Substring(0, 16).Trim();
                    string shotPoint = line.Substring(16, 10).Trim();
                    double x = Double.Parse(line.Substring(26, 10).Trim());
                    double y = Double.Parse(line.Substring(36, 10).Trim());
                    // Retrieve the next n values, where n is the number of horizons
                    string[] zs = line.Substring(46).Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);

                    if (zs.Length < numSurfaces)
                    {
                        StringBuilder message = new StringBuilder();
                        message.AppendFormat("Too few z values at line {0}", lineReader.PhysicalLineNumber);
                        throw new OpenException(message.ToString(), uri);
                    }

                    double[] z = new double[numSurfaces];
                    for (int i = 0 ; i < numSurfaces ; ++i)
                    {
                        // Charisma Z values may have non-numeric characters
                        // within the field, such as 1234.45F so we use a regex
                        // to extract the substring the matches a float
                        Match match = Patterns.CFloat.Match(zs[i]);
                        if (match.Success)
                        {
                            z[i] = Double.Parse(match.Value);
                        }
                        else
                        {
                            StringBuilder message = new StringBuilder();
                            message.AppendFormat("Bad value '{0}' at line {1}", zs[i], lineReader.PhysicalLineNumber);
                            throw new OpenException(message.ToString(), uri);
                        }
                    }
                    // TODO: Call the builder here
                }
            }
        }

        private static int ReadHeader(SkippingLineReader lineReader)
        {
            int numSurfaces = 0;
            // Read the header
            int searchLimit = 50;
            do
            {
                string line = lineReader.ReadLineNoSkip().Trim();
                if (Charisma.surfaceRegex.IsMatch(line))
                {
                    // The header line has been located
                    // TODO Extract the Charisma surface names, and
                    //      and determine how many there are.
                    numSurfaces = 1; // TODO: Fix numSurfaces
                    break;
                }
            }
            while (lineReader.PhysicalLineNumber < searchLimit);

            if (numSurfaces == 0)
            {
                throw new OpenException("No surfaces found in Charisma file");
            }
            return numSurfaces;
        }
        #endregion

        #region IFactoryProduct Members

        public object GetFactoryKey()
        {
            return "Surface";
        }

        #endregion
    }
}
