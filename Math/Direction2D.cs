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
using System.Diagnostics;

namespace Numeric
{
	/// <summary>
	/// Description of Direction2D.
	/// </summary>
	public struct Direction2D : IComparable<Direction2D>
	{
	    private double deltaX;
	    private double deltaY;
		
	    #region Construction
	    
		public Direction2D(double deltaX, double deltaY)
		{
            //Debug.Assert(deltaX != 0.0 || deltaY != 0.0);
		    this.deltaX = deltaX;
		    this.deltaY = deltaY;
		}

        public Direction2D(Vector2D vector) :
            this(vector.DeltaX, vector.DeltaY)
		{
		}

        public Direction2D(Point2D fromPoint, Point2D toPoint) :
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

        /// <summary>
        /// The anti-clockwise angle from the x axis. Result in the range -pi to pi.
        /// </summary>
        public double Bearing
        {
            get { return Math.Atan2(deltaY, deltaX); }
        }

        /// <summary>
        /// Returns a new vector in the same direction of arbitrary magnitude.
        /// </summary>
        public Vector2D Vector
        {
            get { return new Vector2D(deltaX, deltaY); }
        }
	
		#endregion

        #region Methods
        /// <summary>
        /// If det (the determinant) is positive the angle between A (this) and B (rhs) is positive (counter-clockwise).
        /// If the determinant is negative the angle goes clockwise. Finally, if the determinant is 0, the
        /// vectors point in the same direction. In Schneider & Eberly this operator is called Kross
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public double Determinant(Direction2D rhs)
        {
            double determinant = this.deltaX * rhs.deltaY - this.deltaY * rhs.deltaX;
            return determinant;
        }

        /// <summary>
        /// If det (the determinant) is positive the angle between A (this) and B (rhs) is positive (counter-clockwise).
        /// If the determinant is negative the angle goes clockwise. Finally, if the determinant is 0, the
        /// vectors point in the same direction. In Schneider & Eberly this operator is called Kross
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public double Determinant(Vector2D rhs)
        {
            double determinant = this.deltaX * DeltaY - this.deltaY * DeltaX;
            return determinant;
        }

        public double Dot(Vector2D rhs)
        {
            double dotProduct = this.deltaX * rhs.DeltaX + this.deltaY * rhs.DeltaY;
            return dotProduct;
        }

        public double Dot(Direction2D rhs)
        {
            double dotProduct = this.deltaX * rhs.deltaX + this.deltaY * rhs.deltaY;
            return dotProduct;
        }
        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("({0}, {1})", DeltaX, DeltaY);
            return sb.ToString();
        }

        public override bool Equals(object other)
        {
            return other is Direction2D && Equals((Direction2D) other);
        }

        public bool Equals(Direction2D other)
        {
            return this.Bearing == other.Bearing;
        }

        public override int GetHashCode()
        {
            return deltaX.GetHashCode() ^ deltaY.GetHashCode();
        }

        public static bool operator ==(Direction2D lhs, Direction2D rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Direction2D lhs, Direction2D rhs)
        {
            return (lhs.deltaX != rhs.deltaX) || (lhs.deltaY != rhs.deltaY);
        }

		#region Operators
		public static Direction2D operator - (Direction2D rhs)
		{
            return new Direction2D(-rhs.deltaX, -rhs.deltaY);
		}
		
        /// <summary>
        /// Allows the Direction2D to be used like a unit vector in a scalar context
        /// </summary>
        /// <param name="rhs">A direction</param>
        /// <param name="lhs">A scalar distance</param>
        /// <returns>A unit vector of length the right-hand-side, parallel to the left-hand-side direction</returns>
        public static Vector2D operator * (Direction2D lhs, double rhs)
        {
            return lhs.Vector.Unit * rhs;
        }

        /// <summary>
        /// Allows the Direction2D to be used like a unit vector is a scalar context
        /// </summary>
        /// <param name="rhs">A scalar distance</param>
        /// <param name="lhs">A direction</param>
        /// <returns>A unit vector of length the left-hand-side, parallel to the right-hand-side direction</returns>
        public static Vector2D operator *(double lhs, Direction2D rhs)
        {
            return lhs * rhs.Vector.Unit;
        }
		#endregion

        #region IComparable<Direction2D> Members

        /// <summary>
        /// The direction comparison compares anti-clockwise angle with the x-axis. 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Direction2D other)
        {
            return this.Bearing.CompareTo(other.Bearing);
        }

        #endregion
    }
}
