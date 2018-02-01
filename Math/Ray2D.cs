using System;
using System.Collections.Generic;
using System.Text;

namespace Numeric
{
    public struct Ray2D
    {
        private Point2D source;
        private Direction2D direction;

        public Ray2D(Point2D source, Direction2D direction)
        {
            this.source = source;
            this.direction = direction;
        }

        public Point2D Source
        {
            get { return source; }
            set { source = value; }
        }

        public Direction2D Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        public Line2D SupportingLine
        {
            get
            {
                double a = -direction.DeltaY;
                double b = direction.DeltaX;
                double c = -a * source.X - b * source.Y;
                return new Line2D(a, b, c);
            }
        }

        public override bool Equals(object other)
        {
            return other is Ray2D && Equals((Ray2D) other);
        }

        public override int GetHashCode()
        {
            return source.GetHashCode() ^ direction.GetHashCode();
        }

        public bool Equals(Ray2D other)
        {
            return (this.source == other.source) && (this.direction == other.direction);
        }

        #region Operators
        public static bool operator ==(Ray2D lhs, Ray2D rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Ray2D lhs, Ray2D rhs)
        {
            return (lhs.source != rhs.source) || (lhs.direction != rhs.direction);
        }
        #endregion
    }
}
