using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Geometry.HalfEdge;
using Numeric;
using Utility.Collections;

namespace Geometry.PolygonPartitioning
{
    public class MonotoneYPartitioner<TVertex, TEdge, TFace>
        where TVertex :VertexBase, IPositionable2D
        where TEdge :EdgeBase, new()
        where TFace :FaceBase, new()
    {
        // Terminology:
        //  above-incident : edges which are connected to a vertex from above
        //  below-indicent : edges which are connected to a vertex from below

        /// <summary> The resulting mesh, which starts as a copy of the input mesh</summary>
        private readonly Mesh<TVertex, TEdge, TFace> mesh;

        /// <summary>Have we run the computation yet - has the mesh been partitioned into monotone pieces?</summary>
        bool partitioned;

        /// <summary>The vertices in priority order of y-coordinate</summary>
        private readonly PriorityQueueDictionary<Point2D, TVertex> queueDictionary;

        private readonly MonotoneMeshUtilities<TVertex> meshUtilities;

        /// <summary>The comparer used for ordering edges left-to-right along the sweep-line</summary>
        private readonly LeftToRightEdgeComparer xEdgeComparer;

        /// <summary>Provides functions, based on xEdgeComparer for sorting edges along the sweep-line</summary>
        private readonly SweeplineUtilities sweeplineUtilities;

        /// <summary>The ordering of edges left-to-right along the sweep line. Associated with each edge is a helper vertex</summary>
        private readonly SplayDictionary<EdgeBase, TVertex> helpers;

        private readonly Action<Mesh<TVertex, TEdge, TFace>, TVertex, TVertex> DiagonalInserterDelegate;

        private enum VertexType
        {
            Split,
            Merge,
            Regular,
            Collinear,
            Unknown
        }

        /// <summary>
        /// Create the partitioner. New diagonals will be created in the result mesh by using
        /// calls to mesh.AddEdge(fromVertex, toVertex).
        /// </summary>
        /// <param name="mesh">The mesh to be partitioned.  A copy of the mesh is returned.</param>
        public MonotoneYPartitioner(Mesh<TVertex, TEdge, TFace> mesh) :
            this(mesh, InsertDiagonal)
        {
        }

        /// <summary>
        /// Create the partitioner. New diagonals will be inserted into the result mesh by calls to
        /// the provided DiagonalInserterDelegate which accepts the mesh and from and to vertices for the
        /// new edge.
        /// </summary>
        /// <param name="mesh">The mesh to be partitioned.</param>
        /// <param name="DiagonalInserterDelegate">An action for inserting edges into the mesh between the specificed vertices</param>
        public MonotoneYPartitioner(Mesh<TVertex, TEdge, TFace> mesh,
                                    Action<Mesh<TVertex, TEdge, TFace>, TVertex, TVertex> DiagonalInserterDelegate)
        {
            // TODO: We should copy the mesh here, but no copy-constructor yet!
            //this.mesh = new Mesh<TVertex, TEdge, TFace>(mesh);
            this.mesh = mesh;

            // The comparer used for ordering vertices in the queue - controlling
            // the order in which the sweep line sweeps over vertices
            HighYLowXComparer yxComparer = new HighYLowXComparer();
            meshUtilities = new MonotoneMeshUtilities<TVertex>(yxComparer);
            queueDictionary = new PriorityQueueDictionary<Point2D, TVertex>(yxComparer);
            xEdgeComparer = new LeftToRightEdgeComparer();
            sweeplineUtilities = new SweeplineUtilities(xEdgeComparer);
            helpers = new SplayDictionary<EdgeBase, TVertex>(xEdgeComparer);
            this.DiagonalInserterDelegate = DiagonalInserterDelegate;
        }

        public Mesh<TVertex, TEdge, TFace> GetResult()
        {
            if (!partitioned)
            {
                Partition();
            }
            return mesh;
        }

        private void Partition()
        {
            // 1. Construct a priority queue Q on the vertices P, using their y-coordinates
            // as priority. If two points have the same y-coordinate, the one with the smaller
            // x-coordinate has the higher priority

            foreach (TVertex vertex in mesh.Vertices)
            {
                queueDictionary.Enqueue(vertex.Position, vertex);
            }

            // In this approach we need to find the edge to the left of each vertex. Therefore,
            // we store the edges of the mesh intsecting the sweep line in the leaves of a 
            // dynamic binary search tree T. The left-to-right order in the tree corresponds to
            // the left-to-right order of the edges along the sweep line.
            // [ Because we are only interested in edges to the left of split or merge vertices
            // we only need to store edges in T that have the interior of P to their right.]
            // With each edge in T we store its helper. The tree T and the helpers stored with
            // the edges form the status of the sweep line algorithm.

            Debug.Assert(queueDictionary.Count >= 2);

            // Start vertex
            TVertex startVertex = queueDictionary.Dequeue();
            HandleStartVertex(startVertex);

            // Take the vertices in priority order, categorise them, and process them.
            while (queueDictionary.Count > 1)
            {
                TVertex vertex = queueDictionary.Dequeue();
                VertexType type = GetVertexType(vertex);
                switch (type)
                {
                    case VertexType.Split:
                        HandleSplitVertex(vertex);
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
                    case VertexType.Unknown:
                        goto default;
                    default:
                        throw new ApplicationException("Unknown vertex type");
                }
            }

            TVertex endVertex = queueDictionary.Dequeue();
            HandleEndVertex(endVertex);

            Debug.Assert(helpers.IsEmpty);

            partitioned = true;
        }

        /// <summary>
        /// Determine the type of a vertex.
        /// 
        /// Split vertex   : All of the incident edges are below the vertex.
        /// Merge vertex   : All of the incident edges are above the vertex.
        /// Regular vertex : Indicent edges are both above and below the vertex
        /// Collinear vertex : Degree two vertex. It and both of its neighbours have the same y-coordinate
        /// </summary>
        /// <param name="vertex">The vertex</param>
        /// <returns>The type of the vertex</returns>
        private VertexType GetVertexType(TVertex vertex)
        {
            if (vertex.Degree == 2)
            {
                IPositionable2D a = (IPositionable2D) vertex.HalfEdges.ElementAt(0).Target;
                IPositionable2D b = (IPositionable2D) vertex.HalfEdges.ElementAt(1).Target;
                if (a.Position.Y == vertex.Position.Y && b.Position.Y == vertex.Position.Y)
                {
                    return VertexType.Collinear;
                }
            }

            bool edgesAbove = vertex.FindHalfEdge(meshUtilities.Upwards) != null;
            bool edgesBelow = vertex.FindHalfEdge(meshUtilities.Downwards) != null;

            if (edgesAbove && edgesBelow)
            {
                return VertexType.Regular;
            }
            if (edgesAbove)
            {
                return VertexType.Merge;
            }
            if (edgesBelow)
            {
                return VertexType.Split;
            }
            return VertexType.Unknown;
        }

        private void HandleStartVertex(TVertex vertex)
        {
            // Insert each edge into the sweep line state, and the the helper
            // of each edge to be this vertex. There are no connections to be made.

            // We can iterate through all edges because we know this is a start vertex
            // with no above-incident edges
            Debug.Assert(helpers.Count == 0);
            foreach (EdgeBase edge in vertex.Edges)
            {
                helpers.Add(edge, vertex);
            }
            Debug.Assert(helpers.Count == vertex.Degree);
        }

        private void HandleSplitVertex(TVertex vertex)
        {
#if DEBUG
            int hs = helpers.Count;
#endif
            Debug.Assert(GetVertexType(vertex) == VertexType.Split);
            // There are no sweep line status modification to be made from above-incident edges
            // Add a diagonal from vertex to the helper of the edge directly to the left
            EdgeBase rightmostEdge = RightmostEdge(vertex);
            KeyValuePair<EdgeBase, TVertex>? directlyLeft = DirectlyLeftOf(rightmostEdge);
            if (directlyLeft.HasValue)
            {
                TVertex helper = directlyLeft.Value.Value;
                DiagonalInserterDelegate(mesh, vertex, helper);
                helpers[directlyLeft.Value.Key] = vertex;
            }

            // Add all of the below-indicent edges into the sweep line status (helpers)
            // with this vertex as their helper
            IEnumerable<EdgeBase> belowEdges = meshUtilities.BelowEdges(vertex);
            foreach (EdgeBase edge in belowEdges)
            {
                helpers[edge] = vertex;
            }
#if DEBUG
            Debug.Assert(helpers.Count == hs + belowEdges.Count());
#endif
        }

        private void HandleEndVertex(TVertex vertex)
        {
            HandleMergeVertex(vertex);
        }

        private void HandleMergeVertex(TVertex vertex)
        {
#if DEBUG
            int hs = helpers.Count;
#endif
            Debug.Assert(GetVertexType(vertex) == VertexType.Merge);

            // Deal with connections to the right of the above-incident vertices
            // Connect each above indicent edge to its helper, if that
            // helper was a merge vertex. And remove these above-incident edges
            // from the sweep line status (helpers). This is to complete the regularisation
            // of merge vertices encountered earlier in the sweep.
            var aboveEdges = meshUtilities.AboveEdges(vertex).ToList();
            foreach (EdgeBase edge in aboveEdges)
            {
                if (helpers.ContainsKey(edge))
                {
                    TVertex helper = helpers[edge];
                    if (GetVertexType(helper) == VertexType.Merge)
                    {
                        DiagonalInserterDelegate(mesh, vertex, helper);
                    }
                }
            }

            // Remove the above incident-edges from the sweep
            foreach (EdgeBase edge in aboveEdges)
            {
                bool removed = helpers.Remove(edge);
                Debug.Assert(removed);
            }

            // Deal with connections to the left of the above-incident vertices
            KeyValuePair<EdgeBase, TVertex>? directlyLeft = DirectlyLeftOf(sweeplineUtilities.RightMost(aboveEdges));
            if (directlyLeft.HasValue)
            {
                if (helpers.ContainsKey(directlyLeft.Value.Key))
                {
                    TVertex helper1 = helpers[directlyLeft.Value.Key];
                    if (GetVertexType(helper1) == VertexType.Merge)
                    {
                        DiagonalInserterDelegate(mesh, vertex, helper1);
                    }
                    helpers[directlyLeft.Value.Key] = vertex;
                }
            }

#if DEBUG
            // There should be no below edges from a merge-vertex
            var belowEdges = meshUtilities.BelowEdges(vertex);
            Debug.Assert(belowEdges.Count() == 0);
            Debug.Assert(helpers.Count == hs - aboveEdges.Count() + belowEdges.Count());
#endif
        }

        void HandleRegularVertex(TVertex vertex)
        {
#if DEBUG
            int hs = helpers.Count;
#endif
            // Deal with connections to the right of the above-incident vertices
            // Connect each above indicent edge to its helper, if that
            // helper was a merge vertex. And remove these above-incident edges
            // from the sweep line status (helpers). This is to complete the regularisation
            // of merge vertices encountered earlier in the sweep.
            var aboveEdges = meshUtilities.AboveEdges(vertex).ToList();
            foreach (EdgeBase edge in aboveEdges)
            {
                if (helpers.ContainsKey(edge))
                {
                    TVertex helper = helpers[edge];
                    if (GetVertexType(helper) == VertexType.Merge)
                    {
                        DiagonalInserterDelegate(mesh, vertex, helper);
                    }
                }
            }

            // Remove the above incident-edges from the sweep
            foreach (EdgeBase edge in aboveEdges)
            {
                bool removed = helpers.Remove(edge);
                Debug.Assert(removed);
            }

            // Deal with connections to the left of the above-incident vertices
            KeyValuePair<EdgeBase, TVertex>? directlyLeft = DirectlyLeftOf(sweeplineUtilities.RightMost(aboveEdges));
            if (directlyLeft.HasValue)
            {
                if (helpers.ContainsKey(directlyLeft.Value.Key))
                {
                    TVertex helper1 = helpers[directlyLeft.Value.Key];
                    if (GetVertexType(helper1) == VertexType.Merge)
                    {
                        DiagonalInserterDelegate(mesh, vertex, helper1);
                    }
                    helpers[directlyLeft.Value.Key] = vertex;
                }
            }

            // TODO: Maybe we only need to connect to one or other above - the lowest. May lead
            // TODO: to fewer polygons.

            // Add below-incident edges to the sweep line status with the current vertex
            // Add all of the below-indicent edges into the sweep line status (helpers)
            // with this vertex as their helper
            var belowEdges = meshUtilities.BelowEdges(vertex).ToList();
            foreach (EdgeBase edge in belowEdges)
            {
                Debug.Assert(!helpers.ContainsKey(edge));
                helpers[edge] = vertex;
            }
#if DEBUG
            Debug.Assert(helpers.Count == hs - aboveEdges.Count() + belowEdges.Count());
#endif
        }

        /// <summary>
        /// Handle collinear vertices of degree two, where both neighbouring
        /// vertices have the same y-coordinate.
        /// </summary>
        /// <param name="vertex">A vertex</param>
        private void HandleCollinearVertex(TVertex vertex)
        {
            var aboveEdges = meshUtilities.AboveEdges(vertex);
            Debug.Assert(aboveEdges.Count() == 1);
            EdgeBase aboveEdge = aboveEdges.First();
            helpers.Remove(aboveEdge);

            var belowEdges = meshUtilities.BelowEdges(vertex);
            Debug.Assert(belowEdges.Count() == 1);
            EdgeBase belowEdge = belowEdges.First();
            helpers[belowEdge] = vertex;
        }

        /// <summary>
        /// Given a vertex, return the rightmost edge (i.e. most +ve x) incident to that vertex.
        /// </summary>
        /// <param name="vertex">A vertex</param>
        /// <returns>The right most edge from the vertex.</returns>
        private EdgeBase RightmostEdge(TVertex vertex)
        {
            Debug.Assert(vertex.Degree != 0);
            return sweeplineUtilities.RightMost(vertex.Edges);
        }

        /// <summary>
        /// Given an edge, determine the edge directly to the left of it on the sweep line
        /// </summary>
        /// <param name="edge">An edge - not necessarily in the sweepline)</param>
        /// <returns>If it exists, the edge directly to the left on the sweepline, or null if there is none</returns>
        private KeyValuePair<EdgeBase, TVertex>? DirectlyLeftOf(EdgeBase edge)
        {
            KeyValuePair<EdgeBase, TVertex> result;
            bool found = helpers.TryGetLargestBelow(edge, out result);
            if (found)
            {
                return result;
            }
            return null;
        }

        private static void InsertDiagonal(Mesh<TVertex, TEdge, TFace> m, TVertex from, TVertex to)
        {
            Debug.Assert(m.Find(from, to) == null);
            // TODO: May need to handle faces here - e.g. use SplitFace Euler method
            m.AddEdge(from, to);
        }
    }
}
