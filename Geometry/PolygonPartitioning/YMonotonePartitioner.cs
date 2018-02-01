using System;
using System.Collections.Generic;
//using System.Linq;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Wintellect.PowerCollections;

using Utility.Collections;
using Numeric;
using Geometry.HalfEdge;
using Geometry;


namespace Geometry.PolygonPartitioning
{
    public class YMonotonePartitioner<TVertex, TEdge, TFace>
        where TVertex : VertexBase, IPositioned2D, IComparable<TVertex>
        where TEdge : EdgeBase, new()
        where TFace : FaceBase, new()
    {
        //private readonly Mesh<TVertex, TEdge, TFace> source; // The input data
        private readonly Mesh<TVertex, TEdge, TFace> target; // The partitioned result
        private readonly CircularLinkedList<TVertex> vertices; // Temporary list to give vertices an order
        private readonly PriorityQueue<Point2D, TVertex> queue;
        // TODO: Replace OrderedDictionary with a splay tree
        private readonly SplayDictionary<TVertex, TVertex> helpers;
        private readonly Comparer<Point2D> yComparer;
        private readonly HighYLowXComparer yxComparer;

        private enum VertexType
        {
            Start,
            Split,
            End,
            Merge,
            Regular,
            Collinear
        }

        //static Mesh<TVertex, TEdge, TFace> MakeMonotone(Mesh<TVertex, TEdge, TFace> mesh)
        //{
        //    var partitioner = new YMonotonePartitioner<TVertex, TEdge, TFace>(mesh);
        //  //  return partitioner.Result;
        //    return mesh;
        //}

        // TODO: Temporary public constructor taking a polygon during debugging
        public YMonotonePartitioner(CircularLinkedList<TVertex> polygon)
        {
            //source = planar_subdivision;
            //target = (Mesh<TVertex, TEdge, TFace>) source.Clone();

            vertices = polygon;

            // 1. Construct a priority queue Q on the vertices P, using their y-coordinates
            // as priority. If two points have the same y-coordinate, the one with the smaller
            // x-coordinate has the higher priority
            yxComparer = new HighYLowXComparer();
            queue = new PriorityQueue<Point2D, TVertex>(yxComparer);
            foreach (TVertex vertex in polygon)
            {
                queue.Enqueue(vertex.Position, vertex);
            }

            // TODO Need right comparer here
            helpers = new SplayDictionary<TVertex, TVertex>(edgeComparer); 

            while (queue.Count != 0)
            {
                TVertex vertex = queue.Dequeue();
                VertexType type = GetVertexType(vertex);
                switch (type)
                {
                    case VertexType.Start:
                        HandleStartVertex(vertex);
                        break;
                    case VertexType.Split:
                        HandleSplitVertex(vertex);
                        break;
                    case VertexType.End:
                        HandleEndVertex(vertex);
                        break;
                    case VertexType.Merge:
                        HandleMergeVertex(vertex);
                        break;
                    case VertexType.Regular:
                        HandleRegularVertex(vertex);
                        break;
                    case VertexType.Collinear:
                        HandleCollinearVertex(vertex);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// We distinguish five types of vertices. Four of these types are
        /// turn vertices. They are defined as follows: A vertex v is a
        /// start vertex if its two neighbours lie below it and the interior
        /// angle at v is less the pi; if the interior angle is greater than
        /// pi then v is a split vertex. (if both neighbours lie below v,
        /// then the interior angle cannot be exacly pi.  A vertex is an end
        /// vertex if its two neighbours lie above it and the intertior angle
        ///  at v is less that pi; if the interior angle is greater than pi
        /// then v is a merge vertex. The vertices that are not turn vertices
        /// are regular vertices. Thus a regular vertex has one of its
        /// neighbours above it and one below it. These names have been chosen
        /// because the algorithm will use a downward plane sweep.
        /// </summary>
        /// <param name="vb">The vertex whose type is to be determined</param>
        /// <returns>The type of the vertex</returns>
        private VertexType GetVertexType(TVertex vb)
        {
            // TODO: Only works for polygons!
            Debug.Assert(vb.Degree == 2);
            // TODO: Make this more efficient
            VertexBase va = vb.Edges.First().Target;
            VertexBase vc = vb.Edges.Last().Target;

            Point2D a = ((IPositioned2D) va).Position;
            Point2D b = vb.Position;
            Point2D c = ((IPositioned2D) vc).Position;
            if (yComparer.Compare(a, b) == 0 && yComparer.Compare(c, b)== 0)
            {
                return VertexType.Collinear;
            }

            if (yxComparer.Compare(a, b) < 0)
            {
                if (yxComparer.Compare(c, b) < 0)
                {
                    return LeftTurn(a, b, c) ? VertexType.Start : VertexType.Split;
                }
                return VertexType.Regular;
            }
            if (yxComparer.Compare(b, c) < 0)
            {
                return LeftTurn(a, b, c) ? VertexType.End : VertexType.Merge;
            }
            return VertexType.Regular;
        }

        private static bool LeftTurn(Point2D a, Point2D b , Point2D c)
        {
            Triangle2D triangle = new Triangle2D(a, b, c);
            return triangle.Handedness == Sense.Counterclockwise;
        }

        private void HandleStartVertex(TVertex vertex)
        {
            helpers.Add(vertex, vertex);
        }

        private void HandleSplitVertex(TVertex vertex)
        {
            KeyValuePair<TVertex, TVertex>? directlyLeft = DirectlyLeftOf(vertex);
            if (directlyLeft.HasValue)
            {
                TVertex helper = helpers[directlyLeft.Value.Key];
                InsertDiagonal(vertex, helper);
                helpers[directlyLeft.Value.Key] = vertex;
            }
            

            helpers[vertex] = vertex;
        }

        private void HandleEndVertex(TVertex vertex)
        {
            ConnectToPreviousHelper(vertex);
        }

        private void HandleMergeVertex(TVertex vertex)
        {
            ConnectToPreviousHelper(vertex);
            ConnectToLeftHelper(vertex);
        }

        private void HandleRegularVertex(TVertex vertex)
        {
            if (IsInteriorToRight(vertex))
            {
                ConnectToPreviousHelper(vertex);
                helpers[vertex] = vertex;
            }
            else
            {
                ConnectToLeftHelper(vertex);
            }
        }

        private void HandleCollinearVertex(TVertex vertex)
        {
            TVertex previous = PreviousVertex(vertex);
            if (helpers.ContainsKey(previous))
            {
                helpers.Remove(previous);
            }
            helpers[vertex] = vertex;
        }

        private void ConnectToPreviousHelper(TVertex vertex)
        {
            TVertex previous = PreviousVertex(vertex);
            Debug.Assert(helpers.ContainsKey(previous));
            TVertex helper = helpers[previous];
            if (GetVertexType(helper) == VertexType.Merge)
            {
                InsertDiagonal(vertex, helper);
            }
            helpers.Remove(previous);
        }

        private void ConnectToLeftHelper(TVertex vertex)
        {
            KeyValuePair<TVertex, TVertex>? directlyLeft = DirectlyLeftOf(vertex);
            if (directlyLeft.HasValue)
            {
                TVertex helper = helpers[directlyLeft.Value.Key];
                if (GetVertexType(helper) == VertexType.Merge)
                {
                    InsertDiagonal(vertex, helper);
                }
                helpers[directlyLeft.Value.Key] = vertex;
            }
        }

        private KeyValuePair<TVertex, TVertex>? DirectlyLeftOf(TVertex vertex)
        {
            KeyValuePair<TVertex, TVertex> result;
            bool found = helpers.TryGetLargestBelow(vertex, out result);
            if (found)
            {
                return result;
            }
            return null;
        }

        private bool IsInteriorToRight(TVertex vertex)
        {
            Point2D position = vertex.Position;

            Point2D previous = PreviousVertex(vertex).Position;
            int cmp = yComparer.Compare(previous, position);
            if (cmp > 0)
            {
                return true;
            }

            Point2D next = NextVertex(vertex).Position;
            if (cmp == 0 && yComparer.Compare(next, position) < 0)
            {
                return true;
            }

            return false;
        }

        private void InsertDiagonal(TVertex from, TVertex to)
        {
            target.AddEdge(from, to);
        }

        private TVertex PreviousVertex(TVertex vertex)
        {
            // TODO: Temporary and horribly inefficient!
            CircularLinkedListNode<TVertex> node = vertices.Find(vertex);
            return node.Previous.Value;
        }

        private TVertex NextVertex(TVertex vertex)
        {
            // TODO: Temporary and horribly inefficient!
            CircularLinkedListNode<TVertex> node = vertices.Find(vertex);
            return node.Next.Value;
        }
    }
}
