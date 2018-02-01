using System;
using System.Text;

namespace Numeric
{
	/// <summary>
	/// Description of Vector3D.
	/// </summary>
	public struct Vector3D
	{
	    private double deltaX;
	    private double deltaY;
	    private double deltaZ;
		
	    #region Construction
	    
		public Vector3D(double deltaX, double deltaY, double deltaZ)
		{
		    this.deltaX = deltaX;
		    this.deltaY = deltaY;
		    this.deltaZ = deltaZ;
		}
		
		public Vector3D(Point3D point)
		{
		    this.deltaX = point.X;
		    this.deltaY = point.Y;
            this.deltaZ = point.Z;
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
            get { return deltaZ; }
            set { deltaZ = value; }
        }

	    public Vector2D DeltaXY
	    {
            get { return new Vector2D(deltaX, deltaY); }
            set
            {
                deltaX = value.DeltaX;
                deltaY = value.DeltaY;
            }
	    }

        public Vector2D DeltaXZ
        {
            get { return new Vector2D(deltaX, deltaZ); }
            set
            {
                deltaX = value.DeltaX;
                deltaZ = value.DeltaY;
            }
        }

        public Vector2D DeltaYZ
        {
            get { return new Vector2D(deltaY, deltaZ); }
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
                double length = Magnitude;
                if (length == 0.0)
                {
                    throw new DivideByZeroException("Zero-length vector cannot be normalized.");
                }
                return deltaX / length;
            }
        }

        public double Beta
        {
            get
            {
                double length = Magnitude;
                if (length == 0.0)
                {
                    throw new DivideByZeroException("Zero-length vector cannot be normalized.");
                }
                return deltaY / length;
            }
        }

        public double Gamma
        {
            get
            {
                double length = Magnitude;
                if (length == 0.0)
                {
                    throw new DivideByZeroException("Zero-length vector cannot be normalized.");
                }
                return deltaZ / length;
            }
        }

		public double Magnitude2
		{
		    get
		    {
		        return deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
		    }
		}
		
		public double Magnitude
		{
		    get
		    {
		        return Math.Sqrt(Magnitude2);
		    }
		}
		
		public Vector3D Unit
		{
		    get
		    {
		        double length = Magnitude;
                if (length == 0.0)
                {
                    throw new DivideByZeroException("Zero-length vector cannot be normalized.");
                }
		        return new Vector3D(deltaX / length, deltaY / length, deltaZ / length);
		    }
		}
		
		#endregion
		
		#region Methods
		public double Dot(Vector3D rhs)
		{
            double dotProduct = this.deltaX * rhs.deltaX + this.deltaY * rhs.deltaY + this.deltaZ * rhs.deltaZ;
            return dotProduct;
		}

        public double Dot(Direction3D rhs)
        {
            double dotProduct = this.deltaX * rhs.DeltaX + this.deltaY * rhs.DeltaY + this.deltaZ * rhs.DeltaZ;
            return dotProduct;
        }
		#endregion

        public Vector3D Cross(Vector3D rhs)
        {
            return new Vector3D(deltaY*rhs.deltaZ - deltaX*rhs.deltaY,
                                deltaX*rhs.deltaY - deltaX*rhs.deltaZ,
                                deltaX*rhs.deltaY - deltaY*rhs.deltaY);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("({0}, {1}, {2})", DeltaX, DeltaY, DeltaZ);
            return sb.ToString();
        }

        public override bool Equals(object other)
        {
            return other is Vector3D && Equals((Vector3D) other);
        }

        public bool Equals(Vector3D other)
        {
            return (this.deltaX == other.deltaX) && (this.deltaY == other.deltaY) && (this.deltaZ == other.deltaZ);
        }

        public override int GetHashCode()
        {
            return deltaX.GetHashCode() ^ deltaY.GetHashCode() ^ deltaZ.GetHashCode();
        }

        public static bool operator ==(Vector3D lhs, Vector3D rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Vector3D lhs, Vector3D rhs)
        {
            return (lhs.deltaX != rhs.deltaX) || (lhs.deltaY != rhs.deltaY) || (lhs.deltaZ != rhs.deltaZ);
        }

		#region Operators
		public static Vector3D operator + (Vector3D lhs, Vector3D rhs)
		{
            return new Vector3D(lhs.deltaX + rhs.deltaX, lhs.deltaY + rhs.deltaY, lhs.deltaZ + rhs.deltaZ);
		}

        public static Vector3D operator -(Vector3D rhs)
        {
            return new Vector3D(-rhs.deltaX, -rhs.deltaY, -rhs.deltaZ);
        }

        public static Vector3D operator -(Vector3D lhs, Vector3D rhs)
        {
            return new Vector3D(lhs.deltaX - rhs.deltaX, lhs.deltaY - rhs.deltaY, lhs.deltaZ - rhs.deltaZ);
        }

		public static Vector3D operator * (Vector3D lhs, double rhs)
		{
		    return new Vector3D(lhs.deltaX * rhs, lhs.deltaY * rhs, lhs.deltaZ * rhs);
		}

        public static Vector3D operator *(double rhs, Vector3D lhs)
        {
            return new Vector3D(rhs * lhs.deltaX, rhs * lhs.deltaY, rhs * lhs.deltaZ);
        }
		
		public static Vector3D operator / (Vector3D lhs, double rhs)
		{
            return new Vector3D(lhs.deltaX / rhs, lhs.deltaY / rhs, lhs.deltaZ / rhs);
		}
		
		#endregion
	}
}
