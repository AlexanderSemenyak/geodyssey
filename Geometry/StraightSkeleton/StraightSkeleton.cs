using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Geometry.HalfEdge;
using Numeric;
using Utility.Extensions.System.Collections.Generic;
using Wintellect.PowerCollections;

namespace Geometry.StraightSkeleton
{
    public class StraightSkeleton<TVertex, TEdge, TFace>
        where TVertex :VertexBase, IPositionable2D
        where TEdge :EdgeBase, new()
        where TFace :FaceBase, new()
    {
        private readonly Mesh<TVertex, TEdge, TFace> figure;
        private bool skeletonized = false;
        /// <summary>
        /// Construct the straight skeleton from the planar straight line figure.
        /// </summary>
        /// <param name="graph"></param>
        public StraightSkeleton(Mesh<TVertex, TEdge, TFace> graph)
        {
            // TODO: Copy the figure into figure
            figure = (Mesh<TVertex, TEdge, TFace>) graph;
        }

        // TODO: Could provide other contructors to use if a triangulation is already available

        public Mesh<TVertex, TEdge, TFace> GetResult()
        {
            if (!skeletonized)
            {
                Skeletonize();
                skeletonized = true;
            }
            return figure;
        }

        // TODO: Are all of the vertices on wavefronts?
        internal class WavefrontVertex :VertexBase, IPositionable2D, IMoveable2D
        {
            private readonly TVertex graphVertex; // The vertex in the original figure
            private Point2D position;
            private readonly Vector2D velocity;

            /// <summary>
            /// Initialize a wavefront vertex. The initial position of the wavefront vertex is taken from the graphVertex.
            /// </summary>
            /// <param name="graphVertex">The vertex in the figure of the original figure from which this wavefront vertex is propagating</param>
            /// <param name="velocity">The velocity of this wavefront vertex.</param>
            internal WavefrontVertex(TVertex graphVertex, Vector2D velocity)
            {
                this.graphVertex = graphVertex;
                this.position = graphVertex.Position;
                this.velocity = velocity;
            }

            public TVertex GraphVertex
            {
                get { return graphVertex; }
            }

            public Point2D Position
            {
                get { return position; }
            }

            public Vector2D Velocity
            {
                get { return velocity; }
            }

            void UpdatePosition(double t)
            {
                // Just an idea for now
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// An edge which is the equivalent of an edge from the original figure
        /// </summary>
        internal class FigureEdge : EdgeBase
        {
            // These four vertices are used during construction of the graph
            private WavefrontVertex sourceLeftVertex;
            private WavefrontVertex sourceRightVertex;
            private WavefrontVertex targetLeftVertex;
            private WavefrontVertex targetRightVertex;

            public WavefrontVertex SourceLeftVertex
            {
                get { return sourceLeftVertex; }
                set
                {
                    Debug.Assert(sourceLeftVertex == null);
                    sourceLeftVertex = value;
                }
            }

            public WavefrontVertex SourceRightVertex
            {
                get { return sourceRightVertex; }
                set
                {
                    Debug.Assert(sourceRightVertex == null);
                    sourceRightVertex = value;
                }
            }

            public WavefrontVertex TargetLeftVertex
            {
                get { return targetLeftVertex; }
                set
                {
                    Debug.Assert(targetLeftVertex == null);
                    targetLeftVertex = value;
                }
            }

            public WavefrontVertex TargetRightVertex
            {
                get { return targetRightVertex; }
                set
                {
                    Debug.Assert(targetRightVertex == null);
                    targetRightVertex = value;
                }
            }
        }

        /// <summary>
        /// An edge inserted during triangulation of the planar subdivision
        /// </summary>
        internal class TriangulationEdge : EdgeBase
        {
            // These two vertices are used during construction of the graph
            private WavefrontVertex sourceWavefrontVertex;
            private WavefrontVertex targetWavefrontVertex;

            public WavefrontVertex SourceWavefrontVertex
            {
                get { return sourceWavefrontVertex; }
                set
                {
                    Debug.Assert(sourceWavefrontVertex == null);
                    sourceWavefrontVertex = value;
                }
            }

            public WavefrontVertex TargetWavefrontVertex
            {
                get { return targetWavefrontVertex; }
                set
                {
                    Debug.Assert(targetWavefrontVertex == null);
                    targetWavefrontVertex = value;
                }
            }
        }

        /// <summary>
        /// An edge inserted during triangulation of the planar subdivision
        /// </summary>
        internal class SpokeEdge :EdgeBase
        {
            // These two vertices are used during construction of the graph

        }

        /// <summary>
        /// An edge representing a propagating wavefront
        /// </summary>
        internal class WavefrontEdge :EdgeBase
        {

        }

        // A (triangular) face with dynamic (i.e. moving)
        // vertices
        internal class WavefrontFace :FaceBase
        {
            new IEnumerable<WavefrontVertex> Vertices
            {
                get { return base.Vertices.Cast<WavefrontVertex>(); }
            }

            new IEnumerable<WavefrontEdge> Edges
            {
                get { return base.Edges.Cast<WavefrontEdge>(); }
            }

            /// <summary>
            /// Determine the values of t at which this face becomes
            /// degenerate
            /// </summary>
            Pair<double, double> Roots
            {
                get
                {
                    Debug.Assert(EdgeCount == 3);
                    // TOOD: Solve the quadratic equation
                }
            }
        }

        internal SpokeEdge CreateSpokeEdge()
        {
            return new SpokeEdge();
        }

        private void Skeletonize()
        {
            // Duplicate the figure, but convert the types of vertices and edges
            // and maintain a mapping from the triangulation vertices to the wavefront vertices.
            var wavefrontMesh = new Mesh<WavefrontVertex, EdgeBase, WavefrontFace>();
            var wavefrontVertices = new Dictionary<VertexBase, List<WavefrontVertex>>();

            foreach (TVertex v in figure.Vertices)
            {
                WavefrontVertex wavefrontVertex = new WavefrontVertex(v, new Vector2D());
                wavefrontMesh.Add(wavefrontVertex);
                if (!wavefrontVertices.ContainsKey(v))
                {
                    wavefrontVertices.Add(v, new List<WavefrontVertex>());
                }
                wavefrontVertices[v].Add(wavefrontVertex);
            }

            // Now insert the edges from the figure
            foreach (TEdge e in figure.Edges)
            {
                WavefrontVertex from = wavefrontVertices[e.Source].First();
                WavefrontVertex to = wavefrontVertices[e.Target].First();
                wavefrontMesh.AddEdge(from, to, new FigureEdge());
            }

            // Triangulate the figure. All new edges inserted during the triangulation
            // are to be SpokeEdges. TODO: What about bounding-box edges?
            Algorithms.TriangulatePlanarSubdivision<WavefrontVertex, EdgeBase, WavefrontFace, TriangulationEdge>(wavefrontMesh, true);

            // Build the wavefront triangulation 
            //  - insert additional zero size triangles at the terminals of the figure
            //  - split each edge of the figure along its length so that otherwise adjacent
            //    triangles do not share an edge
            foreach (TVertex figureVertex in figure.Vertices)
            {
                // TODO: Ensure we pass the right vertex to the functions here
                if (figureVertex.Degree >= 2)
                {
                    ProcessNonTerminalVertex(wavefrontMesh, wavefrontVertices, figureVertex);
                }
                else if (figureVertex.Degree == 1)
                {
                    ProcessTerminalVertex(wavefrontMesh, wavefrontVertices, figureVertex);
                }

            }
        }

        /// <summary>
        /// Process a non-terminal vertex (a vertex that has two or more connected edges)
        /// 
        /// </summary>
        /// <param name="wavefrontMesh"></param>
        /// <param name="wavefrontVertices"></param>
        /// <param name="figureVertex"></param>
        private static void ProcessNonTerminalVertex(Mesh<WavefrontVertex, EdgeBase, WavefrontFace> wavefrontMesh, Dictionary<VertexBase, List<WavefrontVertex>> wavefrontVertices, TVertex figureVertex)
        {
            // Each vertex of v of G of degree d >= 2 is duplicated into d wavefront vertices.

            // Iterate over successive pairs of outgoing half-edges from this figureVertex
            // This iterates in an anticlockwise direction around the vertex
            foreach (Pair<Half, Half> figureEdgeHalves in figureVertex.HalfEdges
                .Where(e => e.Edge is FigureEdge)
                .Wrap()
                .SuccessivePairs())
            {
                WavefrontVertex wavefrontVertex = BisectingWavefrontVertex(figureEdgeHalves, figureVertex);
                InsertWavefrontVertex(wavefrontMesh, wavefrontVertices, wavefrontVertex);
                LabelWavefrontPrecursorEdges       (figureVertex, figureEdgeHalves, wavefrontVertex);
                LabelInterveningSpokePrecursorEdges(figureVertex, figureEdgeHalves, wavefrontVertex);
            }
        }

        private static void ProcessTerminalVertex(Mesh<WavefrontVertex, EdgeBase, WavefrontFace> wavefrontMesh, IDictionary<VertexBase, List<WavefrontVertex>> wavefrontVertices, TVertex figureVertex)
        {
            // 1. Create two wavefront vertices at ±135° from the incident figure edge
            //  1a. Locate the figure edge
            //  1b. Determine the direction of the figure edge away from the vertex
            Direction2D figureEdgeDirection = new Direction2D(3, 4); // TODO: Temporary - need direction from the terminal vertex along the edge

            Direction2D direction1 = AffineTransformation2D.Rotation(Angle.DegreesToRadians(+135.0)).Transform(figureEdgeDirection);
            Vector2D vector1 = new Vector2D(direction1, Constants.Sqrt2);
            WavefrontVertex wavefrontVertex1 = new WavefrontVertex(figureVertex, vector1);
            InsertWavefrontVertex(wavefrontMesh, wavefrontVertices, wavefrontVertex1);

            Direction2D direction2 = AffineTransformation2D.Rotation(Angle.DegreesToRadians(-135.0)).Transform(figureEdgeDirection);
            Vector2D vector2 = new Vector2D(direction2, Constants.Sqrt2);
            WavefrontVertex wavefrontVertex2 = new WavefrontVertex(figureVertex, vector2);
            InsertWavefrontVertex(wavefrontMesh, wavefrontVertices, wavefrontVertex2);
            
            // 2. Label the incoming figureEdge (wavefront precursors) with the two vertices at the correct end
            // 3. Label all of the incoming triangulation edge (spoke precursors) with the first wavefront vertex
            // 4. Create wavefront edge between the two wavefront vertices
            // 5. Identify the quadrilateral based by the edge created in step 4
            // 6. Split this quadrilateral by inserting a new edge from the second
            //    of the two wavefront vertices.

            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the references to the the new wavefront vertex on the trailing edge
        /// </summary>
        /// <param name="figureVertex"></param>
        /// <param name="figureEdgeHalves"></param>
        /// <param name="wavefrontVertex"></param>
        private static void LabelWavefrontPrecursorEdges(TVertex figureVertex, Pair<Half, Half> figureEdgeHalves, WavefrontVertex wavefrontVertex)
        {
            FigureEdge trailingEdge = (figureEdgeHalves.First.Edge as FigureEdge);
            if (trailingEdge.Source == figureVertex)
            {
                // Outgoing edge
                trailingEdge.SourceLeftVertex = wavefrontVertex;
            }
            else
            {
                // Incoming edge
                Debug.Assert(trailingEdge.Target == figureVertex);
                trailingEdge.TargetRightVertex = wavefrontVertex;
            }

            // Set the references to the new wavefront vertex on the leading edge
            FigureEdge leadingEdge = (figureEdgeHalves.Second.Edge as FigureEdge);
            if (leadingEdge.Source == figureVertex)
            {
                // Outgoing edge
                leadingEdge.SourceRightVertex = wavefrontVertex;
            }
            else
            {
                // Incoming edge
                Debug.Assert(leadingEdge.Target == figureVertex);
                leadingEdge.TargetLeftVertex = wavefrontVertex;
            }
        }

        /// Iterate through all the half edges between figureEdgeHalves.First and
        /// figureEdgeHalves.Second (exclusive), all of which will be TriangulationEdges</summary>
        /// and mark them up with the wavefrontVertex reference.
        /// <param name="figureVertex"></param>
        /// <param name="figureEdgeHalves"></param>
        /// <param name="wavefrontVertex"></param>
        private static void LabelInterveningSpokePrecursorEdges(TVertex figureVertex, Pair<Half, Half> figureEdgeHalves, WavefrontVertex wavefrontVertex)
        {
            Half current = NextOutgoingHalf(figureEdgeHalves.First);
            while (current != figureEdgeHalves.Second)
            {
                LabelSpokePrecursorEdge(figureVertex, wavefrontVertex, current);
                current = NextOutgoingHalf(current);
            }
        }

        private static void InsertWavefrontVertex(Mesh<WavefrontVertex, EdgeBase, WavefrontFace> wavefrontMesh, IDictionary<VertexBase, List<WavefrontVertex>> wavefrontVertices, WavefrontVertex wavefrontVertex)
        {
            
            wavefrontMesh.Add(wavefrontVertex);
            TVertex figureVertex = wavefrontVertex.GraphVertex;
            if (!wavefrontVertices.ContainsKey(figureVertex))
            {
                wavefrontVertices.Add(figureVertex, new List<WavefrontVertex>());
            }
            wavefrontVertices[figureVertex].Add(wavefrontVertex);
        }

        private static void LabelSpokePrecursorEdge(TVertex figureVertex, WavefrontVertex wavefrontVertex, Half current)
        {
            Debug.Assert(current.Edge is TriangulationEdge);
            var triangulationEdge = current.Edge as TriangulationEdge;
            if (triangulationEdge.Source == figureVertex)
            {
                // Outgoing edge
                triangulationEdge.SourceWavefrontVertex = wavefrontVertex;
            }
            else
            {
                // Incoming edge
                Debug.Assert(triangulationEdge.Target == figureVertex);
                triangulationEdge.TargetWavefrontVertex = wavefrontVertex;
            }
        }

        /// <summary>
        /// Given an outgoing half-edge from a vertex, return the
        /// next outgoing half-edge from the same vertex, anticlockwise
        /// around the vertex.
        /// </summary>
        /// <param name="half"></param>
        /// <returns></returns>
        private static Half NextOutgoingHalf(Half half)
        {
            return half.pair.next;
        }

        /// <summary>
        /// Given two outgoing hald-edges from figureVertex create a WavefrontVertex whose
        /// velocity vector bisects the two half-edges. Assumes the Second half-edge is
        /// anticlockwise of the first half-edge.
        /// </summary>
        /// <param name="figureEdgeHalves">A pair of half-edges originating from the same vertex, where Second is anticlockwise from First</param>
        /// <param name="figureVertex">A vertex whose position is coincident with the source of the two half-edges provided</param>
        /// <returns>A new WavefrontVertex with the same position as figureVertex with a velocity vector that bisects the half-edges.</returns>
        private static WavefrontVertex BisectingWavefrontVertex(Pair<Half, Half> figureEdgeHalves, TVertex figureVertex)
        {
            Point2D current = figureVertex.Position;
            Debug.Assert(current == ((IPositionable2D) figureEdgeHalves.First.Source).Position);
            Debug.Assert(current == ((IPositionable2D) figureEdgeHalves.Second.Source).Position);
            Point2D trailing = ((IPositionable2D) figureEdgeHalves.First.Target).Position;
            Point2D leading = ((IPositionable2D) figureEdgeHalves.Second.Target).Position;
            Vector2D incoming = current - trailing;
            Vector2D outgoing = leading - current;
            Direction2D bisector = VectorGeometry.Bisector(ref incoming, ref outgoing);
            // Determine the speed of wave propagation along the direction of the bisector
            double angle = (-incoming).Angle(outgoing);
            double speed = 1.0 / Math.Sin(angle);
            Vector2D velocity = new Vector2D(bisector, speed);
            return new WavefrontVertex(figureVertex, velocity);
        }
    }
}
