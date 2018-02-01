using System;
using System.Collections.Generic;
using System.Text;

namespace Utility.BitManipulation
{
    public abstract class EndianBitConverter
    {
        public static EndianBitConverter CreateForLittleEndianWords()
        {
            if (BitConverter.IsLittleEndian())
            {
                return new StraightBitConverter();
            }
            return SwappingBitConverter();
        }

        public static EndianBitConverter CreateForBigEngianWords()
        {
            if (BitConverter.IsLittleEndian())
            {
                return new SwappingBitConverter();
            }
            return StraightBitConverter();
        }

        public long DoubleToInt64(double value)
        {
            return BitConverter.DoubleToInt64Bits(value);
        }

        public double Int64BitsToDouble(long value)
        {
            return BitConverter.Int64BitsToDouble(value);
        }

        public abstract byte[] GetBytes(double value);
        public abstract byte[] GetBytes(float value);
        public abstract byte[] GetBytes(ulong value);
        public abstract byte[] GetBytes(uint value);
        public abstract byte[] GetBytes(ushort value);
        public abstract byte[] GetBytes(long value);
        public abstract byte[] GetBytes(int value);
        public abstract byte[] GetBytes(short value);
        public abstract byte[] GetBytes(char value);
        public abstract byte[] GetBytes(bool value);

        public abstract bool ToBoolean(byte[] value, int startIndex);
        public abstract char ToChar(byte[] value, int startIndex);
        public abstract double ToDouble(byte[] value, int startIndex);
        public abstract short ToInt16(byte[] value, int startIndex);
        public abstract int ToInt32(byte[] value, int startIndex);
        public abstract long ToInt64(byte[] value, int startIndex);
        public abstract float ToSingle(byte[] value, int startIndex);
        public abstract string ToString(byte[] value, int startIndex);
        public abstract string ToString(byte[] value);
        public abstract string ToString(byte[] value, int startIndex, int length);
        public abstract ushort ToUInt16(byte[] value, int startIndex);
        public abstract uint ToUInt32(byte[] value, int startIndex);
        public abstract ulong ToUInt64(byte[] value, int startIndex);

        public abstract bool IsLittleEndian
        {
            get;
        }
    }
}
