using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Numeric;
using Utility.Collections;

namespace Geometry.HalfEdge
{
    /// <summary>
    /// Class for ordering a mesh as a planar subdivision and
    /// optionally inserting faces
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TEdge"></typeparam>
    /// <typeparam name="TFace"></typeparam>
    public class Orderer2D<TVertex, TEdge, TFace>
        where TVertex :VertexBase, IPositionable2D
        where TEdge :EdgeBase, new()
        where TFace :FaceBase, new()
    {
        private readonly Mesh<TVertex, TEdge, TFace> mesh;

        public Orderer2D(Mesh<TVertex, TEdge, TFace> mesh)
        {
            this.mesh = mesh;
        }

        public Mesh<TVertex, TEdge, TFace> GetResult()
        {
            // 1. for each vertex, check that the vertices are ordered in a 
            //    consistent fashion around the vertex.
            foreach (TVertex v in mesh.Vertices)
            {
                if (!IsOrdered(v))
                {
                    Order(v);
                }
            }


            // 2. starting from an edge with no face, progress around the face
            //    until the face is complete.
            foreach (TEdge edge in mesh.Edges)
            {
                if (edge.half.IsFree)
                {
                    mesh.AddFace(edge.half);
                }
                if (edge.half.pair.IsFree)
                {
                    mesh.AddFace(edge.half.pair);
                }
            }

            return mesh;
        }

        /// <summary>
        /// Determines whether the edges around the supplied vertex are ordered 
        /// geometrically in the plane.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        private static bool IsOrdered(TVertex vertex)
        {
            if (vertex.Degree <= 2)
            {
                return true;
            }

            CircularLinkedList<double> angles = new CircularLinkedList<double>();
            foreach (Half h in vertex.HalfEdges)
            {
                double bearing = Bearing(h);
                angles.AddLast(bearing);
            }
            double min = angles.Min();
            CircularLinkedListNode<double> node = angles.Find(min);
            CircularLinkedListNode<double> end = node;
            // Check that advancing from this minima they are sorted
            do
            {
                if (node.Value >= node.Next.Value)
                {
                    return false;
                }
                node = node.Next;
            }
            while (node != end.Previous);
            return true;
        }

        private static double Bearing(Half h)
        {
            Vector2D vector = ((IPositionable2D) h.Target).Position - ((IPositionable2D) h.Source).Position;
            double bearing = vector.Direction.Bearing;
            if (bearing < 0.0)
            {
                bearing = 2.0 * Math.PI + bearing;
            }
            return bearing;
        }

        /// <summary>
        /// Re-order the half-edges around vertex such that they
        /// are spatially sorted
        /// </summary>
        /// <param name="vertex"></param>
        private static void Order(TVertex vertex)
        {
            // Put the half-edges into order according to bearing
            SortedDictionary<double, Half> outEdges = new SortedDictionary<double, Half>();
            foreach (Half h in vertex.HalfEdges)
            {
                double bearing = Bearing(h);
                outEdges.Add(bearing, h);
            }
            // Copy into a circular linked-list for convenience
            CircularLinkedList<Half> outEdgesCycle = new CircularLinkedList<Half>(outEdges.Values);
            
            // Iterate around the list, correcting pointers as we go
            CircularLinkedListNode<Half> node = outEdgesCycle.First;
            do
            {
                Half previousInEdge = node.Previous.Value.pair;
                Half currentOutEdge = node.Value;

                Half currentInEdge = currentOutEdge.pair;
                Half nextOutEdge = node.Next.Value;

                previousInEdge.next = currentOutEdge;
                currentOutEdge.previous = previousInEdge;

                currentInEdge.next = nextOutEdge;
                nextOutEdge.previous = currentInEdge;

                node = node.Next;
            }
            while (node != outEdgesCycle.Last.Next);
            Debug.Assert(IsOrdered(vertex));
        }
    }
}
