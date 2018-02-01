/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-21
 * Time: 20:00
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Diagnostics;
using System.Text;

namespace Numeric
{
	/// <summary>
	/// Description of Point2D. 
	/// </summary>
	public struct Point2D
	{
	    private double x;
	    private double y;
	    
	    #region Constructors
	    
		public Point2D(double x, double y)
		{
		    this.x = x;
		    this.y = y;
		}
		
        public Point2D(Vector2D vector)
        {
            x = vector.DeltaX;
            y = vector.DeltaY;
        }

		#endregion
		
		#region Properties
		
		public double X
		{
		    get { return x;  }
		    set { x = value; }
		}
		
		public double Y
		{
		    get { return y;  }
		    set { y = value; }
		}
		
		#endregion
		
		#region Methods
		
		public Point2D Lerp(ref Point2D p, double t)
		{
		    return new Point2D(x + (p.x - x) * t,
		                       y + (p.y - y) * t);
		}
		
		#endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("({0}, {1})", X, Y);
            return sb.ToString();
        }

        public override bool Equals(object other)
        {
            return other is Point2D && Equals((Point2D) other);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }

        public bool Equals(Point2D other)
        {
            return (this.x == other.x) && (this.y == other.y);
        }

        #region Operators

        public static bool operator ==(Point2D lhs, Point2D rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Point2D lhs, Point2D rhs)
        {
            return (lhs.x != rhs.x) || (lhs.y != rhs.y);
        }

        /// <summary>
        /// Lexicographically compared on x and then y coordinates
        /// </summary>
        /// <param name="lhs">left hand side</param>
        /// <param name="rhs">right hand side</param>
        /// <returns>true if lhs is less than rhs, otherwise false</returns>
        public static bool operator <(Point2D lhs, Point2D rhs)
        {
            if (lhs.x < rhs.x)
            {
                return true;
            }
            if (lhs.x > rhs.x)
            {
                return false;
            }
            Debug.Assert(lhs.x == rhs.x);
            return lhs.y < rhs.y;
        }

        /// <summary>
        /// Lexicographically compared on x and then y coordinates
        /// </summary>
        /// <param name="lhs">left hand side</param>
        /// <param name="rhs">right hand side</param>
        /// <returns>true if lhs is greater than rhs, otherwise false</returns>
        public static bool operator >(Point2D lhs, Point2D rhs)
        {
            if (lhs.x > rhs.x)
            {
                return true;
            }
            if (lhs.x < rhs.x)
            {
                return false;
            }
            Debug.Assert(lhs.x == rhs.x);
            return lhs.y > rhs.y;
        }

		public static Point2D operator + (Point2D lhs, Vector2D rhs)
		{
		    return new Point2D(lhs.x + rhs.DeltaX, lhs.y + rhs.DeltaY);
		}
		
		public static Point2D operator - (Point2D lhs, Vector2D rhs)
		{
		    return new Point2D(lhs.x - rhs.DeltaX, lhs.y - rhs.DeltaY);
		}
		
		public static Vector2D operator - (Point2D lhs, Point2D rhs)
		{
		    return new Vector2D(lhs.x - rhs.x, lhs.y - rhs.y);
		}
		
		#endregion
	}
}
