using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geometry.HalfEdge
{
    public class VertexBase : ICloneable
    {
        private static int sid = 0;
        private readonly int id;

        internal Half half = null;

        public VertexBase()
        {
            id = sid;
            ++sid;
        }

        public VertexBase(VertexBase other) :
            this()
        {
        }

        public bool IsIsolated
        {
            get { return half == null; }
        }

        /// <summary>
        /// Enumerates the outgoing half-edges of this vertex.
        /// </summary>
        public IEnumerable<Half> HalfEdges
        {
            get
            {
                if (!IsIsolated)
                {
                    Half begin = half;
                    Half current = begin;
                    do
                    {
                        yield return current;
                        current = current.VertexNext;
                    }
                    while (current != begin);
                }
            }
        }

        /// <summary>
        /// Applies an action to each outgoing half-edge around this vertex.
        /// </summary>
        /// <param name="action">An action to be applied to each half-edge.</param>
        // TODO: Replace uses of this with the HalfEdges enumeration 
        public void ForEachHalfEdge(Action<Half> action)
        {
            if (!IsIsolated)
            {
                Half begin = half;
                Half current = begin;
                do
                {
                    action(current);
                    current = current.VertexNext;
                }
                while (current != begin);
            }
        }

        public IEnumerable<EdgeBase> Edges
        {
            get
            {
                if (!IsIsolated)
                {
                    Half begin = half;
                    Half current = begin;
                    do
                    {
                        yield return current.Edge;
                        current = current.VertexNext;
                    }
                    while (current != begin);
                }
            }
        }

        /// <summary>
        /// Searches the out going half-edges surrounding this vertex.
        /// </summary>
        /// <returns>The first half-edge that matches the predicate, or null if none is found.</returns>
        // TODO: Replace uses of this with the HalfEdges enumeration
        public Half FindHalfEdge(Predicate<Half> predicate)
        {
            if (!IsIsolated)
            {
                Half begin = half;
                Half current = begin;
                do
                {
                    if (predicate(current))
                    {
                        return current;
                    }
                    current = current.VertexNext;
                }
                while (current != begin);
            }
            return null;
        }

        public int Degree
        {
            get
            {
                int degree = 0;
                ForEachHalfEdge(h => ++degree);
                return degree;
            }
        }

        #region ICloneable Members
        public virtual object Clone()
        {
            // TODO: Needs implementing
            return new VertexBase();
        }
        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            VertexBase otherVertex = obj as VertexBase;
            if (otherVertex == null)
            {
                return false;
            }

            return id == otherVertex.id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }
}
