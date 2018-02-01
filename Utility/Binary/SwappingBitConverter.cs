using System;
using System.Collections.Generic;
using System.Text;

using Wintellect.PowerCollections;

namespace Utility.Binary
{
    public class SwappingBitConverter : EndianBitConverter
    {
        public override byte[] GetBytes(double value)
        {
            byte[] result = BitConverter.GetBytes(value);
            Array.Reverse(result);
            return result;
        }

        public override byte[] GetBytes(float value)
        {
            byte[] result = BitConverter.GetBytes(value);
            Array.Reverse(result);
            return result;
        }

        public override byte[] GetBytes(ulong value)
        {
            byte[] result = BitConverter.GetBytes(value);
            Array.Reverse(result);
            return result;
        }

        public override byte[] GetBytes(uint value)
        {
            byte[] result = BitConverter.GetBytes(value);
            Array.Reverse(result);
            return result;
        }

        public override byte[] GetBytes(ushort value)
        {
            byte[] result = BitConverter.GetBytes(value);
            Array.Reverse(result);
            return result;
        }

        public override byte[] GetBytes(long value)
        {
            byte[] result = BitConverter.GetBytes(value);
            Array.Reverse(result);
            return result;
        }

        public override byte[] GetBytes(int value)
        {
            byte[] result = BitConverter.GetBytes(value);
            Array.Reverse(result);
            return result;
        }

        public override byte[] GetBytes(short value)
        {
            byte[] result = BitConverter.GetBytes(value);
            Array.Reverse(result);
            return result;
        }

        public override byte[] GetBytes(char value)
        {
            byte[] result = BitConverter.GetBytes(value);
            Array.Reverse(result);
            return result;
        }

        public override byte[] GetBytes(bool value)
        {
            byte[] result = BitConverter.GetBytes(value);
            Array.Reverse(result);
            return result;
        }

        public override bool ToBoolean(byte[] value, int startIndex)
        {
            // Can't swap one byte
            return BitConverter.ToBoolean(value, startIndex);
        }

        public override char ToChar(byte[] value, int startIndex)
        {
            int length = sizeof(System.Char);
            byte[] corrected = new byte[length];
            Algorithms.Copy(value, startIndex, corrected, 0, length);
            Array.Reverse(corrected);
            return BitConverter.ToChar(corrected, startIndex);
        }

        public override double ToDouble(byte[] value, int startIndex)
        {
            int length = sizeof(System.Double);
            byte[] corrected = new byte[length];
            Algorithms.Copy(value, startIndex, corrected, 0, length);
            Array.Reverse(corrected);
            return BitConverter.ToDouble(corrected, startIndex);
        }

        public override short ToInt16(byte[] value, int startIndex)
        {
            int length = sizeof(System.Int16);
            byte[] corrected = new byte[length];
            Algorithms.Copy(value, startIndex, corrected, 0, length);
            Array.Reverse(corrected);
            return BitConverter.ToInt16(corrected, startIndex);
        }

        public override int ToInt32(byte[] value, int startIndex)
        {
            int length = sizeof(System.Int32);
            byte[] corrected = new byte[length];
            Algorithms.Copy(value, startIndex, corrected, 0, length);
            Array.Reverse(corrected);
            return BitConverter.ToInt32(corrected, startIndex);
        }

        public override long ToInt64(byte[] value, int startIndex)
        {
            int length = sizeof(System.Int64);
            byte[] corrected = new byte[length];
            Algorithms.Copy(value, startIndex, corrected, 0, length);
            Array.Reverse(corrected);
            return BitConverter.ToInt64(corrected, startIndex);
        }

        public override float ToSingle(byte[] value, int startIndex)
        {
            int length = sizeof(System.Single);
            byte[] corrected = new byte[length];
            Algorithms.Copy(value, startIndex, corrected, 0, length);
            Array.Reverse(corrected);
            return BitConverter.ToSingle(corrected, startIndex);
        }

        public override string ToString(byte[] value, int startIndex)
        {
            // TODO: What is the length?
            byte[] corrected = new byte[sizeof(System.Int32)];
            Algorithms.Copy(value, startIndex, corrected, 0, sizeof(System.Int32));
            Array.Reverse(corrected);
            return BitConverter.ToString(corrected, startIndex);
        }

        public override string ToString(byte[] value)
        {
            // TODO: Swap the bytes in each two-byte character
            byte[] corrected = (byte[]) value.Clone();
            Array.Reverse(corrected);
            return BitConverter.ToString(corrected);
        }

        public override string ToString(byte[] value, int startIndex, int length)
        {
            // TODO: Swap the bytes in each two-byte character
            byte[] corrected = new byte[length];
            Algorithms.Copy(value, startIndex, corrected, 0, length);
            Array.Reverse(corrected);
            return BitConverter.ToString(corrected, startIndex, length);
        }

        public override ushort ToUInt16(byte[] value, int startIndex)
        {
            int length = sizeof(System.UInt16);
            byte[] corrected = new byte[length];
            Algorithms.Copy(value, startIndex, corrected, 0, length);
            Array.Reverse(corrected);
            return BitConverter.ToUInt16(corrected, startIndex);
        }

        public override uint ToUInt32(byte[] value, int startIndex)
        {
            int length = sizeof(System.UInt32);
            byte[] corrected = new byte[length];
            Algorithms.Copy(value, startIndex, corrected, 0, length);
            Array.Reverse(corrected);
            return BitConverter.ToUInt32(corrected, startIndex);
        }

        public override ulong ToUInt64(byte[] value, int startIndex)
        {
            int length = sizeof(System.UInt64);
            byte[] corrected = new byte[length];
            Algorithms.Copy(value, startIndex, corrected, 0, length);
            Array.Reverse(corrected);
            return BitConverter.ToUInt64(corrected, startIndex);
        }

        public override bool IsLittleEndian
        {
            get { return !BitConverter.IsLittleEndian; }
        }
    }
}
