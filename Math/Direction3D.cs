using System;
using System.Text;

namespace Numeric
{
    public struct Direction3D
    {
        private double deltaX;
	    private double deltaY;
        private double deltaZ;
		
	    #region Construction
	    
		public Direction3D(double deltaX, double deltaY, double deltaZ)
		{
            //Debug.Assert(deltaX != 0.0 || deltaY != 0.0);
		    this.deltaX = deltaX;
		    this.deltaY = deltaY;
		    this.deltaZ = deltaZ;
		}

        public Direction3D(Vector3D point) :
            this(point.DeltaX, point.DeltaY, point.DeltaZ)
		{
		}

        public Direction3D(Point3D fromPoint, Point3D toPoint) :
            this(toPoint - fromPoint)
        {
        }

		#endregion
		
		#region Properties
		
		public double DeltaX
		{
		    get { return deltaX;  }
		    set { deltaX = value; }
		}
		
		public double DeltaY
		{
		    get { return deltaY;  }
		    set { deltaY = value; }
		}

        public double DeltaZ
        {
            get { return deltaY; }
            set { deltaY = value; }
        }

        public Direction2D DeltaXY
        {
            get { return new Direction2D(deltaX, deltaY); }
            set
            {
                deltaX = value.DeltaX;
                deltaY = value.DeltaY;
            }
        }

        public Direction2D DeltaXZ
        {
            get { return new Direction2D(deltaX, deltaZ); }
            set
            {
                deltaX = value.DeltaX;
                deltaZ = value.DeltaY;
            }
        }

        public Direction2D DeltaYZ
        {
            get { return new Direction2D(deltaY, deltaZ); }
            set
            {
                deltaX = value.DeltaX;
                deltaZ = value.DeltaY;
            }
        }

        public double Alpha
        {
            get
            {
                return deltaX / Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
            }
        }

        public double Beta
        {
            get
            {
                return deltaY / Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
            }
        }

        public double Gamma
        {
            get
            {
                return deltaZ / Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
            }
        }

        /// <summary>
        /// The anti-clockwise angle from the x axis in the XY subspace. Result in the range -pi to pi.
        /// </summary>
        public double Bearing
        {
            get { return Math.Atan2(deltaY, deltaX); }
        }

        /// <summary>
        /// Returns a new vector in the same direction of arbitrary magnitude.
        /// </summary>
        public Vector3D Vector
        {
            get { return new Vector3D(deltaX, deltaY, deltaZ); }
        }
	
		#endregion

        #region Methods
        public Direction3D Cross(Direction3D rhs)
        {
            return new Direction3D(deltaY*rhs.deltaZ - deltaX*rhs.deltaY,
                                   deltaX*rhs.deltaY - deltaX*rhs.deltaZ,
                                   deltaX*rhs.deltaY - deltaY*rhs.deltaY);
        }

        public double Dot(Vector3D rhs)
        {
            double dotProduct = this.deltaX * rhs.DeltaX + this.deltaY * rhs.DeltaY + this.deltaZ * rhs.DeltaZ;
            return dotProduct;
        }

        public double Dot(Direction3D rhs)
        {
            double dotProduct = this.deltaX * rhs.deltaX + this.deltaY * rhs.deltaY + this.deltaZ * rhs.deltaZ;
            return dotProduct;
        }
        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("({0}, {1}, {2})", DeltaX, DeltaY, DeltaZ);
            return sb.ToString();
        }

        public override bool Equals(object other)
        {
            return other is Direction3D && Equals((Direction3D) other);
        }

        public bool Equals(Direction3D other)
        {
            return Math.Sign(deltaX) == Math.Sign(other.deltaX)
                && Math.Sign(deltaY) == Math.Sign(other.deltaY)
                && Math.Sign(deltaZ) == Math.Sign(other.deltaZ)
                && DeltaXY.Determinant(other.DeltaXY) == 0.0
                && DeltaXZ.Determinant(other.DeltaXZ) == 0.0
                && DeltaYZ.Determinant(other.DeltaYZ) == 0.0;
        }

        public override int GetHashCode()
        {
            return deltaX.GetHashCode() ^ deltaY.GetHashCode();
        }

        public static bool operator ==(Direction3D lhs, Direction3D rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Direction3D lhs, Direction3D rhs)
        {
            return !lhs.Equals(rhs);
        }

		#region Operators
		public static Direction3D operator - (Direction3D rhs)
		{
            return new Direction3D(-rhs.deltaX, -rhs.deltaY, -rhs.deltaZ);
		}
		
        /// <summary>
        /// Allows the Direction2D to be used like a unit vector in a scalar context
        /// </summary>
        /// <param name="rhs">A direction</param>
        /// <param name="lhs">A scalar distance</param>
        /// <returns>A unit vector of length the right-hand-side, parallel to the left-hand-side direction</returns>
        public static Vector3D operator * (Direction3D lhs, double rhs)
        {
            return lhs.Vector.Unit * rhs;
        }

        /// <summary>
        /// Allows the Direction2D to be used like a unit vector is a scalar context
        /// </summary>
        /// <param name="rhs">A scalar distance</param>
        /// <param name="lhs">A direction</param>
        /// <returns>A unit vector of length the left-hand-side, parallel to the right-hand-side direction</returns>
        public static Vector3D operator *(double lhs, Direction3D rhs)
        {
            return lhs * rhs.Vector.Unit;
        }
		#endregion
    }
}