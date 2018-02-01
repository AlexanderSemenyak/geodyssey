using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Geometry.HalfEdge;
using Numeric;

namespace FaultMapper.HalfEdge
{
    public class FaultVertex : PositionedVertexBase
    {
        private readonly IPositionable2D point;

        public FaultVertex(IPositionable2D point)
        {
            this.point = point;
        }

        /// <summary>
        /// Copy constructor.  This is a
        /// shallow copy. The referred to FaultPoint will not be copied.
        /// </summary>
        /// <param name="other">The FaultMapPoint to be copied.</param>
        FaultVertex(FaultVertex other) :
            base(other)
        {
            point = other.point;
        }

        public override Point2D Position
        {
            get { return point.Position; }
        }

        public override object Clone()
        {
            return new FaultVertex(this);
        }
    }
}