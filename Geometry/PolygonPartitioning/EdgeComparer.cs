using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Utility.Collections;

using Numeric;

namespace Geometry.PolygonPartitioning
{
    /// <summary>
    /// Used for ordering edges along the sweep line. Given two CircularLinkedList nodes
    /// which are the start points of two edges in a circular-linked list - sort the two edges
    /// along a horizontal sweep line.  This comparator is use by teh YMonotonePartitioner
    /// algorithm.
    /// </summary>
    internal class EdgeComparer<TVertex> : IComparer<CircularLinkedListNode<TVertex>>
        where TVertex: IPositioned2D
    {
        public int Compare(CircularLinkedListNode<TVertex> pNode, CircularLinkedListNode<TVertex> qNode)
        {
            // Check to see if any of the endpoints are shared
            if (pNode == qNode)
            {
                return 0;
            }

            var after_p_node = pNode.Next;
            var after_q_node = qNode.Next;

            if (pNode == after_q_node)
            {
                return LargerXAtVertexY(pNode, qNode);
            }

            if (after_p_node == qNode)
            {
                return -1 * LargerXAtVertexY(qNode, pNode);
            }

            // TODO: Is this redundant - for a polygon at least?
            if (pNode == qNode)
            {
                return LargerXAtVertexY(pNode, after_q_node);
            }

            // TODO: Is this redundant- for a polygon at least?
            if (after_p_node == after_q_node)
            {
                return LargerXAtVertexY(pNode, qNode);
            }

            // None of the endpoints are shared

            // Create a supporing line for the first segment
            Point2D p = pNode.Value.Position;
            Point2D q = qNode.Value.Position;
            Point2D pEnd = after_p_node.Value.Position;
            Point2D qEnd = after_q_node.Value.Position;

            // Supporting lines for the segments
            Line2D pLine = new Line2D(p, pEnd);
            Line2D qLine = new Line2D(q, qEnd);
            
            if (pLine.IsHorizontal)
            {                
                if (qLine.IsHorizontal)
                {
                    Point2D pMax = p.X < pEnd.X ? pEnd: p;
                    Point2D qMax = q.X < qEnd.X ? qEnd: q;
                    return pMax.X.CompareTo(qMax.X);
                }
                // Project p onto the qLine and compare the y-coordinate of p
                // with the y-coordinate of the projected point
                return CompareYProjection(p, qLine);
            }

            int qCmp = CompareYProjection(q, pLine);
            int qEndCmp = CompareYProjection(qEnd, pLine);
            if (qCmp == qEndCmp)
            {
                return qCmp;
            }

            if (qLine.IsHorizontal)
            {
                return CompareYProjection(q, pLine);
            }
            return CompareYProjection(p, qLine);
        }

        // Project point onto the line and compare the y-coordinate of point
        // with the y-coordinate of the projected point
        private static int CompareYProjection(Point2D point, Line2D line)
        {
            double yProjection = line.SolveForY(point.X);
            return point.Y.CompareTo(yProjection);
        }

        /// <summary>
        /// Determine if the edge beginning at start has a larger x value
        /// than vertex.X at a y-value of vertex.Y
        /// </summary>
        /// <param name="startNode">The start of an edge</param>
        /// <param name="vertexNode">The test vertex</param>
        /// <returns></returns>
        private static int LargerXAtVertexY(CircularLinkedListNode<TVertex> startNode, CircularLinkedListNode<TVertex> vertexNode)
        {
            CircularLinkedListNode<TVertex> endNode = startNode.Next;
            Point2D start = startNode.Value.Position;
            Point2D end = endNode.Value.Position;
            Point2D point = vertexNode.Value.Position;
            if (start.Y == end.Y)
            {
                return Math.Min(start.X, end.X).CompareTo(point.X);
            }
            Line2D line = new Line2D(start, end);
            return CompareYProjection(point, line);
        }
    }
}
