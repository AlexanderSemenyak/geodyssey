using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geometry.HalfEdge;
using Numeric;

namespace Geometry.PolygonPartitioning
{
    /// <summary>
    /// Utiltity functions for assisting with performing sweepline
    /// based algorithms with Mesh objects.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    public class MonotoneMeshUtilities<TVertex>
        where TVertex :VertexBase, IPositionable2D
    {
        private readonly IComparer<Point2D> yxComparer;

        public MonotoneMeshUtilities(IComparer<Point2D> yxComparer)
        {
            this.yxComparer = yxComparer;
        }

        /// <summary>
        /// Determine whether this half-edge point upwards (+ve y-axis)
        /// </summary>
        /// <param name="half">The half-edge to be tested</param>
        /// <returns>true if the source is below the target</returns>
        public bool Upwards(Half half)
        {
            int cmp = UpwardsOrDownwards(half);
            return cmp > 0;
        }

        /// <summary>
        /// Determine whether this half-edge point downwards (-ve y-axis)
        /// </summary>
        /// <param name="half">The half-edge to be tested</param>
        /// <returns>true if the source is above the target</returns>
        public bool Downwards(Half half)
        {
            int cmp = UpwardsOrDownwards(half);
            return cmp < 0;
        }

        /// <summary>
        /// Determine the direction of a half-edge with respect to the y-axis.
        /// The comparison is done with a HighYLowXComparer.
        /// </summary>
        /// <param name="half">The half-edge to be tested</param>
        /// <returns>+1 if the edge is upwards. -1 if the edge is downwards </returns>
        /// <exception cref="ArgumentException">Thrown if half is geometrically degenerate</exception>
        private int UpwardsOrDownwards(Half half)
        {
            Point2D source = ((IPositionable2D) half.Source).Position;
            Point2D target = ((IPositionable2D) half.Target).Position;
            int cmp = yxComparer.Compare(target, source);
            if (cmp == 0)
            {
                throw new ArgumentException("Degenerate half-edge");
            }
            return cmp;
        }

        /// <summary>
        /// Return a sequence of the above-incident edges to the given vertex.
        /// </summary>
        /// <param name="vertex">A vertex</param>
        /// <returns>The sequence of above incidient vertices</returns>
        public IEnumerable<EdgeBase> AboveEdges(TVertex vertex)
        {
            return vertex.HalfEdges.Where(Upwards).Select(half => half.Edge);
        }

        /// <summary>
        /// Return a sequence of the below-incident edges to the given vertex.
        /// </summary>
        /// <param name="vertex">A vertex</param>
        /// <returns>The sequence of below indicent vertices</returns>
        public IEnumerable<EdgeBase> BelowEdges(TVertex vertex)
        {
            return vertex.HalfEdges.Where(Downwards).Select(half => half.Edge);
        }

        /// <summary>
        /// Given an edge return the upper vertex according to the comparator
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public TVertex UpperVertex(EdgeBase edge)
        {
            Point2D source = ((IPositionable2D) edge.Source).Position;
            Point2D target = ((IPositionable2D) edge.Target).Position;
            int cmp = yxComparer.Compare(source, target);
            if (cmp > 0)
            {
                return (TVertex) edge.Source;
            }
            return (TVertex) edge.Target;
        }

        /// <summary>
        /// Given an edge return the lower vertex according to the comparator
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public TVertex LowerVertex(EdgeBase edge)
        {
            Point2D source = ((IPositionable2D) edge.Source).Position;
            Point2D target = ((IPositionable2D) edge.Target).Position;
            int cmp = yxComparer.Compare(source, target);
            if (cmp < 0)
            {
                return (TVertex) edge.Source;
            }
            return (TVertex) edge.Target;
        }
    }
}
