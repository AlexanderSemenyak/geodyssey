using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Loaders
{
    class SkippingLineReader : ILineReader, IDisposable
    {
        private readonly LineReader lineReader;
        private readonly Regex skipLinePattern;
        private int logicalLineNumber = 0;

        #region Constructors

        public SkippingLineReader(string path) :
            this(new StreamReader(path))
        {
        }

        public SkippingLineReader(StreamReader streamReader)
        {
            this.lineReader = new LineReader(streamReader);
            this.skipLinePattern = new Regex(@"^\s*$");
        }

        public SkippingLineReader(string path, Regex skipLinePattern) :
            this(new StreamReader(path), skipLinePattern)
        {
        }

        public SkippingLineReader(string path, Regex skipLinePattern, int maxCachedLines) :
            this(new StreamReader(path), skipLinePattern, maxCachedLines)
        {
        }

        public SkippingLineReader(StreamReader streamReader, Regex skipLinePattern)
        {
            this.lineReader = new LineReader(streamReader);
            this.skipLinePattern = skipLinePattern;
        }

        public SkippingLineReader(StreamReader streamReader, Regex skipLinePattern, int maxCachedLines)
        {
            this.lineReader = new LineReader(streamReader, maxCachedLines);
            this.skipLinePattern = skipLinePattern;
        }

        #endregion

        #region ILineReader Members

        public int LogicalLineNumber
        {
            get { return logicalLineNumber; }
        }

        public int PhysicalLineNumber
        {
            get { return lineReader.PhysicalLineNumber; }
        }

        public string PeekLine()
        {
            string peekedLine = ReadLine();
            UnreadLine();
            return peekedLine;
        }

        public string ReadLine()
        {
            string line = null;
            do
            {
                line = lineReader.ReadLine();
                if (line == null)
                {
                    return null;
                }
            }
            while (skipLinePattern.IsMatch(line));
            ++logicalLineNumber;
            return line;
        }

        public string UnreadLine()
        {
            string line = null;
            do
            {
                line = lineReader.UnreadLine();
            }
            while (skipLinePattern.IsMatch(line));

            --logicalLineNumber;
            return line;
        }

        #endregion

        public string ReadLineNoSkip()
        {
            string line = lineReader.ReadLine();
            ++logicalLineNumber;
            return line;
        }

        #region Dispose Pattern

        private bool disposed = false;

        public void Dispose()
        {
            CleanUp(true);
            GC.SuppressFinalize(this);
        }

        private void CleanUp(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    lineReader.Dispose();
                }
            }
            disposed = true;
        }

        ~SkippingLineReader()
        {
            CleanUp(false);
        }

        #endregion
    }
}
