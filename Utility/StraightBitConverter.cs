using System;
using System.Collections.Generic;
using System.Text;

namespace Utility.BitManipulation
{
    class StraightBitConverter : EndianBitConverter
    {
        public override byte[] GetBytes(double value)
        {
            return BitConverter.GetBytes(value);
        }

        public override byte[] GetBytes(float value)
        {
            return BitConverter.GetBytes(value);
        }

        public override byte[] GetBytes(ulong value)
        {
            return BitConverter.GetBytes(value);
        }

        public override byte[] GetBytes(uint value)
        {
            return BitConverter.GetBytes(value);
        }

        public override byte[] GetBytes(ushort value)
        {
            return BitConverter.GetBytes(value);
        }

        public override byte[] GetBytes(long value)
        {
            return BitConverter.GetBytes(value);
        }

        public override byte[] GetBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }

        public override byte[] GetBytes(short value)
        {
            return BitConverter.GetBytes(value);
        }

        public override byte[] GetBytes(char value)
        {
            return BitConverter.GetBytes(value);
        }

        public override byte[] GetBytes(bool value)
        {
            return BitConverter.GetBytes(value);
        }

        public override bool ToBoolean(byte[] value, int startIndex)
        {
            return BitConverter.ToBoolean(value, startIndex);
        }

        public override char ToChar(byte[] value, int startIndex)
        {
            return BitConverter.ToChar(value, startIndex);
        }

        public override double ToDouble(byte[] value, int startIndex)
        {
            return BitConverter.ToDouble(value, startIndex);
        }

        public override short ToInt16(byte[] value, int startIndex)
        {
            return BitConverter.ToInt16(value, startIndex);
        }

        public override int ToInt32(byte[] value, int startIndex)
        {
            return BitConverter.ToInt32(value, startIndex);
        }

        public override long ToInt64(byte[] value, int startIndex)
        {
            return BitConverter.ToInt64(value, startIndex);
        }

        public override float ToSingle(byte[] value, int startIndex)
        {
            return BitConverter.ToSingle(value, startIndex);
        }

        public override string ToString(byte[] value, int startIndex)
        {
            return BitConverter.ToString(value, startIndex);
        }

        public override string ToString(byte[] value)
        {
            return BitConverter.ToString(value, startIndex);
        }

        public override string ToString(byte[] value, int startIndex, int length)
        {
            return BitConverter.ToString(value, startIndex, length);
        }

        public override ushort ToUInt16(byte[] value, int startIndex)
        {
            return BitConverter.ToUInt16(value, startIndex);
        }

        public override uint ToUInt32(byte[] value, int startIndex)
        {
            return BitConverter.ToUInt32(value, startIndex);
        }

        public override ulong ToUInt64(byte[] value, int startIndex)
        {
            return BitConverter.ToUInt64(value, startIndex);
        }

        public override bool IsLittleEndian
        {
            get { return BitConverter.IsLittleEndian; }
        }
    }
}
