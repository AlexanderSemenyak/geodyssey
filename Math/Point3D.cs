using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Numeric
{
    public struct Point3D
    {
        private double x;
        private double y;
        private double z;

        #region Constructors

        public Point3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        #endregion

        public double X
        {
            get { return x;  }
            set { x = value; }
        }

        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        public double Z
        {
            get { return z; }
            set { z = value; }
        }

        public Point2D XY
        {
            get { return new Point2D(x, y); }
            set
            {
                x = value.X;
                y = value.Y;
            }
        }

        public Point2D XZ
        {
            get { return new Point2D(x, z); }
            set
            {
                x = value.X;
                z = value.Y;
            }
        }

        public Point2D YZ
        {
            get { return new Point2D(y, z); }
            set
            {
                y = value.X;
                z = value.Y;
            }
        }

        #region Methods

        public Point3D Lerp(ref Point3D p, double t)
        {
            return new Point3D(x + (p.x - x) * t,
                               y + (p.y - y) * t,
                               z + (p.z - z) * t);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("({0}, {1}, {2})", X, Y, Z);
            return sb.ToString();
        }

        public override bool Equals(object other)
        {
            return other is Point3D && Equals((Point3D) other);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }

        public bool Equals(Point3D other)
        {
            return (this.x == other.x) && (this.y == other.y) && (this.z == other.z);
        }

        #endregion

        #region Operators

        public static bool operator ==(Point3D lhs, Point3D rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Point3D lhs, Point3D rhs)
        {
            return (lhs.x != rhs.x) || (lhs.y != rhs.y) || (lhs.z != rhs.z);
        }

        public static Point3D operator +(Point3D lhs, Vector3D rhs)
        {
            return new Point3D(lhs.x + rhs.DeltaX,
                               lhs.y + rhs.DeltaY,
                               lhs.z + rhs.DeltaZ);
        }

        public static Point3D operator -(Point3D lhs, Vector3D rhs)
        {
            return new Point3D(lhs.x - rhs.DeltaX,
                               lhs.y - rhs.DeltaY,
                               lhs.z - rhs.DeltaZ);
        }

        public static Vector3D operator -(Point3D lhs, Point3D rhs)
        {
            return new Vector3D(lhs.x - rhs.x,
                                lhs.y - rhs.y,
                                lhs.z - rhs.z);
        }

        #endregion
    }
}
