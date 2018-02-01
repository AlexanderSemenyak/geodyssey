using System;
using System.Collections.Generic;
using System.Text;

using Wintellect.PowerCollections;

namespace Utility.BitManipulation
{
    class SwappingBitConverter : EndianBitConverter
    {
        public override byte[] GetBytes(double value)
        {
            return Array.Reverse(Array.Reverse(BitConverter.GetBytes(value)));
        }

        public override byte[] GetBytes(float value)
        {
            return Array.Reverse(BitConverter.GetBytes(value));
        }

        public override byte[] GetBytes(ulong value)
        {
            return Array.Reverse(BitConverter.GetBytes(value));
        }

        public override byte[] GetBytes(uint value)
        {
            return Array.Reverse(BitConverter.GetBytes(value));
        }

        public override byte[] GetBytes(ushort value)
        {
            return Array.Reverse(BitConverter.GetBytes(value));
        }

        public override byte[] GetBytes(long value)
        {
            return Array.Reverse(BitConverter.GetBytes(value));
        }

        public override byte[] GetBytes(int value)
        {
            return Array.Reverse(BitConverter.GetBytes(value));
        }

        public override byte[] GetBytes(short value)
        {
            return Array.Reverse(BitConverter.GetBytes(value));
        }

        public override byte[] GetBytes(char value)
        {
            return Array.Reverse(BitConverter.GetBytes(value));
        }

        public override byte[] GetBytes(bool value)
        {
            return Array.Reverse(BitConverter.GetBytes(value));
        }

        public override bool ToBoolean(byte[] value, int startIndex)
        {
            byte[] corrected = value.Clone();
            Array.Reverse(corrected);
            return BitConverter.ToBoolean(corrected, startIndex);
        }

        public override char ToChar(byte[] value, int startIndex)
        {
            byte[] corrected = new byte[sizeof(System.Int32)];
            Algorithms.Copy(value, startIndex, corrected, 0, sizeof(System.Int32));
            Array.Reverse(corrected);
            return BitConverter.ToChar(corrected, startIndex);
        }

        public override double ToDouble(byte[] value, int startIndex)
        {
            byte[] corrected = new byte[sizeof(System.Int32)];
            Algorithms.Copy(value, startIndex, corrected, 0, sizeof(System.Int32));
            Array.Reverse(corrected);
            return BitConverter.ToDouble(corrected, startIndex);
        }

        public override short ToInt16(byte[] value, int startIndex)
        {
            byte[] corrected = new byte[sizeof(System.Int32)];
            Algorithms.Copy(value, startIndex, corrected, 0, sizeof(System.Int32));
            Array.Reverse(corrected);
            return BitConverter.ToInt16(corrected, startIndex);
        }

        public override int ToInt32(byte[] value, int startIndex)
        {
            byte[] corrected = new byte[sizeof(System.Int32)];
            Algorithms.Copy(value, startIndex, corrected, 0, sizeof(System.Int32));
            Array.Reverse(corrected);
            return BitConverter.ToInt32(corrected, startIndex);
        }

        public override long ToInt64(byte[] value, int startIndex)
        {
            byte[] corrected = new byte[sizeof(System.Int32)];
            Algorithms.Copy(value, startIndex, corrected, 0, sizeof(System.Int32));
            Array.Reverse(corrected);
            return BitConverter.ToInt64(corrected, startIndex);
        }

        public override float ToSingle(byte[] value, int startIndex)
        {
            byte[] corrected = new byte[sizeof(System.Int32)];
            Algorithms.Copy(value, startIndex, corrected, 0, sizeof(System.Int32));
            Array.Reverse(corrected);
            return BitConverter.ToSingle(corrected, startIndex);
        }

        public override string ToString(byte[] value, int startIndex)
        {
            byte[] corrected = new byte[sizeof(System.Int32)];
            Algorithms.Copy(value, startIndex, corrected, 0, sizeof(System.Int32));
            Array.Reverse(corrected);
            return BitConverter.ToString(corrected, startIndex);
        }

        public override string ToString(byte[] value)
        {
            byte[] corrected = new byte[sizeof(System.Int32)];
            Algorithms.Copy(value, startIndex, corrected, 0, sizeof(System.Int32));
            Array.Reverse(corrected);
            return BitConverter.ToString(corrected, startIndex);
        }

        public override string ToString(byte[] value, int startIndex, int length)
        {
            byte[] corrected = new byte[sizeof(System.Int32)];
            Algorithms.Copy(value, startIndex, corrected, 0, sizeof(System.Int32));
            Array.Reverse(corrected);
            return BitConverter.ToString(corrected, startIndex, length);
        }

        public override ushort ToUInt16(byte[] value, int startIndex)
        {
            byte[] corrected = new byte[sizeof(System.Int32)];
            Algorithms.Copy(value, startIndex, corrected, 0, sizeof(System.Int32));
            Array.Reverse(corrected);
            return BitConverter.ToUInt16(corrected, startIndex);
        }

        public override uint ToUInt32(byte[] value, int startIndex)
        {
            byte[] corrected = new byte[sizeof(System.Int32)];
            Algorithms.Copy(value, startIndex, corrected, 0, sizeof(System.Int32));
            Array.Reverse(corrected);
            return BitConverter.ToUInt32(corrected, startIndex);
        }

        public override ulong ToUInt64(byte[] value, int startIndex)
        {
            byte[] corrected = new byte[sizeof(System.Int32)];
            Algorithms.Copy(value, startIndex, corrected, 0, sizeof(System.Int32));
            Array.Reverse(corrected);
            return BitConverter.ToUInt64(corrected, startIndex);
        }

        public override bool IsLittleEndian
        {
            get { return !BitConverter.IsLittleEndian; }
        }
    }
}
