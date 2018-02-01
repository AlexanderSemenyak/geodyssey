using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

using Utility;
using Utility.Text.RegularExpressions;
using Builders;

namespace Loaders.plugins.com.slb.cps3
{
    public class Cps3SurfaceLoader : ICps3EntityLoader, IFactoryProduct
    {
        #region Fields
        private GridParameters parameters = new GridParameters();
        private IGrid2DBuilder builder = null;

        private static readonly Regex entityNamePattern = new Regex(@"^\s*->\s*(?:CPS|MSMODL):?\s*(.*)");
        private static readonly Regex xOriginPattern = new Regex(@"(?:X\s+Origin|XMIN)\s*:\s*(" + Patterns.CFloat + @")\s*(\w*)", RegexOptions.IgnoreCase);
        private static readonly Regex yOriginPattern = new Regex(@"(?:Y\s+Origin|YMIN)\s*:\s*(" + Patterns.CFloat + @")\s*(\w*)", RegexOptions.IgnoreCase);
        private static readonly Regex xIntervalPattern = new Regex(@"(?:X\s+Interval|XINC)\s*:\s*(" + Patterns.CFloat + @")\s*(\w*)", RegexOptions.IgnoreCase);
        private static readonly Regex yIntervalPattern = new Regex(@"(?:Y\s+Interval|YINC)\s*:\s*(" + Patterns.CFloat + @")\s*(\w*)", RegexOptions.IgnoreCase);
        private static readonly Regex rotationAnglePattern = new Regex(@"Rotation\s+Angle\s*:\s*(" + Patterns.CFloat + ")", RegexOptions.IgnoreCase);
        private static readonly Regex nColPattern = new Regex(@"NCOL\s*:\s*(\d+)", RegexOptions.IgnoreCase);
        private static readonly Regex nRowPattern = new Regex(@"NROW\s*:\s*(\d+)", RegexOptions.IgnoreCase);
        private static readonly Regex numberListPattern = new Regex(@"^(" + Patterns.CFloat + @"\s+){3,}$"); // TODO Complete this regex

        private string name = null;
        private Uri uri = null;
        #endregion

        #region Construction
        public Cps3SurfaceLoader()
        {
            parameters.ZNull = 1.0e30;
        }
        #endregion

        #region Properties
        public Uri Location
        {
            get { return uri; }
            set { uri = value; }
        }
        #endregion

        #region Methods
        public void Open(IGeoModel model)
        {
            Debug.Assert(uri != null);
            Debug.Assert(model != null);
            builder = model.CreateGridBuilder();
            int postHeaderLine = ParseHeader();
            // Read the body of the grid, skipping comment and blank lines
            using (SkippingLineReader lineReader = new SkippingLineReader(uri.LocalPath, Cps3.commentBlanksPattern))
            {
                // Skip forward over the header
                while (lineReader.PhysicalLineNumber < postHeaderLine)
                {
                    lineReader.ReadLineNoSkip();
                }

                // Parse the body of the grid
                string line;
                int counter = 0;
                while ((line = lineReader.ReadLine()) != null)
                {
                    string[] fields = Regex.Split(line, @"\s+");
                    foreach (string field in fields)
                    {
                        double z;
                        if (double.TryParse(field, out z))
                        {
                            int i = counter % parameters.INum.Value;
                            int j = counter / parameters.INum.Value;
                            if (z != parameters.ZNull)
                            {
                                builder[i, j] = z;
                            }
                            ++counter;
                        }
                        else
                        {
                            StringBuilder message = new StringBuilder();
                            message.AppendFormat("Bad grid data '{0}' at line {1}", field, lineReader.PhysicalLineNumber);
                            throw new OpenException(message.ToString());
                        }
                    }
                }                 
            }
        }

        /// <summary>
        /// Parse the CPS3 header
        /// </summary>
        /// <returns>The line number of the first line following the header</returns>
        private int ParseHeader()
        {
            // Two common styles of header are found with CPS3

            // Header with grid parameters as data
            // FSASCI 0 1 "Computed" 0 1E30 0
            // FSATTR 0 0
            // FSLIMI 340 760 180 620 941.7343 1158.477
            // FSNROW 45 43
            // FSXINC 10 10

            // Header using Volume Of Interest in comments
            // FSASCI 0 1 "Computed" 0 1E30 0
            // FSATTR 0 0
            // !Grid Lattice: Generic Binset                                                    
            // !VOI Box           XMIN: 0.0 ft                                                  
            // !                  XMAX: 24606.3007812 ft                                        
            // !                  YMIN: 0.0 ft                                                  
            // !                  YMAX: 20505.2500000 ft                                        
            // !Lattice           XINC: 205.05251 ft
            int lineNumber = -1;
            using (LineReader lineReader = new LineReader(uri.LocalPath))
            {
                do
                {
                    string line = lineReader.ReadLine();
                    // Check for premature end of file
                    if (line == null)
                    {
                        StringBuilder message = new StringBuilder();
                        message.AppendFormat("End of file unexpectedly reached at line {0}", lineReader.PhysicalLineNumber);
                        throw new OpenException(message.ToString());
                    }
                    // Check for end of header - marked by first entity name
                    if (entityNamePattern.IsMatch(line))
                    {
                        name = entityNamePattern.Match(line).Captures[0].ToString();
                        break;
                    }
                    else if (line.StartsWith("FSASCI"))
                    {
                        ParseFsasci(lineReader, line);
                    }
                    else if (line.StartsWith("FSLIMI"))
                    {
                        ParseFslimi(lineReader, line);
                    }
                    else if (line.StartsWith("FSNROW"))
                    {
                        ParseFsnrow(lineReader, line);
                    }
                    else if (line.StartsWith("FSXINC"))
                    {
                        ParseFsinc(lineReader, line);
                    }
                    else if (line.StartsWith("!"))
                    {
                        // Comment line which may contain header information
                        
                        if (xOriginPattern.IsMatch(line))
                        {
                            ParseXOrigin(line);
                        }
                        else if (yOriginPattern.IsMatch(line))
                        {
                            ParseYOrigin(line);
                        }
                        else if (xIntervalPattern.IsMatch(line))
                        {
                            ParseXInterval(line);
                        }
                        else if (rotationAnglePattern.IsMatch(line))
                        {
                            ParseRotationAngle(line);
                        }
                    }
                    else if (numberListPattern.IsMatch(line))
                    {
                        // Too far - we are into the grid data. Back up.
                        lineReader.UnreadLine();
                        break;
                    }

                }
                while (true);
                lineNumber = lineReader.PhysicalLineNumber;
            }
            VerifyHeaderCompleteness();
            return lineNumber;
        }

        /// <summary>
        /// Verify that we have collected enough information from the
        /// header to describe the grid.
        /// </summary>
        private void VerifyHeaderCompleteness()
        {
            parameters.VerifyAndCompleteParameters();
            if (name != null)
            {
                // TODO: Build a grid name from the filename
                name = uri.LocalPath;
            }
        }

        private void ParseRotationAngle(string line)
        {
            GroupCollection groups = rotationAnglePattern.Match(line).Groups;
            parameters.Orientation = Double.Parse(groups[0].ToString());
        }

        private void ParseXOrigin(string line)
        {
            GroupCollection groups = xOriginPattern.Match(line).Groups;
            // TODO: xOrigin is a better name than xMin
            parameters.XOrigin = Double.Parse(groups[1].ToString());
            if (groups.Count > 5)
            {
                string xUnits = groups[5].ToString();
                // TODO: Use this to transform to SI if necessary
            }
        }

        private void ParseYOrigin(string line)
        {
            GroupCollection groups = yOriginPattern.Match(line).Groups;
            // TODO: yOrigin is a better name than xMin
            parameters.YOrigin = Double.Parse(groups[1].ToString());
            if (groups.Count > 5)
            {
                string yUnits = groups[5].ToString();
                // TODO: Use this to transform to SI if necessary
            }
        }

        private void ParseXInterval(string line)
        {
            GroupCollection groups = xIntervalPattern.Match(line).Groups;
            parameters.IInc = Double.Parse(groups[1].ToString());
            if (groups.Count > 5)
            {
                string xIncUnits = groups[5].ToString();
                // TODO: Use this to transform to SI if necessary
            }
        }

        private void ParseYInterval(string line)
        {
            GroupCollection groups = yIntervalPattern.Match(line).Groups;
            parameters.JInc = Double.Parse(groups[1].ToString());
            if (groups.Count > 5)
            {
                string yIncUnits = groups[5].ToString();
                // TODO: Use this to transform to SI if necessary
            }
        }

        private void ParseFsasci(LineReader lineReader, string line)
        {
            // TODO: What do the other fields mean?
            string[] fields = Regex.Split(line, @"\s+");
            int numRequiredFields = 6;
            if (fields.Length < numRequiredFields)
            {
                StringBuilder message = new StringBuilder();
                message.AppendFormat("FSASCI requires at least {0} fields, only {1} provided, at line {2}", numRequiredFields - 1, fields.Length - 1, lineReader.PhysicalLineNumber);
                throw new OpenException(message.ToString(), uri);
            }
            parameters.ZNull = Double.Parse(fields[5]);
        }

        private void ParseFslimi(LineReader lineReader, string line)
        {
            string[] fields = Regex.Split(line, @"\s+");
            int numRequiredFields = 7;
            if (fields.Length < numRequiredFields)
            {
                StringBuilder message = new StringBuilder();
                message.AppendFormat("FSLIMI requires {0} fields, only {1} provided, at line {2}", numRequiredFields - 1, fields.Length - 1, lineReader.PhysicalLineNumber);
                throw new OpenException(message.ToString(), uri); 
            }
            parameters.XOrigin = Double.Parse(fields[1]);
            parameters.XMax = Double.Parse(fields[2]);
            parameters.YOrigin = Double.Parse(fields[3]);
            parameters.YMax = Double.Parse(fields[4]);
            parameters.ZMin = Double.Parse(fields[5]);
            parameters.ZMax = Double.Parse(fields[6]);
        }

        private void ParseFsnrow(LineReader lineReader, string line)
        {
            string[] fields = Regex.Split(line, @"\s+");
            int numRequiredFields = 3;
            if (fields.Length < numRequiredFields)
            {
                StringBuilder message = new StringBuilder();
                message.AppendFormat("FSNROW requires {0} fields, only {1} provided, at line {2}", numRequiredFields - 1, fields.Length - 1, lineReader.PhysicalLineNumber);
                throw new OpenException(message.ToString(), uri);
            }
            parameters.JNum = Int32.Parse(fields[1]);
            parameters.INum = Int32.Parse(fields[2]);
        }

        private void ParseFsinc(LineReader lineReader, string line)
        {
            string[] fields = Regex.Split(line, @"\s+");
            int numRequiredFields = 3;
            if (fields.Length < numRequiredFields)
            {
                StringBuilder message = new StringBuilder();
                message.AppendFormat("FSXINC requires {0} fields, only {1} provided, at line {2}", numRequiredFields - 1, fields.Length - 1, lineReader.PhysicalLineNumber);
                throw new OpenException(message.ToString(), uri);
            }
            parameters.IInc = Double.Parse(fields[1]);
            parameters.JInc = Double.Parse(fields[2]);
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
