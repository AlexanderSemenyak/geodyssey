using System;
using System.Collections.Generic;
using System.Text;

using Numeric;

namespace Geometry
{
    public static class Intersector
    {
        public enum Intersection
        {
            DoIntersect,
            DontIntersect,
            Collinear
        }

        /// <summary>
        /// Determine whether two Ray2Ds intersect, and if so the location of the
        /// intersection point.
        /// </summary>
        /// <param name="ray1">The first Ray2D to be intersected</param>
        /// <param name="ray2">The second Ray2D to be intersected</param>
        /// <returns>
        /// If the rays intersect a Point2D containing the location of the intersection. If
        /// the rays are collinear or do not intersect, null is returned.
        /// </returns>
        public static Point2D? Intersect(Ray2D ray1, Ray2D ray2)
        {
            Point2D result;
            Intersection state = Intersect(ray1, ray2, out result);
            if (state == Intersection.DoIntersect)
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// Determine whether two Ray2Ds intersect, and if they do return the location
        /// of the intersection.
        /// </summary>
        /// <param name="ray1">The first Ray2D to be intersected.</param>
        /// <param name="ray2">The second Ray2D to be intersected</param>
        /// <param name="result">The location of the intersection point if the rays intersect, otherwise an undefined value.</param>
        /// <returns>DoIntersect, DontIntersect or Collinear.</returns>
        public static Intersection Intersect(Ray2D ray1, Ray2D ray2, out Point2D result)
        {
            Line2D line1 = ray1.SupportingLine;
            Line2D line2 = ray2.SupportingLine;

            Intersection supportingLinesIntersection = Intersect(line1, line2, out result);
            if (supportingLinesIntersection != Intersection.DoIntersect)
            {
                return supportingLinesIntersection;
            }

            if ((line1.Normal.Dot(ray2.Direction) * (line1.Normal.Dot(new Vector2D(ray2.Source)) + line1.C) <= 0.0)
                && (line2.Normal.Dot(ray1.Direction) * (line2.Normal.Dot(new Vector2D(ray1.Source)) + line2.C) <= 0.0))
            {
                return Intersection.DoIntersect;
            }
            return Intersection.DontIntersect;
        }

        /// <summary>
        /// Determine whether two Line2Ds intersect, and if so the location of the intersection.
        /// </summary>
        /// <param name="line1">The first Line2D to be intersected.</param>
        /// <param name="line2">The second Line2D to be intersected.</param>
        /// <returns>The location of the intersection if the lines intersect, or null if the lines are collinear or parallel.</returns>
        public static Point2D? Intersect(Line2D line1, Line2D line2)
        {
            Point2D result;
            Intersection state = Intersect(line1, line2, out result);
            if (state == Intersection.DoIntersect)
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// Determine whether two Line2Ds intersect, and if so the location of the intersection.
        /// </summary>
        /// <param name="line1">The first Line2D to be intersected.</param>
        /// <param name="line2">The second Line2D to be intersected.</param>
        /// <param name="result">The location of the intersection if the lines intersect, otherwise an undefined value.</param>
        /// <returns>DoIntersect, DontIntersect or Collinear.</returns>
        public static Intersection Intersect(Line2D line1, Line2D line2, out Point2D result)
        {
            // Compute the determinant
            double denominator = line1.A * line2.B - line1.B * line2.A;

            if (denominator == 0.0)
            {
                // TODO: Need to identify collinear lines
                result = new Point2D();
                return Intersection.DontIntersect;
            }

            double xNumerator = line1.B * line2.C - line2.B * line1.C;
            double yNumerator = line2.A * line1.C - line1.A * line2.C;
            result = new Point2D(xNumerator / denominator, yNumerator / denominator);
            return Intersection.DoIntersect;
        }

        public static Point2D? Intersect(Ray2D ray, Line2D line)
        {
            Point2D result;
            Intersection state = Intersect(ray, line, out result);
            if (state == Intersection.DoIntersect)
            {
                return result;
            }
            return null;
        }

        public static Intersection Intersect(Ray2D ray, Line2D line, out Point2D result)
        {
            Line2D rayLine = ray.SupportingLine;

            Intersection supportingLinesIntersection = Intersect(rayLine, line, out result);
            if (supportingLinesIntersection != Intersection.DoIntersect)
            {
                return supportingLinesIntersection;
            }

            if (line.Normal.Dot(ray.Direction) * (line.Normal.Dot(new Vector2D(ray.Source)) + line.C) <= 0.0)
            {
                return Intersection.DoIntersect;
            }
            return Intersection.DontIntersect;
        }

        public static Point2D? Intersect(Segment2D segment1, Segment2D segment2)
        {
            Point2D result;
            Intersection state = Intersect(segment1, segment2, out result);
            if (state == Intersection.DoIntersect)
            {
                return result;
            }
            return null;
        }

        public static Intersection Intersect(Segment2D segment1, Segment2D segment2, out Point2D result)
        {
            double x1 = segment1.Source.X;
            double x2 = segment1.Target.X;
            double x3 = segment2.Source.X;
            double x4 = segment2.Target.X;
            double y1 = segment1.Source.Y;
            double y2 = segment1.Target.Y;
            double y3 = segment2.Source.Y;
            double y4 = segment2.Target.Y;
            double denominator = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);

            double numerator1 = (x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3);
            double numerator2 = (x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3);

            if (denominator == 0.0)
            {
                result = new Point2D();
                if (numerator1 == 0.0 && numerator2 == 0.0)
                {
                    return Intersection.Collinear;
                }
                return Intersection.DontIntersect;
            }

            double ua = numerator1 / denominator;
            double ub = numerator2 / denominator;

            if (ua < 0.0 || ua > 1.0 || ub < 0.0 || ub > 1.0)
            {
                result = new Point2D();
                return Intersection.DontIntersect;
            }

            result = new Point2D(x1 + ua * (x2 - x1),
                                 y1 + ua * (y2 - y1));

            return Intersection.DoIntersect;
        }
    }
}
