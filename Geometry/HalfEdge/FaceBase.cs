using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Geometry.HalfEdge
{
    public class FaceBase : ICloneable
    {
        private static int sid = 0;
        public int id;

        internal Half half = null;

        public FaceBase()
        {
            id = sid;
            ++sid;
        }

        /// <summary>
        /// Creates a face that is a copy of the supplied face.  The new face
        /// will have the same half edge as the original face, but the half-edge
        /// will not be connected to this face.  After using this constructor,
        /// this faces half edge should be reassigned.
        /// </summary>
        /// <param name="other">The face to copy</param>
        private FaceBase(FaceBase other) :
            this()
        {
            half = other.half;
        }

        public FaceBase(Half incidentHalf) :
            this()
        {
            this.half = incidentHalf;
        }

        public IEnumerable<VertexBase> Vertices
        {
            get
            {
                Half begin = half;
                Half current = begin;
                do
                {
                    Debug.Assert(current != null);
                    Debug.Assert(current.Face == this);
                    yield return current.Source;
                    current = current.next;
                }
                while (current != begin);
            }
        }

        public IEnumerable<EdgeBase> Edges
        {
            get
            {
                Half begin = half;
                Half current = begin;
                do
                {
                    Debug.Assert(current != null);
                    Debug.Assert(current.Face == this);
                    yield return current.Edge;
                    current = current.next;
                }
                while (current != begin);
            }
        }

        /// <summary>
        /// Apply the given action to all of the half-edges incident to this face.
        /// </summary>
        /// <param name="action">The action to apply to each Half-edge visited</param>
        public void ForEachHalfEdge(Action<Half> action)
        {
            Half begin = half;
            Half current = begin;
            do
            {
                Debug.Assert(current != null);
                Debug.Assert(current.Face == this);
                action(current);
                current = current.next;
            }
            while (current != begin);
        }

        /// <summary>
        /// Find a half-edge incident to this face which matches the predicate.
        /// </summary>
        /// <param name="predicate">A predicate to me matched by a half-edge.</param>
        /// <returns>The matching half-edge or null if none is found.</returns>
        public Half FindHalfEdge(Predicate<Half> predicate)
        {
            Half begin = half;
            Half current = begin;
            do
            {
                Debug.Assert(current != null);
                Debug.Assert(current.Face == this);
                if (predicate(current))
                {
                    return current;
                }
                current = current.next;
            }
            while (current != begin);
            return null;
        }

        #region ICloneable Members

        /// <summary>
        /// The number of edges (and vertices) around this face. O(N) in the number
        /// of incident edges.
        /// </summary>
        public int EdgeCount
        {
            get
            {
                int edgeCount = 0;
                ForEachHalfEdge(delegate { ++edgeCount; });
                return edgeCount;
            }
        }

        /// <summary>
        /// Creates a face that is a copy of the supplied face.  The new face
        /// will have the same half edge as the original face, but the half-edge
        /// will not be connected to this face.  After using this constructor,
        /// this faces half edge should be reassigned.
        /// </summary>
        /// <returns>A clone of this face.</returns>
        public virtual object Clone()
        {
            return new FaceBase(this);
        }

        #endregion
    }
}
