using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Geometry.HalfEdge;
using Numeric;
using Utility.Collections;
using Utility.Extensions.System.Collections.Generic;
using Wintellect.PowerCollections;

namespace Geometry.PolygonPartitioning
{
    /// <summary>
    /// Triangulation of a y-monotone polygon.  The polgon is provided as a face
    /// of a half-edge based mesh.  Usee MonotoneYPartitioner to subdivide a general
    /// planar subdivision into y-monotone polygons before applying this algorithm.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TEdge"></typeparam>
    /// <typeparam name="TFace"></typeparam>
    public class MonotonePolygonTriangulator<TVertex, TEdge, TFace>
        where TVertex :VertexBase, IPositionable2D
        where TEdge :EdgeBase, new()
        where TFace :FaceBase, new()
    {
        public enum Chain
        {
            Left,
            Right
        }

        public struct ChainVertex
        {
            public readonly TVertex Vertex;
            public readonly Chain Chain;

            public ChainVertex(TVertex vertex, Chain chain)
            {
                Vertex = vertex;
                Chain = chain;
            }
        }

        /// <summary> The resulting mesh, which starts as a copy of the input mesh</summary>
        private readonly Mesh<TVertex, TEdge, TFace> mesh;

        /// <summary>The face of the mesh to be triangulated</summary>
        private readonly TFace face;

        /// <summary>Have we run the computation yet - has the mesh been partitioned into monotone pieces?</summary>
        private bool triangulated;
     
        private readonly Func<Mesh<TVertex, TEdge, TFace>, FaceBase, ChainVertex, ChainVertex, TEdge> splitFace;
        private MonotoneMeshUtilities<TVertex> meshUtilities;
        private SweeplineUtilities sweeplineUtilities;

        public MonotonePolygonTriangulator(Mesh<TVertex, TEdge, TFace> mesh, TFace face) :
            this(mesh, face, SplitFace)
        {
        }

        // TODO: This constructor requires that ChainVertex be public, which isn't good
        public MonotonePolygonTriangulator(Mesh<TVertex, TEdge, TFace> mesh, TFace face,
                                           Func<Mesh<TVertex, TEdge, TFace>, FaceBase, ChainVertex, ChainVertex, TEdge> splitFaceDelegate)
        {
            // TODO: We should copy the mesh here, but no copy-constructor yet!
            //this.mesh = new Mesh<TVertex, TEdge, TFace>(mesh);
            this.mesh = mesh;

            if (face.EdgeCount < 3)
            {
                throw new ArgumentException("face must be a polygon with at least three edges");
            }

            this.face = face;
            triangulated = face.EdgeCount == 3;
            splitFace = splitFaceDelegate;
        }

        public Mesh<TVertex, TEdge, TFace> GetResult()
        {
            if (!triangulated)
            {
                Triangulate();
            }
            return mesh;
        }

        private void Triangulate()
        {
            // From O'Rourke (1994) Computational Geometry in C, section 2.1
            // Monotone Partitioning
            
            // To identify the chains: The vertices in each chain of a monotone
            // polygon are sorted with respect to the line of monotonicity [y-axis].
            // Then the vertices can be sorted by the y-axis in linear time: Find a
            // Highest vertex, find a lowest, and partition the boundary between the two
            // chains. The vertices in each chain are sorted with respect to y.
            // Two sorted lists of vertices can be merged in linear time into one list
            // sorted by y.
            Debug.Assert(face.EdgeCount > 3);
            var vertices = face.Vertices.Cast<TVertex>();

            IComparer<Point2D> pointComparer = new HighYLowXComparer();
            // TODO: Replace with Transform extension method. Consider removing transform comparer
            IComparer<TVertex> vertexComparer = new TransformComparer<TVertex, Point2D>(pointComparer, v => v.Position);
            Pair<TVertex, TVertex> minMax = vertices.MinMax(vertexComparer);
            meshUtilities = new MonotoneMeshUtilities<TVertex>(pointComparer);

            LeftToRightEdgeComparer xEdgeComparer = new LeftToRightEdgeComparer();
            sweeplineUtilities = new SweeplineUtilities(xEdgeComparer);

            // Assign each vertex to the left or right chain
            IEnumerable<ChainVertex> leftChainReversed = TraceLeftAndUp(minMax.First);
            IEnumerable<ChainVertex> leftChain  = leftChainReversed.Reverse();
            IEnumerable<ChainVertex> rightChain = TraceRightAndDown(minMax.Second);

            // From Berg et al (2000) Computational Geometry Algorithms and Applications

            // 1. Merge the vertices on the right chain with those on the left
            // chain into one sequence sorted on decreasing y-coordinate. If
            // two vertices have the same y-coordinate the left one comes first.
            // Let u1 to un denote the sorted sequence.

            IComparer<ChainVertex> chainVertexComparer = vertexComparer.Transform<ChainVertex, TVertex>(p => p.Vertex).Invert();
            IEnumerable<ChainVertex> mergedVertices = rightChain.MergeSorted(leftChain, chainVertexComparer);
            List<ChainVertex> u = new List<ChainVertex>(mergedVertices);

            // 2. Initialize an empty stack and push u1 and u2 onto it
            // This stack contains all the vertices of the polygon that have
            // already been encountered, but which may require additional diagonals
            Stack<ChainVertex> stack = new Stack<ChainVertex>();
            stack.Push(u[0]);
            stack.Push(u[1]);

            for (int j = 2; j < u.Count - 1; ++j)
            {
                // If u[j] and the vertext on top of the stack are of different chains
                if (u[j].Chain != stack.Peek().Chain)
                {
                    // Insert a diagonal from u[j] to each popped vertex...
                    // We progessively subdivide the new faces created by the splitting
                    // so we must keep track of which face to split
                    FaceBase faceToSplit = face;
                    while (stack.Count > 1)
                    {
                        TEdge edge = splitFace(mesh, faceToSplit, u[j], stack.Pop());
                        faceToSplit = edge.Faces.First; // First is always the new face
                    }
                    // ... except the last one
                    if (stack.Count > 0)
                    {
                        stack.Pop();
                    }
                    // Push u j-1 and uj onto the stack
                    stack.Push(u[j - 1]);
                    stack.Push(u[j]);
                }
                else
                {
                    // Pop one vertex from the stack; this vertex is already connected
                    ChainVertex previous = stack.Pop();
                    // We progressively split triangles from the original face
                    TFace faceToSplit = face;
                    while (stack.Count > 0 && CanInsertDiagonal(u[j], stack.Peek(), previous))
                    {
                        previous = stack.Pop();
                        splitFace(mesh, faceToSplit, u[j], previous);
                    }
                    // Push the last vertex popped back onto the stack
                    stack.Push(previous);
                    // Push uj onto the stack
                    stack.Push(u[j]);
                }
            }
            // Add diagonals from un to all vertices except the first and last one
            stack.Pop();
            // Which chain is the stack of vertices on - affects which face we split
            Chain stackChain = stack.Peek().Chain;
            FaceBase finalFaceToSplit = face;
            while (stack.Count > 1)
            {
                TEdge edge = splitFace(mesh, finalFaceToSplit, u[u.Count - 1], stack.Pop());
                finalFaceToSplit = stackChain == Chain.Left ? edge.Faces.First : edge.Faces.Second;
            }
            Debug.Assert(stack.Count == 1);

            triangulated = true;
        }

        // TODO: This could be an iterator
        private List<ChainVertex> TraceLeftAndUp(TVertex bottom)
        {
            TVertex vertex = bottom;
            List<ChainVertex> result = new List<ChainVertex>();
            IEnumerable<EdgeBase> aboveEdges;
            while ((aboveEdges = AboveEdges(vertex)).Count() > 0)
            {
                EdgeBase edge = sweeplineUtilities.LeftMost(aboveEdges);
                TVertex upper = meshUtilities.UpperVertex(edge);
                result.Add(new ChainVertex(upper, Chain.Left));
                vertex = upper;
            }
            return result;
        }

        // TODO: This could be an iterator
        private List<ChainVertex> TraceRightAndDown(TVertex top)
        {
            TVertex vertex = top;
            List<ChainVertex> result = new List<ChainVertex>();
            IEnumerable<EdgeBase> belowEdges;
            while ((belowEdges = BelowEdges(vertex)).Count() > 0)
            {
                EdgeBase edge = sweeplineUtilities.RightMost(belowEdges);
                TVertex lower = meshUtilities.LowerVertex(edge);
                result.Add(new ChainVertex(lower, Chain.Right));
                vertex = lower;
            }
            return result;
        }

        /// <summary>
        /// Find the edges which are above-indicent to the given vertex and
        /// indicent to the face we are triangulating
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        private IEnumerable<EdgeBase> AboveEdges(TVertex vertex)
        {
            return meshUtilities.AboveEdges(vertex).Where(e => e.Faces.First == face
                                                            || e.Faces.Second == face);
        }

        /// <summary>
        /// Find the edges which are below-indicent to the given vertex and
        /// indicent to the face we are triangulating
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        private IEnumerable<EdgeBase> BelowEdges(TVertex vertex)
        {
            return meshUtilities.BelowEdges(vertex).Where(e => e.Faces.First == face
                                                            || e.Faces.Second == face);
        }

        /// <summary>
        /// Checking whether a diagonal can be drawn from vj to a vertex vk
        /// still on the stack can be done by looking at vj, vk and the previous
        /// vertex that was popped.
        /// </summary>
        /// <param name="vj">The current vertex</param>
        /// <param name="vk">The candidate vertex to connect to</param>
        /// <param name="vp">The previous vertex to be popped from the stack</param>
        /// <returns></returns>
        private static bool CanInsertDiagonal(ChainVertex vj, ChainVertex vk, ChainVertex vp)
        {
            //Debug.Assert(vj.Chain == vk.Chain); Not sure whether this assertion is correct
            Triangle2D triangle = new Triangle2D(vj.Vertex.Position, vk.Vertex.Position, vp.Vertex.Position);
            if (triangle.Area == 0.0)
            {
                return false;
            }
            if (vj.Chain == Chain.Right)
            {
                return triangle.Handedness == Sense.Clockwise;
            }
            return triangle.Handedness == Sense.Counterclockwise;
        }

        /// <summary>
        /// The default delagate for splitting faces, used unless an alternative was supplied to the constructor.
        /// </summary>
        /// <param name="mesh">The mesh in which the face and vertices reside</param>
        /// <param name="face">The face to be split; continues to exist as one of the 'new' faces</param>
        /// <param name="v1">Vertex on the new edge</param>
        /// <param name="v2">Vertex on the new edge</param>
        /// <returns>A new face above the new diagonal.</returns>
        private static TEdge SplitFace(Mesh<TVertex, TEdge, TFace> mesh, FaceBase face, ChainVertex v1, ChainVertex v2)
        {
            if (v1.Chain < v2.Chain)
            {
                return mesh.SplitFace(face, v1.Vertex, v2.Vertex);
            }
            if (v1.Chain > v2.Chain)
            {
                return mesh.SplitFace(face, v2.Vertex, v1.Vertex);
            }
            Debug.Assert(v1.Chain == v2.Chain);
            if (v1.Chain == Chain.Right)
            {
                return mesh.SplitFace(face, v2.Vertex, v1.Vertex);
            }
            return mesh.SplitFace(face, v1.Vertex, v2.Vertex);
            //var faces = edge.Faces;
            //TFace otherFace = (TFace) faces.First;
            //return otherFace;
        }
    }
}
