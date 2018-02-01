using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Geometry.HalfEdge;
using Numeric;

namespace FaultMapper.HalfEdge
{
    /// <summary>
    /// Vertex used for bounding box
    /// </summary>
    public class BoundingVertex : PositionedVertexBase
    {
        private Point2D position;

        public BoundingVertex(Point2D position) :
            base()
        {
            this.position = position;
        }

        public BoundingVertex(BoundingVertex other) :
            base(other)
        {
            this.position = other.position;
        }

        public override Point2D Position
        {
            get { return position; }
        }

        public override object Clone()
        {
            return new BoundingVertex(this);
        }
    }
}
