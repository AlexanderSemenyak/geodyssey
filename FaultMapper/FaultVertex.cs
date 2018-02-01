using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Geometry.HalfEdge;
using Numeric;

namespace FaultMapper
{
    public class FaultVertex : VertexBase, IPositioned2D
    {
        private readonly IPositioned2D point;

        public FaultVertex(IPositioned2D point)
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

        public Point2D Position
        {
            get { return point.Position; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            FaultVertex otherFaultVertex = obj as FaultVertex;
            if (otherFaultVertex == null)
            {
                return false;
            }

            return base.Equals(obj) && point.Equals(otherFaultVertex.point);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ point.GetHashCode();
        }

        public override string ToString()
        {
            return point.ToString();
        }

        public override object Clone()
        {
            return new FaultVertex(this);
        }
    }
}
