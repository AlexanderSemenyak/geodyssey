using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Geometry.HalfEdge;

namespace Geometry.PolygonPartitioning
{
    internal class SweeplineUtilities
    {
        public IComparer<EdgeBase> comparer;

        public SweeplineUtilities(IComparer<EdgeBase> comparer)
        {
            this.comparer = comparer;
        }

        /// <summary>
        /// Returns the rightmost of a sequence of edges
        /// </summary>
        /// <param name="edges">A sequence of edges</param>
        /// <returns>The rightmost (i.e. most +ve x) of the edges</returns>
        public EdgeBase RightMost(IEnumerable<EdgeBase> edges)
        {
            Debug.Assert(edges != null);
            return edges.OrderByDescending(edge => edge, comparer).First();
        }

        /// <summary>
        /// Returns the leftmost of a sequence of edges
        /// </summary>
        /// <param name="edges">A sequence of edges</param>
        /// <returns>The leftmost (i.e. most -ve x) of the edges</returns>
        public EdgeBase LeftMost(IEnumerable<EdgeBase> edges)
        {
            Debug.Assert(edges != null);
            return edges.OrderBy(edge => edge, comparer).First();
        }
    }
}
