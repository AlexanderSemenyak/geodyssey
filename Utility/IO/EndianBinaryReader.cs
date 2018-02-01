using System;
using System.Collections.Generic;
using System.Text;

using Utility.Binary;

namespace Utility.IO
{
    // Summary:
    //     Reads primitive data types as binary values in a specific encoding.
    // TODO: Make this a subclass of System.IO.BinaryReader
    public class EndianBinaryReader : System.IO.BinaryReader
    {
        #region Fields
        private EndianBitConverter bitConverter;
        private bool hasBeenDisposed = false;
        #endregion

        public static EndianBinaryReader CreateForBigEndianData(System.IO.Stream input)
        {
            return new EndianBinaryReader(input, EndianBitConverter.CreateForBigEngianData());
        }

        public static EndianBinaryReader CreateForLittleEndianData(System.IO.Stream input)
        {
            return new EndianBinaryReader(input, EndianBitConverter.CreateForLittleEndianData());
        }

        public static EndianBinaryReader CreateForBigEndianData(System.IO.Stream input, Encoding encoding)
        {
            return new EndianBinaryReader(input, encoding, EndianBitConverter.CreateForBigEngianData());
        }

        public static EndianBinaryReader CreateForLittleEndianData(System.IO.Stream input, Encoding encoding)
        {
            return new EndianBinaryReader(input, encoding, EndianBitConverter.CreateForLittleEndianData());
        }

        // Summary:
        //     Initializes a new instance of the System.IO.EndianBinaryReader class based on the
        //     supplied stream and using System.Text.UTF8Encoding.
        //
        // Parameters:
        //   input:
        //     A stream.

        //   swapEndianness:
        //     True if the incoming bytes are to have their endianness swapped.
        // Exceptions:
        //   System.ArgumentException:
        //     The stream does not support reading, the stream is null, or the stream is
        //     already closed.
        public EndianBinaryReader(System.IO.Stream input, EndianBitConverter bitConverter)
            :
            base(input)
        {
            this.bitConverter = bitConverter;
        }
        //
        // Summary:
        //     Initializes a new instance of the System.IO.EndianBinaryReader class based on the
        //     supplied stream and a specific character encoding.
        //
        // Parameters:
        //   encoding:
        //     The character encoding.
        //
        //   input:
        //     The supplied stream.
        //
        //   swapEndianness:
        //     True if the incoming bytes are to have their endianness swapped.
        // Exceptions:
        //   System.ArgumentNullException:
        //     encoding is null.
        //
        //   System.ArgumentException:
        //     The stream does not support reading, the stream is null, or the stream is
        //     already closed.
        public EndianBinaryReader(System.IO.Stream input, Encoding encoding, EndianBitConverter bitConverter)
            :
            base(input, encoding)
        {
            this.bitConverter = bitConverter;
        }

        // Summary:
        //     Releases the unmanaged resources used by the System.IO.EndianBinaryReader and optionally
        //     releases the managed resources.
        //
        // Parameters:
        //   disposing:
        //     true to release both managed and unmanaged resources; false to release only
        //     unmanaged resources.
        protected override void Dispose(bool disposing)
        {
            if (!hasBeenDisposed)
            {
                try
                {
                    GC.SuppressFinalize(this);
                    base.Dispose(disposing);
                }
                catch (Exception)
                {
                    hasBeenDisposed = false;
                    throw;
                }
                hasBeenDisposed = true;
            }
        }

        // The finalizer
        ~EndianBinaryReader()
        {
            Dispose(false);
        }

        //
        // Summary:
        //     Returns the next available character and does not advance the byte or character
        //     position.
        //
        // Returns:
        //     The next available character, or -1 if no more characters are available or
        //     the stream does not support seeking.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurs.
        public override int PeekChar()
        {
            int chr = base.PeekChar();
            // TODO: Byteswap the char
            return chr;
        }

        //
        // Summary:
        //     Reads characters from the underlying stream and advances the current position
        //     of the stream in accordance with the Encoding used and the specific character
        //     being read from the stream.
        //
        // Returns:
        //     The next character from the input stream, or -1 if no characters are currently
        //     available.
        //
        // Exceptions:
        //   System.ObjectDisposedException:
        //     The stream is closed.
        //
        //   System.IO.IOException:
        //     An I/O error occurs.
        public override int Read()
        {
            int chr = base.Read();
            // TODO: Byteswap chr
            return chr;
        }
      
        // Summary:
        //     Reads count characters from the stream with index as the starting point in
        //     the character array.
        //
        // Parameters:
        //   count:
        //     The number of characters to read.
        //
        //   buffer:
        //     The buffer to read data into.
        //
        //   index:
        //     The starting point in the buffer at which to begin reading into the buffer.
        //
        // Returns:
        //     The total number of characters read into the buffer. This might be less than
        //     the number of characters requested if that many characters are not currently
        //     available, or it might be zero if the end of the stream is reached.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     buffer is null.
        //
        //   System.IO.IOException:
        //     An I/O error occurs.
        //
        //   System.ObjectDisposedException:
        //     The stream is closed.
        //
        //   System.ArgumentOutOfRangeException:
        //     index or count is negative.
        //
        //   System.ArgumentException:
        //     The buffer length minus index is less than count.
        public override int Read(char[] buffer, int index, int count)
        {
            int num = base.Read(buffer, index, count);
            // TODO: Byteswap chars in the buffer
            return num;
        }

        //
        // Summary:
        //     Reads the next character from the current stream and advances the current
        //     position of the stream in accordance with the Encoding used and the specific
        //     character being read from the stream.
        //
        // Returns:
        //     A character read from the current stream.
        //
        // Exceptions:
        //   System.ObjectDisposedException:
        //     The stream is closed.
        //
        //   System.IO.IOException:
        //     An I/O error occurs.
        //
        //   System.IO.EndOfStreamException:
        //     The end of the stream is reached.
        //
        //   System.ArgumentException:
        //     A surrogate character was read.
        public override char ReadChar()
        {
            byte[] bytes = ReadBytes(sizeof(char));
            return bitConverter.ToChar(bytes, 0);
        }

        //
        // Summary:
        //     Reads count characters from the current stream, returns the data in a character
        //     array, and advances the current position in accordance with the Encoding
        //     used and the specific character being read from the stream.
        //
        // Parameters:
        //   count:
        //     The number of characters to read.
        //
        // Returns:
        //     A character array containing data read from the underlying stream. This might
        //     be less than the number of characters requested if the end of the stream
        //     is reached.
        //
        // Exceptions:
        //   System.ObjectDisposedException:
        //     The stream is closed.
        //
        //   System.IO.IOException:
        //     An I/O error occurs.
        //
        //   System.IO.EndOfStreamException:
        //     The end of the stream is reached.
        //
        //   System.ArgumentOutOfRangeException:
        //     count is negative.
        public override char[] ReadChars(int count)
        {
            char[] chrs = new char[count];
            for (int i = 0 ; i < count ; ++i)
            {
                byte[] bytes = ReadBytes(sizeof(char));
                chrs[i] = bitConverter.ToChar(bytes, 0);
            }
            return chrs;
        }

        // Summary:
        //     Reads an 8-byte floating point value from the current stream and advances
        //     the current position of the stream by eight bytes.
        //
        // Returns:
        //     An 8-byte floating point value read from the current stream.
        //
        // Exceptions:
        //   System.ObjectDisposedException:
        //     The stream is closed.
        //
        //   System.IO.IOException:
        //     An I/O error occurs.
        //
        //   System.IO.EndOfStreamException:
        //     The end of the stream is reached.
        public override double ReadDouble()
        {
            byte[] bytes = ReadBytes(sizeof(double));
            return bitConverter.ToDouble(bytes, 0);
        }

        // Summary:
        //     Reads a 2-byte signed integer from the current stream and advances the current
        //     position of the stream by two bytes.
        //
        // Returns:
        //     A 2-byte signed integer read from the current stream.
        //
        // Exceptions:
        //   System.ObjectDisposedException:
        //     The stream is closed.
        //
        //   System.IO.IOException:
        //     An I/O error occurs.
        //
        //   System.IO.EndOfStreamException:
        //     The end of the stream is reached.
        public override short ReadInt16()
        {
            byte[] bytes = ReadBytes(sizeof(System.Int16));
            return bitConverter.ToInt16(bytes, 0);
        }

        //
        // Summary:
        //     Reads a 4-byte signed integer from the current stream and advances the current
        //     position of the stream by four bytes.
        //
        // Returns:
        //     A 4-byte signed integer read from the current stream.
        //
        // Exceptions:
        //   System.ObjectDisposedException:
        //     The stream is closed.
        //
        //   System.IO.IOException:
        //     An I/O error occurs.
        //
        //   System.IO.EndOfStreamException:
        //     The end of the stream is reached.
        public override int ReadInt32()
        {
            byte[] bytes = ReadBytes(sizeof(System.Int32));
            return bitConverter.ToInt32(bytes, 0);
        }

        //
        // Summary:
        //     Reads an 8-byte signed integer from the current stream and advances the current
        //     position of the stream by eight bytes.
        //
        // Returns:
        //     An 8-byte signed integer read from the current stream.
        //
        // Exceptions:
        //   System.ObjectDisposedException:
        //     The stream is closed.
        //
        //   System.IO.IOException:
        //     An I/O error occurs.
        //
        //   System.IO.EndOfStreamException:
        //     The end of the stream is reached.
        public override long ReadInt64()
        {
            byte[] bytes = ReadBytes(sizeof(System.Int64));
            return bitConverter.ToInt64(bytes, 0);
        }

        // Summary:
        //     Reads a 4-byte floating point value from the current stream and advances
        //     the current position of the stream by four bytes.
        //
        // Returns:
        //     A 4-byte floating point value read from the current stream.
        //
        // Exceptions:
        //   System.ObjectDisposedException:
        //     The stream is closed.
        //
        //   System.IO.IOException:
        //     An I/O error occurs.
        //
        //   System.IO.EndOfStreamException:
        //     The end of the stream is reached.
        public override float ReadSingle()
        {
            byte[] bytes = ReadBytes(sizeof(System.Single));
            return bitConverter.ToSingle(bytes, 0);
        }

        // Summary:
        //     Reads a 2-byte unsigned integer from the current stream using little endian
        //     encoding and advances the position of the stream by two bytes.
        //
        // Returns:
        //     A 2-byte unsigned integer read from this stream.
        //
        // Exceptions:
        //   System.ObjectDisposedException:
        //     The stream is closed.
        //
        //   System.IO.IOException:
        //     An I/O error occurs.
        //
        //   System.IO.EndOfStreamException:
        //     The end of the stream is reached.
        public override ushort ReadUInt16()
        {
            byte[] bytes = ReadBytes(sizeof(System.UInt16));
            return bitConverter.ToUInt16(bytes, 0);
        }

        // Summary:
        //     Reads a 4-byte unsigned integer from the current stream and advances the
        //     position of the stream by four bytes.
        //
        // Returns:
        //     A 4-byte unsigned integer read from this stream.
        //
        // Exceptions:
        //   System.ObjectDisposedException:
        //     The stream is closed.
        //
        //   System.IO.IOException:
        //     An I/O error occurs.
        //
        //   System.IO.EndOfStreamException:
        //     The end of the stream is reached.
        public override uint ReadUInt32()
        {
            byte[] bytes = ReadBytes(sizeof(System.UInt32));
            return bitConverter.ToUInt32(bytes, 0);
        }

        //
        // Summary:
        //     Reads an 8-byte unsigned integer from the current stream and advances the
        //     position of the stream by eight bytes.
        //
        // Returns:
        //     An 8-byte unsigned integer read from this stream.
        //
        // Exceptions:
        //   System.ObjectDisposedException:
        //     The stream is closed.
        //
        //   System.IO.IOException:
        //     An I/O error occurs.
        //
        //   System.IO.EndOfStreamException:
        //     The end of the stream is reached.
        public override ulong ReadUInt64()
        {
            byte[] bytes = ReadBytes(sizeof(System.UInt64));
            return bitConverter.ToUInt64(bytes, 0);
        }
    }
}
