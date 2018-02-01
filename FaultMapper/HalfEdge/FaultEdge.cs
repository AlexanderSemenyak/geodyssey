using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Geometry.HalfEdge;

namespace FaultMapper.HalfEdge
{
    /// <summary>
    /// Subclass of Edgebase for representing edges which correspond to fault center lines
    /// </summary>
    public class FaultEdge : EdgeBase
    {
        /// <summary>
        /// The equivalent SegmentNode in the FaultNetwork
        /// </summary>
        private readonly SegmentNode node;

        public FaultEdge(SegmentNode node) :
            base()
        {
            this.node = node;
        }

        public FaultEdge(FaultEdge other) :
            base(other)
        {
            this.node = other.node;
        }

        public override object Clone()
        {
            return new FaultEdge(this);
        }
    }
}
