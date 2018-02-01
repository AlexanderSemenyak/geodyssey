using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Geometry.HalfEdge;

namespace FaultMapper.HalfEdge
{
    /// <summary>
    /// A bounding box edge in the mesh
    /// </summary>
    public class BoundingEdge : EdgeBase
    {
        public BoundingEdge() :
            base()
        {
        }

        public BoundingEdge(BoundingEdge other) :
            base(other)
        {
        }

        public override object Clone()
        {
            return new BoundingEdge(this);
        }
    }
}
