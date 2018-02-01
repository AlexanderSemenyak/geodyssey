using System;
using System.Collections;
using System.Collections.Generic;

namespace Numeric
{
    public struct Triangle2D
    {
        private Point2D a;
        private Point2D b;
        private Point2D c;
        private Point2D? circumcenter; // cached

        #region Constructors
        public Triangle2D(Point2D a, Point2D b, Point2D c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.circumcenter = null;
        }

        /// <summary>
        /// Constructs a triangle from the first three points yielded
        /// by the supplied enumerable.
        /// </summary>
        /// <param name="points">A sequence of three points</param>
        public Triangle2D(IEnumerable<Point2D> points)
        {
            using (IEnumerator<Point2D> enumerator = points.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    throw new ArgumentException("points enumerable must contain at least three points");
                }
                a = enumerator.Current;
                if (!enumerator.MoveNext())
                {
                    throw new ArgumentException("points enumerable must contain at least three points");
                }
                b = enumerator.Current;
                if (!enumerator.MoveNext())
                {
                    throw new ArgumentException("points enumerable must contain at least three points");
                }
                c = enumerator.Current;
            }
            this.circumcenter = null;
        }

        #endregion

        #region Properties
        public Point2D Incenter
        {
            get
            {
                // TODO: Check here for division by zero etc.
                double ax1 = LengthA * a.X;
                double bx2 = LengthB * b.X;
                double cx3 = LengthC * c.X;
                double x = (ax1 + bx2 + cx3) / (LengthA + LengthB + LengthC);

                double ay1 = LengthA * a.Y;
                double by2 = LengthB * b.Y;
                double cy3 = LengthC * c.Y;
                double y = (ay1 + by2 + cy3) / (LengthA + LengthB + LengthC);
                return new Point2D(x, y);
            }
        }

        public Point2D Circumcenter
        {
            get
            {
                if (!circumcenter.HasValue)
                {
                    if (Math.Abs(a.Y - b.Y) < double.Epsilon && Math.Abs(b.Y - c.Y) < double.Epsilon)
                    {
                        throw new ArithmeticException("Cannot compute circumcenter for degenerate triangle");
                    }

                    // TODO: Check here for division by zero, etc
                    if (Math.Abs(b.Y - a.Y) < double.Epsilon)
                    {
                        circumcenter = ComputeCircumCenter1();
                    }
                    if (Math.Abs(c.Y - b.Y) < double.Epsilon)
                    {
                        circumcenter = ComputeCircumcenter2();
                    }
                    circumcenter = ComputeCircumcenter3();
                }
                return circumcenter.Value;
            }
        }

        private Point2D ComputeCircumcenter3()
        {
            double m1 = -(b.X - a.X) / (b.Y - a.Y);
            double m2 = -(c.X - b.X) / (c.Y - b.Y);
            double mx1 = (a.X + b.X) * 0.5;
            double mx2 = (b.X + c.X) * 0.5;
            double my1 = (a.Y + b.Y) * 0.5;
            double my2 = (b.Y + c.Y) * 0.5;
            double xc = (m1 * mx1 - m2 * mx2 + my2 - my1) / (m1 - m2);
            return new Point2D(xc, m1 * (xc - mx1) + my1);
        }

        private Point2D ComputeCircumcenter2()
        {
            double m1 = -(b.X - a.X) / (b.Y - a.Y);
            double mx1 = (a.X + b.X) * 0.5;
            double my1 = (a.Y + b.Y) * 0.5;
            double xc = (c.X + b.X) * 0.5;
            return new Point2D(xc, m1 * (xc - mx1) + my1);
        }

        private Point2D ComputeCircumCenter1()
        {
            double m2 = -(c.X - b.X) / (c.Y - b.Y);
            double mx2 = (b.X + c.X) * 0.5;
            double my2 = (b.Y + c.Y) * 0.5;
            double xc = (b.X + a.X) * 0.5;
            return new Point2D(xc, m2 * (xc - mx2) + my2);
        }

        public double Determinant
        {
            get { return a.X * b.Y - a.X * c.Y - b.X * a.Y + b.X * c.Y - c.X * b.Y + c.X * a.Y; }
        }

        public double SignedArea
        {
            get
            {
                return Determinant / 2.0;
            }
        }

        public double Area
        {
            get
            {
                return Math.Abs(SignedArea);
            }
        }

        public bool IsDegenerate
        {
            get { return Determinant == 0.0; }
        }

        public Sense Handedness
        {
            get
            {
                // TODO: Compare with numerical robustness in CGAL Orientation_2
                double det = Determinant;
                if (det < 0.0)
                {
                    return Sense.Clockwise;
                }
                if (det > 0.0)
                {
                    return Sense.Counterclockwise;
                }
                return Sense.None;
            }
        }

        public Point2D A
        {
            get { return a; }
        }

        public Point2D B
        {
            get { return b; }
        }

        public Point2D C
        {
            get { return c; }
        }

        public double LengthA
        {
            get { return (b - c).Magnitude; }
        }

        public double LengthB
        {
            get { return (a - c).Magnitude; }
        }

        public double LengthC
        {
            get { return (a - b).Magnitude; }
        }

        public double Perimeter
        {
            get { return LengthA + LengthB + LengthC; }
        }

        public double AngleA
        {
            get
            { return AngleFromSideVectors(b - a, c - a); }
        }

        public double AngleB
        {
            get
            { return AngleFromSideVectors(c - b, a - b); }
        }

        public double AngleC
        {
            get { return AngleFromSideVectors(a - c, b - c); }
        }

        private static double AngleFromSideVectors(Vector2D p, Vector2D q)
        {
            return p.Angle(q);
        }

        #endregion

        public bool InCircumcircle(Point2D p)
        {
            //Return TRUE if the point (xp,yp) lies inside the circumcircle
            //made up by points (x1,y1) (x2,y2) (x3,y3)
            //NOTE: A point on the edge is inside the circumcircle

            Point2D center = Circumcenter;
            double bsqr = (b - center).Magnitude2;
            double psqr = (p - center).Magnitude2;

            return psqr <= bsqr;
        }
    }
}
