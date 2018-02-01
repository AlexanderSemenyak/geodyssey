using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Geometry.HalfEdge;

namespace FaultMapper.HalfEdge
{
    /// <summary>
    /// Edge type of edges that are inserted during monotonization of the mesh
    /// </summary>
    public class MonotonizingEdge : EdgeBase
    {
        public MonotonizingEdge() :
            base()
        {
        }

        public MonotonizingEdge(MonotonizingEdge other) :
            base(other)
        {
        }

        public override object Clone()
        {
            return new MonotonizingEdge(this);
        }
    }
}
