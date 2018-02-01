using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Loaders
{
    class LineReader : ILineReader
    {
        private List<string> lineCache = new List<string>();
        private int maxCachedLines = 128;
        private int currentLine = -1; /// The current line number
        private int highestCachedLine = -1; /// The line number of highest numbered cached line
        private int highestIndex = -1; /// The index in the cache of the highest number cached line 
        private StreamReader streamReader;

        #region Constructors

        public LineReader(string path) :
            this(new StreamReader(path))
        {
        }

        public LineReader(string path, int maxCachedLines) :
            this(new StreamReader(path), maxCachedLines)
        {
        }

        public LineReader(StreamReader streamReader)
        {
            this.streamReader = streamReader;
        }

        public LineReader(StreamReader streamReader, int maxCachedLines) :
            this(streamReader)
        {
            this.maxCachedLines = maxCachedLines;
        }

        #endregion

        #region ILineReader Members

        public int LogicalLineNumber
        {
            get { return PhysicalLineNumber; }
        }

        public int PhysicalLineNumber
        {
            get { return currentLine + 1; }
        }

        // <summary>
        // Read the next line, without advancing the current line number. It may
        // affect the contents of the internal cache.
        // </summary>
        public string PeekLine()
        {
            string peekedLine = ReadLine();
            UnreadLine();
            return peekedLine;
        }

        // <summary>
        // Reads a line of characters from the current stream and returns the data as
        // a string.
        // </summary>
        //
        // <returns>
        // The next line from the input stream, or null if the end of the input stream
        // is reached.
        // </returns>
        //
        // <exceptions>
        //   System.IO.IOException:
        //     An I/O error occurs.
        //
        //   System.OutOfMemoryException:
        //     There is insufficient memory to allocate a buffer for the returned string.
        // </exceptions>
        public string ReadLine()
        {
            string line = null;
            // Is the next line in the cache?
            int nextIndex = currentLine + 1;
            if ((nextIndex > highestCachedLine - lineCache.Count) && (nextIndex <= highestCachedLine))
            {
                int index = highestIndex + nextIndex - highestCachedLine;
                currentLine = nextIndex;
                if (index < 0)
                {
                    // Negative index counts back from end of cache
                    index = lineCache.Count - index;
                }
                line = lineCache[index];
            }
            else
            {
                line = streamReader.ReadLine();
                ++highestIndex;
                // Check for cache overflow
                if (highestIndex >= maxCachedLines)
                {
                    highestIndex = 0;
                }
                if (highestIndex > lineCache.Count - 1)
                {
                    lineCache.Add(line);
                    highestCachedLine = nextIndex;
                }
                else
                {
                    lineCache[highestIndex] = line;
                }
            }
            currentLine = nextIndex;
            return line;
        }

        /// <summary>
        /// Unread a line from the stream. Reads backwards through the stream, returning
        /// lines which have previously been read. A call to ReadLine(), followed by a call to
        /// UnreadLine() will return the same line. Allows 'backing up' when reading a file line-wise.
        /// This method may be called multiple times in succession, upto the size of the internal
        /// line cache.
        /// </summary>
        /// <returns>A line of text from the file, which will have been previously read</returns>
        /// <exception cref="CacheUnderflow">Raised when the internal line cache is exhausted,
        /// and no more lines can be Unread() from the cache.</exception>

        public string UnreadLine()
        {
            if (currentLine <= 0)
            {
                return null;
            }

            if (currentLine <= highestCachedLine - lineCache.Count)
            {
                throw new IndexOutOfRangeException("Line cache underflow");
            }

            int index = highestIndex + currentLine - highestCachedLine;
            --currentLine;
            string line = lineCache[index];
            return line;
        }

        #endregion

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
                    streamReader.Dispose();
                }
            }
            disposed = true;
        }

        ~LineReader()
        {
            CleanUp(false);
        }

        #endregion
    }
}
