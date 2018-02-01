using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Utility;
using Builders;
using Utility.Text.RegularExpressions;

namespace Loaders.plugins.com.roxar.irapclassic
{
    public class IrapClassicAscii : Loader, IFactoryProduct
    {
        private readonly static Regex[] uriPatterns = { new Regex(@"irap", RegexOptions.IgnoreCase),
                                                        new Regex(@"rms", RegexOptions.IgnoreCase),
                                                        new Regex(@"\.gri$", RegexOptions.IgnoreCase) };
        private readonly static string[] fileExtensions = { "*" };
        private const string description = "Irap Classic Grid";
        private readonly GridParameters parameters = new GridParameters();
        private IGrid2DBuilder builder = null;
        private readonly static Regex commentBlanksPattern = new Regex(@"^(\s*)");
        private readonly char[] separator = new[] {' ', '\t'};

        #region Constructors
        public IrapClassicAscii()
        {
            parameters.ZNull = 9999900.0;
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
            using (LineReader lineReader = new LineReader(Location.LocalPath))
            {
                string line = lineReader.ReadLine();
                if (line != null)
                {
                    string[] fields = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    if (fields.Length >= 1)
                    {
                        int magic;
                        bool parsed = Int32.TryParse(fields[0], out magic);
                        if (parsed && magic == -996)
                        {
                            return Recognition.Yes;
                        }
                    }
                }
            }
            return Recognition.No;
        }

        public override void Open(IGeoModel model)
        {
            Debug.Assert(model != null);
            builder = model.CreateGridBuilder();
            using (LineReader lineReader = new LineReader(Location.LocalPath))
            {
                try
                {
                    ReadHeader(lineReader);
                    ReadRowMajorGrid(lineReader);
                }
                catch (EndOfStreamException endOfStreamException)
                {
                    throw new OpenException("Unexpected end of file", Location, endOfStreamException);
                }
            }
            builder.Build();
        }

        #endregion
        #endregion

        // TODO: Factor this method out somhow - pass in grid
        //       parameters. Modify grid parameters to contain
        //       information on whether the grid is stored in
        //       RowMajor or ColumnMajor order, and in order ot
        //       increasing or decreasing I and J
        private void ReadRowMajorGrid(ILineReader lineReader)
        {
            Debug.Assert(parameters.INum.HasValue);
            string line;
            int counter = 0;
            while ((line = lineReader.ReadLine()) != null)
            {
                string[] fields = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                //string[] fields = Patterns.Whitespace.Split(line.Trim());
                foreach (string field in fields)
                {
                    double z;
                    if (double.TryParse(field, out z))
                    {
                        if (z != parameters.ZNull)
                        {
                            int i = counter % parameters.INum.Value;
                            int j = counter / parameters.INum.Value; 
                            builder[i, j] = -z; // Irap Classic is in depth
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

        private void ReadHeader(ILineReader lineReader)
        {
            // TODO: Cope with truncated headers
            string header1 = lineReader.ReadLine();
            string[] fields1 = header1.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            parameters.JNum = Int32.Parse(fields1[1]);
            parameters.IInc = Double.Parse(fields1[2]);
            parameters.JInc = Double.Parse(fields1[3]);

            string header2 = lineReader.ReadLine();
            string[] fields2 = header2.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            parameters.XOrigin = Double.Parse(fields2[0]);
            parameters.XMax = Double.Parse(fields2[1]);
            parameters.YOrigin = Double.Parse(fields2[2]);
            parameters.YMax = Double.Parse(fields2[3]);

            string header3 = lineReader.ReadLine();
            string[] fields3 = header3.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            parameters.INum = Int32.Parse(fields3[0]);
            parameters.Orientation = Double.Parse(fields3[1]);
            // Two other unused fields here

            lineReader.ReadLine(); // Header 4 - Always seems to contain seven zeros

            parameters.VerifyAndCompleteParameters();
            parameters.Build(builder);
        }

        #region IFactoryProduct Members

        public object GetFactoryKey()
        {
            return "IrapClassicAsciiGrid";
        }

        #endregion
    }
}
