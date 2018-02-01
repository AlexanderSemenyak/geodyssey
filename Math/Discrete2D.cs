using System;
using System.Collections.Generic;
using System.Text;

namespace Numeric
{
    /// <summary>
    /// Description of Discrete2D. 
    /// </summary>
    public struct Discrete2D
    {
        private int i;
        private int j;

        #region Constructors

        public Discrete2D(int i, int j)
        {
            this.i = i;
            this.j = j;
        }

        #endregion

        #region Properties

        public int I
        {
            get { return i; }
            set { i = value; }
        }

        public int J
        {
            get { return j; }
            set { j = value; }
        }

        public override bool Equals(object other)
        {
            return other is Discrete2D && Equals((Discrete2D) other);
        }

        public override int GetHashCode()
        {
            return i.GetHashCode() ^ j.GetHashCode();
        }

        public bool Equals(Discrete2D other)
        {
            return (this.i == other.i) && (this.j == other.j);
        }

        public static bool operator ==(Discrete2D lhs, Discrete2D rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Discrete2D lhs, Discrete2D rhs)
        {
            return (lhs.i != rhs.i) || (lhs.j != rhs.j);
        }

        public static Discrete2D operator +(Discrete2D lhs, Discrete2D rhs)
        {
            return new Discrete2D(lhs.i + rhs.i, lhs.j + rhs.j);
        }

        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("({0}, {1})", I, J);
            return sb.ToString();
        }

        public Discrete2D North()
        {
            return new Discrete2D(i, j + 1);
        }

        public Discrete2D South()
        {
            return new Discrete2D(i, j - 1);
        }

        public Discrete2D East()
        {
            return new Discrete2D(i + 1, j);
        }

        public Discrete2D West()
        {
            return new Discrete2D(i - 1, j);
        }
    }
}
