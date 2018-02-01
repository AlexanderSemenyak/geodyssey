using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Numeric
{
    public struct Line2D
    {
        private double a;
        private double b;
        private double c;

        /// <summary>
        /// Determine the general equation of a line ax + by + c = 0 specififed by two, non-conincident
        /// points on the line.
        /// </summary>
        /// <param name="p">The first point on the line</param>
        /// <param name="q">The second point on the line</param>
        /// <param name="a">The coefficient of x</param>
        /// <param name="b">The coefficient of y</param>
        /// <param name="c">The constant</param>
        public Line2D(Point2D p, Point2D q)
        {
            if (p == q)
            {
                throw new ArgumentException("Attempt to create a degenerate line");
            }
            Debug.Assert(p != q);

            if (p.Y == q.Y) // Horizontal lines
            {
                a = 0;
                if (q.X > p.X)
                {
                    b = 1;
                    c = -p.Y;
                }
                else if (q.X == p.X)
                {
                    b = 0;
                    c = 0;
                }
                else
                {
                    b = -1;
                    c = p.Y;
                }
            }
            else if (q.X == p.X) // Vertical lines
            {
                b = 0;
                if (q.Y > p.Y)
                {
                    a = -1;
                    c = p.X;
                }
                else if (q.Y == p.Y)
                {
                    a = 0;
                    c = 0;
                }
                else
                {
                    a = 1;
                    c = -p.X;
                }
            }
            else // General lines
            {
                a = p.Y - q.Y;
                b = q.X - p.X;
                c = -p.X * a - p.Y * b;
            }
        }

        public Line2D(double a, double b, double c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public double A
        {
            get { return a; }
        }

        public double B
        {
            get { return b; }
        }

        public double C
        {
            get { return c; }
        }

        public bool IsHorizontal
        {
            get { return a == 0.0 && b != 0.0; }
        }

        public bool IsVertical
        {
            get { return b == 0.0 && a != 0; }
        }

        public Line2D Opposite
        {
            get
            {
                return new Line2D(-A, -B, -C);
            }
        }

        public Direction2D Normal
        {
            get { return new Direction2D(A, B); }
        }

        public Line2D Perpendicular
        {
            get
            {
                return new Line2D(B, -A, C);
            }
        }

        public double SolveForX(double y)
        {
            Debug.Assert(a != 0.0);
            return -b * y / a - c / a;
        }

        public double SolveForY(double x)
        {
            Debug.Assert(b != 0.0);
            return -a * x / b - c / b;   
        }

        /// <summary>
        /// Signed distance to line.
        /// </summary>
        /// <param name="p">The query point</param>
        /// <returns>The signed shortest distance to this line</returns>
        public double DistanceTo(Point2D p)
        {
            return (p.X * A + p.Y * B + C) / Math.Sqrt(A * A + B * B);
        }

        public bool IsParallelTo(Line2D other)
        {
            bool bothHorizontal = this.IsHorizontal && other.IsHorizontal;
            bool bothVertical = this.IsVertical && other.IsVertical;
            return bothHorizontal || bothVertical || (this.a / this.b == other.a / other.b);
        }

        public OrientedSide Side(Point2D point)
        {
            double solution = A * point.X + B * point.Y + C;
            if (solution > 0)
            {
                return OrientedSide.Positive;
            }
            else if (solution < 0)
            {
                return OrientedSide.Negative;
            }
            return OrientedSide.Boundary;
        }

        public bool HasOn(Point2D point)
        {
            return Side(point) == OrientedSide.Boundary;
        }

        public bool HasOnPositiveSide(Point2D point)
        {
            return Side(point) == OrientedSide.Positive;
        }

        public bool HasOnNegativeSide(Point2D point)
        {
            return Side(point) == OrientedSide.Negative;
        }
    }
}
