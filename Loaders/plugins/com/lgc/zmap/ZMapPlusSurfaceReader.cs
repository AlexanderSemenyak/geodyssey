using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

using Utility;
using Builders;
using Utility.Text.RegularExpressions;

namespace Loaders.plugins.com.lgc.zmap
{
    class ZMapPlusSurfaceReader : IZMapReader, IFactoryProduct
    {
        #region Fields
        private IGrid2DBuilder builder = null;
        private readonly GridParameters parameters = new GridParameters();
        private static readonly char[] separator = new[] { ' ', '\t' };
        #endregion

        #region IZMapReader Members
        public void Read(ILineReader lineReader, IGeoModel model)
        {
            // Pre-condition: The next line to be read will be the first line
            // of the header
            builder = model.CreateGridBuilder();
            try
            {
                ReadHeader(lineReader);
                parameters.VerifyAndCompleteParameters();
                parameters.Build(builder);
                ReadBody(lineReader);
            }
            catch (FormatException formatException)
            {
                StringBuilder message = new StringBuilder();
                message.AppendFormat("Unable to parse ZMap file at line {0} because {1}", lineReader.PhysicalLineNumber, formatException.Message);
                throw new OpenException(message.ToString());
            }
            builder.Build();
        }
        #endregion

        private void ReadBody(ILineReader lineReader)
        {
            Debug.Assert(parameters.INum.HasValue);
            // Parse the body of the grid
            string line;
            int counter = 0;
            while ((line = lineReader.ReadLine()) != null)
            {
                string[] fields = SplitOnWhitespace(line);
                foreach (string field in fields)
                {
                    double z;
                    if (double.TryParse(field, out z))
                    {
                        int i = counter / parameters.JNum.Value;
                        int j = parameters.JNum.Value - (counter % parameters.JNum.Value);
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

        private static string[] SplitOnWhitespace(string line)
        {

            //return Patterns.Whitespace.Split(line.Trim());
            return line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        }

        private void ReadHeader(ILineReader lineReader)
        {
            // TODO: Parse failure exception handling

            
            // Header 1 : name, type, number of values per line
            string header1 = lineReader.ReadLine();
            string[] fields1 = header1.Split(',');
            if (fields1.Length < 3)
            {
                StringBuilder message = new StringBuilder();
                message.AppendFormat("Expected at least two comma separated fields at line {0}", lineReader.PhysicalLineNumber);
                throw new OpenException(message.ToString());
            }
           
            Match header1Match = Regex.Match(fields1[0], @"\s*@\s*(.*)\s*HEADER\s*");
            if (!header1Match.Success)
            {
                StringBuilder message = new StringBuilder();
                message.AppendFormat("First line of header does not match expected pattern at line {0}", lineReader.PhysicalLineNumber);
                throw new OpenException(message.ToString());
            }
            string headerName = header1Match.Groups[1].ToString();
            string headerType = fields1[1].Trim();
            int headerValuesPerLine = Int32.Parse(fields1[2]);

            // Header 2 : field width, Z null value, Z null text, decimal position, start position
            string header2 = lineReader.ReadLine();
            string[] fields2 = header2.Split(',');
            // TODO Header2 error handling
            int fieldWidth = Int32.Parse(fields2[0].Trim());
            parameters.ZNull = Double.Parse(fields2[1]);
            string nullText = fields2[2].Trim();
            int decimalPosition = Int32.Parse(fields2[3]);
            int startPosition = Int32.Parse(fields2[4]);

            // Header 3: number of rows (j), number of columns (i), X min, X max, Y min, Y max 
            string header3 = lineReader.ReadLine();
            string[] fields3 = header3.Split(',');
            // TODO Header3 error handling
            parameters.JNum = Int32.Parse(fields3[0]);
            parameters.INum = Int32.Parse(fields3[1]);
            parameters.XOrigin = Double.Parse(fields3[2]);
            parameters.XMax = Double.Parse(fields3[3]);
            parameters.YOrigin = Double.Parse(fields3[4]);
            parameters.YMax = Double.Parse(fields3[5]);

            // Header 4: three floats. What do they mean?
            string header4 = lineReader.ReadLine();
            // TODO: Parse this header

            // Header 5: We expect a line starting with @ to terminate the header
            string header5 = lineReader.ReadLine();
            if (!header5.StartsWith("@"))
            {
                StringBuilder message = new StringBuilder();
                message.AppendFormat("Malformed ZMap header at line {0}. Expected header termination", lineReader.PhysicalLineNumber);
                throw new OpenException(message.ToString());
            }
        }

        #region IFactoryProduct Members

        public object GetFactoryKey()
        {
            return "Surface";
        }

        #endregion
    }
}
