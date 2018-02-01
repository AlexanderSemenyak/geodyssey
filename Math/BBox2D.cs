using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Numeric
{
    /// <summary>
    /// Immutable 2D bounding box
    /// </summary>
    public struct BBox2D
    {
        Point2D min;
        Point2D max;

        /// <summary>
        /// Create a bounding box covering the sequence of points
        /// </summary>
        /// <param name="points">A sequence of points</param>
        /// <exception cref="ArgumentNullException">Thrown if points is null</exception>
        /// <exception cref="InvalidOperationException">Thrown if points is an empty sequence</exception>
        public static BBox2D FromPoints(IEnumerable<Point2D> points)
        {
            return FromPoints(points, 0.0);
        }

        /// <summary>
        /// Create a bounding box covering the sequence of points with a border
        /// </summary>
        /// <param name="points">A sequence of points</param>
        /// <param name="border">A border distance around the data</param>
        /// <exception cref="ArgumentNullException">Thrown if points is null</exception>
        /// <exception cref="InvalidOperationException">Thrown if points is an empty sequence</exception>
        public static BBox2D FromPoints(IEnumerable<Point2D> points, double border)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            double minX = points.First().X;
            double minY = points.First().Y;
            double maxX = minX;
            double maxY = minY;

            foreach (Point2D point in points)
            {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }
            return new BBox2D(minX - border, minY - border, maxX + border, maxY + border);
        }

        public BBox2D(Point2D min, Point2D max)
        {
            this.min = min;
            this.max = max;
        }

        public BBox2D(double xmin, double ymin, double xmax, double ymax) :
            this(new Point2D(xmin, ymin), new Point2D(xmax, ymax))
        {
        }

        public Point2D Min
        {
            get { return min; }
        }

        public Point2D Max
        {
            get { return max; }
        }

        public double XMin
        {
            get { return min.X; }
        }

        public double XMax
        {
            get { return max.X; }
        }

        public double YMin
        {
            get { return min.Y; }
        }

        public double YMax
        {
            get { return max.Y; }
        }

        /// <summary>
        /// Determines whether there is overlap between this bounding box and another
        /// </summary>
        /// <param name="other">A bounding box</param>
        /// <returns>True if the bounding boxes overlap, otherwise false</returns>
        public bool IsIntersection(BBox2D other)
        {
            if (XMax < other.XMin || other.XMax < XMin)
            {
                return false;
            }

            if (YMax < other.YMin || other.YMax < YMin)
            {
                return false;
            }

            return true;
        }

        public override bool Equals(object other)
        {
            return other is BBox2D && Equals((BBox2D) other);
        }

        public bool Equals(BBox2D other)
        {
            return (XMin == other.XMin) && (XMax == other.XMax)
                && (YMin == other.YMin) && (YMax == other.YMax);
        }

        public override int GetHashCode()
        {
            return min.GetHashCode() ^ max.GetHashCode();
        }

        public static bool operator ==(BBox2D lhs, BBox2D rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(BBox2D lhs, BBox2D rhs)
        {
            return !lhs.Equals(rhs);
        }

        #region Operators
        public static BBox2D operator +(BBox2D lhs, BBox2D rhs)
        {
            return new BBox2D(Math.Min(lhs.XMin, rhs.XMin),
                              Math.Min(lhs.YMin, rhs.YMin),
                              Math.Max(lhs.XMax, rhs.XMax),
                              Math.Max(lhs.YMax, rhs.YMax));
        }
        #endregion
    }
}
