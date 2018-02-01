using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geometry.HalfEdge;
using Numeric;

namespace Geometry.PolygonPartitioning
{
    /// <summary>
    /// Comparator for comparting the left-to-right (x-axis) location of
    /// edges on a sweep-line. 
    /// </summary>
    public class LeftToRightEdgeComparer : IComparer<EdgeBase>
    {
        public int Compare(EdgeBase p, EdgeBase q)
        {
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }

            if (q == null)
            {
                throw new ArgumentNullException("q");
            }

            // Check to see if any of the endpoints are shared
            if (p == q)
            {
                return 0;
            }

            // Create a supporing line for the first segment
            Point2D pStart = ((IPositionable2D) p.Source).Position;
            Point2D qStart = ((IPositionable2D) q.Source).Position;
            Point2D pEnd = ((IPositionable2D) p.Target).Position;
            Point2D qEnd = ((IPositionable2D) q.Target).Position;

            if (pStart == qStart && pEnd == qEnd)
            {
                return 0;
            }

            if (pStart == qEnd)
            {
                return CompareXAtVertexY(pStart, pEnd, qStart);
            }

            if (pEnd == qStart)
            {
                return -CompareXAtVertexY(qStart, qEnd, pStart);
            }

            if (pStart == qStart)
            {
                return CompareXAtVertexY(pStart, pEnd, qEnd);
            }

            if (pEnd == qEnd)
            {
                return CompareXAtVertexY(pStart, pEnd, qStart);
            }

            // None of the endpoints are shared

            // Supporting lines for the segments
            Line2D pLine = new Line2D(pStart, pEnd);
            Line2D qLine = new Line2D(qStart, qEnd);

            if (pLine.IsHorizontal)
            {
                if (qLine.IsHorizontal)
                {
                    Point2D pMax = pStart.X < pEnd.X ? pEnd : pStart;
                    Point2D qMax = qStart.X < qEnd.X ? qEnd : qStart;
                    return pMax.X.CompareTo(qMax.X);
                }
                // Project p onto the qLine and compare the y-coordinate of p
                // with the y-coordinate of the projected point
                return CompareXAtY(pStart, qLine);
            }

            int qCmp = -CompareXAtY(qStart, pLine);
            int qEndCmp = -CompareXAtY(qEnd, pLine);
            if (qCmp == qEndCmp)
            {
                return qCmp;
            }

            if (qLine.IsHorizontal)
            {
                return -CompareXAtY(qStart, pLine);
            }
            return CompareXAtY(pStart, qLine);
        }

        /// <summary>
        /// Compares the x-coordinates of p and the horizontal projection of p
        /// on h
        /// </summary>
        /// <param name="p">A point to be projected onto line</param>
        /// <param name="line">The line onto which point will be projected</param>
        /// <returns>-1 if the p is left of the line, 0 if p is on the line, 1 if p is to the right of the line</returns>
        private static int CompareXAtY(Point2D p, Line2D line)
        {
            double xProjection = line.SolveForX(p.Y);
            return p.X.CompareTo(xProjection);
        }

        /// <summary>
        /// Determine if the edge between start and end has a larger x-value than point.X at
        /// a y-value of point.Y
        /// </summary>
        /// <returns>-1 if the x-value is smaller. 0 if the x-value is the same. +1 if the x-value is greater</returns>
        private static int CompareXAtVertexY(Point2D start, Point2D end, Point2D point)
        {
            if (start.Y == end.Y)
            {
                int minCmp = Math.Min(start.X, end.X).CompareTo(point.X);
                if (minCmp == 0)
                {
                    int maxCmp = Math.Max(start.X, end.X).CompareTo(point.X);
                    return maxCmp;
                }
                return minCmp;
            }
            Line2D line = new Line2D(start, end);
            return -CompareXAtY(point, line);
        }
    }
}
