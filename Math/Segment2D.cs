using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Numeric
{
    public struct Segment2D
    {
        private Point2D source;
        private Point2D target;

        public Segment2D(Point2D source, Point2D target)
        {
            this.source = source;
            this.target = target;
        }

        public Segment2D(Point2D source, Vector2D vector) :
            this(source, source + vector)
        {
        }

        public Point2D Source
        {
            get { return source; }
            set { source = value; }
        }

        public Point2D Target
        {
            get { return target; }
            set { target = value; }
        }

        public Vector2D Vector
        {
            get { return target - source; }
        }

        public Direction2D Direction
        {
            get { return new Direction2D(source, target); }
        }

        public Line2D SupportingLine
        {
            get
            {
                double a = source.Y - target.Y;
                double b = target.X - source.X;
                double c = -a * source.X - b * source.Y;
                return new Line2D(a, b, c);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Segment2D && Equals((Segment2D) obj);
        }

        public bool Equals(Segment2D rhs)
        {
            return (source == rhs.source) && (target == rhs.target);
        }

        public override int GetHashCode()
        {
            return source.GetHashCode() ^ target.GetHashCode();
        }

        public static bool operator ==(Segment2D lhs, Segment2D rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Segment2D lhs, Segment2D rhs)
        {
            return (lhs.source != rhs.source) || (lhs.target != rhs.target);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} ->- {1}", source, target);
            return sb.ToString();
        }
    }
}
