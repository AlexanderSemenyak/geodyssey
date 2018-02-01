using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Geometry.HalfEdge;
using Numeric;

namespace FaultMapper.HalfEdge
{
    public abstract class PositionedVertexBase : VertexBase, IPositionable2D
    {
        protected PositionedVertexBase() :
            base()
        {
        }

        protected PositionedVertexBase(PositionedVertexBase other) :
            base(other)
        {
        }

        public abstract Point2D Position
        {
            get;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            PositionedVertexBase otherFaultVertex = obj as PositionedVertexBase;
            if (otherFaultVertex == null)
            {
                return false;
            }

            // We deliberately replace and do not call the base class implementation
            return Position.Equals(otherFaultVertex.Position);
        }

        public override int GetHashCode()
        {
            // We deliberately replace and do not call the base class implementation
            return Position.GetHashCode();
        }

        public override string ToString()
        {
            return Position.ToString();
        }
    }
}
