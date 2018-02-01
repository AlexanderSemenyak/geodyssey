/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-22
 * Time: 23:10
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Text;

namespace Numeric
{
	/// <summary>
	/// Description of Vector2D.
	/// </summary>
	public struct Vector2D
	{
	    private double deltaX;
	    private double deltaY;
		
	    #region Construction
	    
		public Vector2D(double deltaX, double deltaY)
		{
		    this.deltaX = deltaX;
		    this.deltaY = deltaY;
		}
		
		public Vector2D(Point2D point)
		{
		    this.deltaX = point.X;
		    this.deltaY = point.Y;
		}
		
        public Vector2D(Vector2D other)
        {
            deltaX = other.deltaX;
            deltaY = other.deltaY;
        }

        public Vector2D(Direction2D direction, double magnitude) :
            this(direction.Vector.Unit * magnitude)
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
		
		public double Magnitude2
		{
		    get
		    {
		        return deltaX * deltaX + deltaY * deltaY;
		    }
		}
		
		public double Magnitude
		{
		    get
		    {
		        return Math.Sqrt(Magnitude2);
		    }
		}
		
		public Vector2D Unit
		{
		    get
		    {
		        double length = Magnitude;
                if (length == 0.0)
                {
                    throw new DivideByZeroException("Zero-length vector cannot be normalized.");
                }
                return new Vector2D(deltaX / length, deltaY / length);
		    }
		}
		
	    public Direction2D Direction
	    {
	        get
	        {
	            return new Direction2D(this);
	        }
	    }

		#endregion
		
		public double Dot(Vector2D rhs)
		{
		    double dotProduct = this.deltaX * rhs.deltaX + this.deltaY * rhs.deltaY;
            return dotProduct;
		}

        public double Dot(Direction2D rhs)
        {
            double dotProduct = this.deltaX * rhs.DeltaX + this.deltaY * rhs.DeltaY;
            return dotProduct;
        }

        /// <summary>
        /// Anticlockwise perpendicular vector.
        /// </summary>
        public Vector2D Perp
        {
            get
            {
                return new Vector2D(-deltaY, deltaX);
            }
        }

        /// <summary>
        /// If det (the determinant) is positive the angle between A (this) and B (rhs) is positive (counter-clockwise).
        /// If the determinant is negative the angle goes clockwise. Finally, if the determinant is 0, the
        /// vectors point in the same direction. In Schneider & Eberly this operator is called Kross.
        /// Is is also often known as PerpDot.
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public double Determinant(Vector2D rhs)
        {
            double determinant = this.deltaX * rhs.deltaY - this.deltaY * rhs.deltaX;
            return determinant;
        }

        /// <summary>
        /// The angle between two vectors
        /// </summary>
        /// <param name="rhs">The through which this vector is conceptually rotated to obtain the angle</param>
        /// <returns>The DECIDE! clockwise/anticlockwise angle between the vectors</returns>
        public double Angle(Vector2D rhs)
        {
            // TODO: Does this return the clockwise angle, or the anticlockwise angle
            return Math.Atan2(Math.Abs(Determinant(rhs)), Dot(rhs));    
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("({0}, {1})", DeltaX, DeltaY);
            return sb.ToString();
        }

        public override bool Equals(object other)
        {
            return other is Vector2D && Equals((Vector2D) other);
        }

        public bool Equals(Vector2D other)
        {
            return (this.deltaX == other.deltaX) && (this.deltaY == other.deltaY);
        }

        public override int GetHashCode()
        {
            return deltaX.GetHashCode() ^ deltaY.GetHashCode();
        }

        public static bool operator ==(Vector2D lhs, Vector2D rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Vector2D lhs, Vector2D rhs)
        {
            return (lhs.deltaX != rhs.deltaX) || (lhs.deltaY != rhs.deltaY);
        }

		#region Operators
		public static Vector2D operator + (Vector2D lhs, Vector2D rhs)
		{
		    return new Vector2D(lhs.deltaX + rhs.deltaX, lhs.deltaY + rhs.deltaY);
		}

        public static Vector2D operator -(Vector2D rhs)
        {
            return new Vector2D(-rhs.deltaX, -rhs.deltaY);
        }

        public static Vector2D operator -(Vector2D lhs, Vector2D rhs)
        {
            return new Vector2D(lhs.deltaX - rhs.deltaX, lhs.deltaY - rhs.deltaY);
        }

		public static Vector2D operator * (Vector2D lhs, double rhs)
		{
		    return new Vector2D(lhs.deltaX * rhs, lhs.deltaY * rhs);
		}

        public static Vector2D operator *(double rhs, Vector2D lhs)
        {
            return new Vector2D(rhs * lhs.deltaX, rhs * lhs.deltaY);
        }
		
		public static Vector2D operator / (Vector2D lhs, double rhs)
		{
		    return new Vector2D(lhs.deltaX / rhs, lhs.deltaY / rhs);
		}
		
		#endregion
	}
}
