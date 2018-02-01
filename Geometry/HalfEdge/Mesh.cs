using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Wintellect.PowerCollections;

using Utility.Collections;
using Utility.Extensions.System.Collections.Generic;


namespace Geometry.HalfEdge
{
    public class Mesh<TVertex, TEdge, TFace> : ICloneable
        where TVertex : VertexBase
        where TEdge : EdgeBase, new()
        where TFace : FaceBase, new()
    {
        // TODO: Could use hash tables here...
        private readonly Set<TVertex> vertices = new Set<TVertex>();
        private readonly List<TEdge> edges = new List<TEdge>();
        private readonly List<TFace> faces = new List<TFace>();
        private readonly bool allowLoopEdges;
        private readonly bool allowParallelEdges;

        public Mesh()
        {
            allowLoopEdges = false;
            allowParallelEdges = false;
        }

        public Mesh(IEnumerable<TVertex> vs) :
            this()
        {
            AddRange(vs);
        }

        public Mesh(bool allowLoopEdges, bool allowParallelEdges)
        {
            this.allowLoopEdges = allowLoopEdges;
            this.allowParallelEdges = allowParallelEdges;
        }

        public Mesh(IEnumerable<TVertex> vs, bool allowLoopEdges, bool allowParallelEdges) :
            this(allowLoopEdges, allowParallelEdges)
        {
            AddRange(vs);
        }

        /// <summary>
        /// Copy-constructor - deep copy including all vertices,
        /// edges and faces.
        /// </summary>
        /// <param name="other"></param>
        public Mesh(Mesh<TVertex, TEdge, TFace> other)
        {
            // TODO: Implement Mesh cctor!
            throw new NotImplementedException();
        }

        public void Clear()
        {
            faces.Clear();
            edges.Clear();
            vertices.Clear();
        }

        public IEnumerable<TVertex> Vertices
        {
            get
            {
                foreach (TVertex vertex in vertices)
                {
                    yield return vertex;
                }
            }
        }

        public IEnumerable<TEdge> Edges
        {
            get
            {
                foreach (TEdge edge in edges)
                {
                    yield return edge;
                }
            }
        }

        public IEnumerable<TFace> Faces
        {
            get
            {
                foreach (TFace face in faces)
                {
                    yield return face;
                }
            }
        }

        public int VertexCount
        {
            get { return vertices.Count; }
        }

        public int EdgeCount
        {
            get { return edges.Count;  }
        }

        public int FaceCount
        {
            get { return faces.Count;  }
        }

        public TVertex Add(TVertex vertex)
        {
            // TODO: Disconnect vertex?
            TVertex foundVertex;
            if (vertices.TryGetItem(vertex, out foundVertex))
            {
                return foundVertex;
            }
            vertices.Add(vertex);
            return vertex;
        }

        public void AddRange(IEnumerable<TVertex> vs)
        {
            foreach (TVertex vertex in vs)
            {
                Add(vertex);
            }
        }

        public TEdge AddEdge(TVertex fromVertex, TVertex toVertex)
        {
            return AddEdge(fromVertex, toVertex, new TEdge());
        }

        /// <summary>
        /// Inserts a copy of edge into the mesh between fromVertex and toVertex, creating two
        /// 
        /// </summary>
        /// <param name="fromVertex"></param>
        /// <param name="toVertex"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        public TEdge AddEdge(TVertex fromVertex, TVertex toVertex, TEdge edge)
        {
            if (fromVertex == null)
            {
                throw new ArgumentNullException("fromVertex");
            }

            if (toVertex == null)
            {
                throw new ArgumentNullException("toVertex");
            }

            if (!vertices.Contains(fromVertex))
            {
                throw new ArgumentException("Vertex does not exist in mesh", "fromVertex");
            }

            if (!vertices.Contains(toVertex))
            {
                throw new ArgumentException("Vertex does not exist in mesh", "toVertex");
            }

            if (edge == null)
            {
                throw new ArgumentNullException("edge");
            }

            // Decide what to do with loop edges
            if (!allowLoopEdges)
            {
                if (fromVertex == toVertex)
                {
                    return new TEdge();
                }
            }

            // Decide what to do with loop edges
            if (!allowParallelEdges)
            {
                Half existingHalf = FindHalf(fromVertex, toVertex);
                if (existingHalf != null)
                {
                    // TODO: Do we assign the value of the parameter edge to the existing edge?
                    return existingHalf.Edge as TEdge;
                }
            }

            // Allocate data
            Half fromToHalf = new Half();
            Half toFromHalf = new Half();

            // Initialize data
            edge.half = fromToHalf;

            fromToHalf.next = toFromHalf;
            fromToHalf.previous = toFromHalf;
            fromToHalf.pair = toFromHalf;
            fromToHalf.Source = fromVertex;
            fromToHalf.Edge = edge;
            fromToHalf.Face = null;

            toFromHalf.next = fromToHalf;
            toFromHalf.previous = fromToHalf;
            toFromHalf.pair = fromToHalf;
            toFromHalf.Source = toVertex;
            toFromHalf.Edge = edge;
            toFromHalf.Face = null;

            // Link the from side of the edge
            if (fromVertex.IsIsolated)
            {
                fromVertex.half = fromToHalf;
            }
            else
            {
                Half fromIn = FindFreeIncident(fromVertex);
                Half fromOut = fromIn.next;

                fromIn.next = fromToHalf;
                fromToHalf.previous = fromIn;

                toFromHalf.next = fromOut;
                fromOut.previous = toFromHalf;
            }

            if (toVertex.IsIsolated)
            {
                toVertex.half = toFromHalf;
            }
            else
            {
                Half toIn = FindFreeIncident(toVertex);
                Half toOut = toIn.next;

                toIn.next = toFromHalf;
                toFromHalf.previous = toIn;

                fromToHalf.next = toOut;
                toOut.previous = fromToHalf;
            }

            edges.Add(edge);

            return edge;
        }

        /// <summary>
        /// Find the TVertex that compares equal to the provided vertex. Uses the Equality comparison
        /// of TVertex.
        /// </summary>
        /// <param name="v"></param>
        /// <returns>A vertex equal to the parameter if it exists in the Mesh, or null.</returns>
        public TVertex Find(TVertex v)
        {
            TVertex foundVertex;
            return vertices.TryGetItem(v, out foundVertex) ? foundVertex : null;
        }

        /// <summary>
        /// Find the edge connecting 
        /// </summary>
        /// <param name="v1">The first vertex</param>
        /// <param name="v2">The second vertex</param>
        /// <returns>The edge between first vertex and second vertex, or null if none exists</returns>
        public TEdge Find(VertexBase v1, VertexBase v2)
        {
            Half half = FindHalf(v1, v2);
            return half != null ? (TEdge) half.Edge : null;
        }

        /// <summary>
        /// Find an existing half-edge between fromVertex to toVertex
        /// </summary>
        /// <param name="fromVertex">The hald-edge source.</param>
        /// <param name="toVertex">The half-edge target.</param>
        /// <returns>An existing half-edge, or null.</returns>
        private static Half FindHalf(VertexBase fromVertex, VertexBase toVertex)
        {
            Half result = fromVertex.FindHalfEdge(half => half.Target == toVertex);
            return result;
        }

        /// <summary>
        /// Given a half-edge halfIn arriving at a vertex V, and a half-edge halfOut leaving from V,
        /// order, if possible, the neighbourhood of V such that halfIn is a predecessor of halfOut
        /// and halfOut is a successor of halfIn. For this to make sense, halfIn and halfOut must
        /// both be free (have null faces to the left).
        /// 
        /// </summary>
        /// <param name="halfIn">An inbound half-edge to the vertex</param>
        /// <param name="halfOut">An outbound half-edge from the vertex</param>
        /// <returns>true if the re-ordering was successful</returns>
        private static bool MakeAdjacent(Half halfIn, Half halfOut)
        {
            Debug.Assert(halfIn.Face == null);
            Debug.Assert(halfOut.Face == null);
            Debug.Assert(halfIn.Target == halfOut.Source);

            if (halfIn.next == halfOut && halfOut.previous == halfIn)
            {
                // Adjacency is already correct
                return true;
            }

            Half b = halfIn.next;
            Half d = halfOut.previous;

            // Find a free incident half-edge after halfOut and before halfIn
            Half g = FindFreeIncident(halfOut.Source, halfOut.pair, halfIn);
            if (g == null)
            {
                // There is no such half-edge
                return false;
            }
            Half h = g.next;

            halfIn.next = halfOut;
            halfOut.previous = halfIn;

            g.next = b;
            b.previous = g;

            d.next = h;
            h.previous = d;

            return true;
        }

        /// <summary>
        /// Find a free incident half-edge around vertex.
        /// </summary>
        /// <param name="vertex">The vertex around which the search is to be performed</param>
        /// <returns>An inbound half-edge with a null face, or null if none could be found.</returns>
        private static Half FindFreeIncident(VertexBase vertex)
        {
            Half incident = vertex.half.pair;
            return FindFreeIncident(vertex, incident, incident);
        }

        /// <summary>
        /// Find a free incident half-edge after half1 and before half2. Here 'free' is defined as
        /// as having a null face to the left. The edge should be inbound to the query vertex.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static Half FindFreeIncident(VertexBase vertex, Half begin, Half end)
        {
            Debug.Assert(begin.Target == vertex);
            Debug.Assert(end.Target == vertex);
            Debug.Assert(begin.Target == end.Target);
            Half current = begin;
            do
            {
                if (current.IsFree)
                {
                    return current;
                }
                current = current.VertexNext;
            }
            while (current != end);
            return null;
        }

        // TODO: Euler operators from http://www.cgal.org/Manual/3.2/doc_html/cgal_manual/HalfedgeDS_ref/Class_HalfedgeDS_decorator.html#Cross_link_anchor_573

        /// <summary>
        /// Split the face across a diagonal between two vertices a and b.
        /// The returned edge has a pair of faces accessible through its
        /// Faces property.  edge.Faces.First will always be the new face
        /// instance. edge.Faces.Second will always be the original reduced
        /// face instance that was supplied as a parameter.
        /// </summary>
        /// <param name="face">The face to be split</param>
        /// <param name="a">The start of the diagonal</param>
        /// <param name="b">The end of the diagonal</param>
        /// <returns></returns>
        public TEdge SplitFace(FaceBase face, VertexBase a, VertexBase b)
        {
            // Two vertices alone are not sufficient to determine a
            // unique face, so the face is also supplied. (The two
            // vertices may lie on the same boundary between two faces.
            Half p = a.FindHalfEdge(half => (half.Face == face));
            Half q = b.FindHalfEdge(half => (half.Face == face));

            Half h = p.previous;
            Half g = q.previous;

            return SplitFace(h, g);
        }

        /// <summary>
        /// Euler operator: Split the face incident to h and g into two faces with a new
        /// diagonal between the two vertices denoted by h and g respectively.
        /// The new face is to the right of the diagonal, the old face to the left.
        /// </summary>
        /// <param name="h">A half-edge indicent to a face to be split</param>
        /// <param name="g">A half-edge incident to a face to be split</param>
        /// <returns>The new diagonal</returns>
        public TEdge SplitFace(Half h, Half g)
        {
            #if DEBUG
                int euler = Euler;    
            #endif

            if (h.Face != g.Face)
            {
                throw new ArgumentException();
            }

            FaceBase originalFace = h.Face;
            TFace secondFace = (TFace) h.Face.Clone();

            // Re-assign the half-edges to which both faces are connected
            originalFace.half = h;
            secondFace.half = g;

            // Re-assign half-edges after h upto g inclusive to the new face
            Half begin = h.next;
            Half current = begin;
            do
            {
                Debug.Assert(current != null);
                Debug.Assert(current.Face == originalFace);
                current.Face = secondFace;
                current = current.next;
            }
            while (current != g.next);

            // Insert a new edge and half-edge pair
            // Allocate data
            TEdge edge = new TEdge();
            Half fromToHalf = new Half();
            Half toFromHalf = new Half();

            // Initialize data
            edge.half = fromToHalf;

            fromToHalf.next = h.next;
            fromToHalf.previous = g;
            fromToHalf.pair = toFromHalf;
            fromToHalf.Source = g.Target;
            fromToHalf.Edge = edge;
            fromToHalf.Face = secondFace;

            toFromHalf.next = g.next;
            toFromHalf.previous = h;
            toFromHalf.pair = fromToHalf;
            toFromHalf.Source = h.Target;
            toFromHalf.Edge = edge;
            toFromHalf.Face = originalFace;

            h.next.previous = fromToHalf;
            h.next = toFromHalf;

            g.next.previous = toFromHalf;
            g.next = fromToHalf;

            

            edges.Add(edge);
            faces.Add(secondFace);

            #if DEBUG
                // Assert the Euler invariant
                Debug.Assert(euler == Euler);
            #endif

            return edge;
        }

        /// <summary>
        /// Euler operator: Joins the two faces incident to the supplied half-edge. 
        /// </summary>
        /// <param name="h">A half-edge incident to one face, with its pair incident to the other face.</param>
        /// <returns>The predecessor of h around the face.</returns>
        public Half JoinFace(Half h)
        {
            #if DEBUG
                int euler = Euler;
            #endif
            throw new NotImplementedException();
        }

        /// <summary>
        /// Euler operator: Splits the vertex indicent to h and g into two vertices
        /// and connects them with a new edge. The new vertex is a copy of the first vertex.
        /// </summary>
        /// <param name="h">A half-edge incident to the vertex to be split</param>
        /// <param name="g">A half-edge incident to the vertex to be split</param>
        /// <returns>h.Next.Opposite - the new half-edge towards the new vertex</returns>
        public Half SplitVertex(Half h, Half g)
        {
            #if DEBUG
                int euler = Euler;
            #endif
            throw new NotImplementedException();
            #if DEBUG
                // Assert the Euler invariant
                Debug.Assert(euler == Euler);
            #endif
        }

        /// <summary>
        /// Euler operator: Joins the two vertices incident to h. The vertex denoted by h.Opposite
        /// is removed.
        /// </summary>
        /// <param name="h">A half-edge between the two vertices to be joined.</param>
        /// <returns>The predecessor of h.</returns>
        public Half JoinVertex(Half h)
        {
            #if DEBUG
                int euler = Euler;
            #endif
            throw new NotImplementedException();
            #if DEBUG
                // Assert the Euler invariant
                Debug.Assert(euler == Euler);
            #endif
        }

        /// <summary>
        /// Euler operator: Creates a new vertex, a clone of h.Vertex and connects it
        /// to each vertex incident to h.Face. h remains incident to the
        /// original Face. All other triangles are clones of this face.
        /// </summary>
        /// <param name="h">A half-edge incident to the face to be triangulated.</param>
        /// <returns>h.Next - a half-egde pointing to the new Vertex</returns>
        public Half CreateCenterVertex(Half h)
        {
            #if DEBUG
                int euler = Euler;
            #endif
            throw new NotImplementedException();
            #if DEBUG
                // Assert the Euler invariant
                Debug.Assert(euler == Euler);
            #endif
        }

        /// <summary>
        /// Euler operator: The inverse of CreateCenterVertex. Removed all incident half-edges to the
        /// vertex pointed to by g, and merges all incident faces. Only g.Face remains.
        /// The faces indicent to g.Vertex need not be triangular.
        /// </summary>
        /// <param name="g">A half-edge pointing to a vertex to be removed.</param>
        /// <returns>g.Previous </returns>
        public Half RemoveCenterVertex(Half g)
        {
            #if DEBUG
                int euler = Euler;
            #endif
            throw new NotImplementedException();
            #if DEBUG
                // Assert the Euler invariant
                Debug.Assert(euler == Euler);
            #endif
        }

        public void RemoveFace(TFace face)
        {
            face.ForEachHalfEdge(current => current.Face = null);
            faces.Remove(face);
        }

        private TEdge CreateEdge()
        {
            TEdge edge = new TEdge();
            edges.Add(edge);
            return edge;
        }

        public void Add(TFace face)
        {
            throw new NotImplementedException();
        }

        public bool Remove(VertexBase item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TEdge item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TFace item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The Euler characteristic V - E + F
        /// </summary>
        public int Euler
        {
            get { return VertexCount - EdgeCount + FaceCount;  }
        }

        /// <summary>
        /// Add the face described by the supplied vertices in either a clockwise or anticlockwise
        /// order. Note that they may be in a reversed order of the resulting face.
        /// </summary>
        /// <param name="vs">A complete list of vertices surrounding a proposed face.</param>
        /// <exception cref="ArgumentException">Thrown if the vertices do not propose a face
        /// which can be legalling inserted into the mesh.</exception>
        /// <returns>The new face</returns>
        public TFace AddFace(IEnumerable<TVertex> vs)
        {
            CircularLinkedList<Half> halves = new CircularLinkedList<Half>();
            vs.Wrap().ForEachPair(delegate(TVertex va, TVertex vb)
            {
                // Locate a half-edge directed between va and vb
                Half half = va.FindHalfEdge(h => h.Target == vb);
                if (half == null)
                {
                    throw new ArgumentException("No half-edge exists between successive vertices");
                }
                halves.AddLast(half);
            });
            TFace face = FaceFromHalfEdgeLoop(halves);
            return face;
        }

        public TFace AddFace(params TEdge[] es)
        {
            CircularLinkedList<Half> halves = new CircularLinkedList<Half>();
            es.ForEachPair(delegate(EdgeBase ea, EdgeBase eb)
            {
                Half ha = ea.half;
                Half hb = eb.half;

                Half hp = null;
                Half hq = null;

                if (ha.Target == hb.Source)
                {
                    hp = ha;
                    hq = hb;
                }
                else if (ha.pair.Target == hb.Source)
                {
                    hp = ha.pair;
                    hq = hb;
                }
                else if (ha.Target == hb.pair.Source)
                {
                    hp = ha;
                    hq = hb.pair;
                }
                else if (ha.pair.Target == hb.pair.Source)
                {
                    hp = ha.pair;
                    hq = hb.pair;
                }
                else
                {
                    throw new ArgumentException("The edges do not form a chain");
                }
                Debug.Assert(hp.Target == hq.Source);
                if (halves.Count == 0)
                {
                    halves.AddLast(hp);
                }
                halves.AddLast(hq);
            });
            TFace face = FaceFromHalfEdgeLoop(halves);
            return face;
        }

        /// <summary>
        /// Given a half edge incident to the face, trace edges through the
        /// mesh to identify the boundary of the face, and create the face.
        /// </summary>
        /// <param name="incidentHalf">A half-edge incident to the face</param>
        /// <returns>A new face, to which incidentHalf is part of the boundary</returns>
        public TFace AddFace(Half incidentHalf)
        {
            CircularLinkedList<Half> halfLoop = TraceHalfLoop(incidentHalf);
            return FaceFromHalfEdgeLoop(halfLoop);
        }

        /// <summary>
        /// Given a half-edge, trace half-edges head to tail through the mesh
        /// to find closed loops of half-edges.
        /// </summary>
        /// <param name="incidentHalf">The half edge at which tracing should begin</param>
        /// <returns>A circular linked list of half-edges which form a loop</returns>
        private static CircularLinkedList<Half> TraceHalfLoop(Half incidentHalf)
        {
            CircularLinkedList<Half> halfLoop = new CircularLinkedList<Half>();
            Half begin = incidentHalf;
            Half current = begin;
            do
            {
                if (current == null)
                {
                    throw new IncompleteHalfLoopException();
                }
                Debug.Assert(current != null);
                halfLoop.AddLast(current);
                current = current.next;
            }
            while (current != begin);
            return halfLoop;
        }

        /// <summary>
        /// Given a circular list of half edges which completely surround a proposed face, create the new face.
        /// The supplied half-edges need not be in an order compatible with the existing mesh, but the order of
        /// the supplied faces or other connections in the mesh may be modified to accommodate the new face.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <param name="halfLoop">The supplied halfLoop parameter did not propose a face which could be
        /// legally inserted into the mesh</param>
        /// <returns>The new face</returns>
        private TFace FaceFromHalfEdgeLoop(CircularLinkedList<Half> halfLoop)
        {

            if (!EdgesAreFree(halfLoop))
            {
                halfLoop = new CircularLinkedList<Half>(halfLoop.Select(h => h.pair).Reverse());
                if (!EdgesAreFree(halfLoop))
                {
                    throw new ArgumentException("The polygon would introduce a non-manifold condition");
                }
            }

            // Try to re-order the links to get a proper orientation
            halfLoop.ForEachPair(delegate(Half current, Half next)
            {
                if (!MakeAdjacent(current, next))
                {
                    throw new ArgumentException("The polygon would introduce a non-manifold condition");
                }
            });

            TFace face = new TFace {half = halfLoop.First.Value};
            faces.Add(face);

            foreach (Half half in halfLoop)
            {
                half.Face = face;
            }

            return face;
        }

        /// <summary>
        /// Determine whether all of the half-edges in halfLoop are not incident to a face.
        /// </summary>
        /// <param name="halfLoop">The loop of half-edges to examine</param>
        /// <returns>Returns true none of the half-edges in halfLoop are incident to a face, otherwise false.</returns>
        private static bool EdgesAreFree(CircularLinkedList<Half> halfLoop)
        {
            CircularLinkedListNode<Half> notFree = halfLoop.Find(half => !half.IsFree);
            return notFree == null;
        }

        /// <summary>
        /// Create a deep copy of the Mesh, calling Clone on all
        /// TVertex, TEdge and TFace objects.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new Mesh<TVertex, TEdge, TFace>(this);
        }
    }
}
