using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Numeric;

namespace Geometry.RedBlue
{
    /// <summary>
    /// A 2D point with specific comparison behaviour for the red-blue intersection algorithm.
    /// </summary>
    public class FPoint :ISpatiallyComparable, IComparable<FPoint>
    {
        private Point2D point;

        public FPoint(Point2D point)
        {
            this.point = point;
        }

        public FPoint(double x, double y)
        {
            point = new Point2D(x, y);
        }

        public int CompareTo(ISpatiallyComparable obj)
        {
            // TODO: Refactor all this to use double-dispatch
            if (obj is Bundle || obj is Segment)
            {
                return -obj.CompareTo(this);
            }
            return CompareTo((FPoint) obj);
        }

        public int CompareTo(FPoint rhsPoint)
        {
            if (point.X == rhsPoint.X)
            {
                if (point.Y == rhsPoint.Y)
                {
                    return 0;
                }
                return Y < rhsPoint.Y ? -1 : 1;
            }
            return X < rhsPoint.X ? -1 : 1;
        }

        public Point2D Position
        {
            get { return point; }
        }

        public double X
        {
            get { return point.X; }
        }

        public double Y
        {
            get { return point.Y; }
        }

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj" />. Zero This instance is equal to <paramref name="obj" />. Greater than zero This instance is greater than <paramref name="obj" />. 
        /// </returns>
        /// <param name="obj">An object to compare with this instance. </param>
        /// <exception cref="T:System.ArgumentException"><paramref name="obj" /> is not the same type as this instance. </exception><filterpriority>2</filterpriority>
        public int CompareTo(object obj)
        {
            if (!(obj is ISpatiallyComparable))
            {
                throw new ArgumentException("Both arguments must be ISpatiallyComparable");
            }
            // TODO: Could we fall back on default comparer here?
            return CompareTo((ISpatiallyComparable) obj);
        }
    }
}
