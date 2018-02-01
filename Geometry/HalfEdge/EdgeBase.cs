using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wintellect.PowerCollections;

namespace Geometry.HalfEdge
{
    public class EdgeBase :ICloneable
    {
        internal static int sid = 0;
        internal int id;
        internal Half half;

        public EdgeBase()
        {
            id = sid;
            ++sid;
        }

        public EdgeBase(EdgeBase other) :
            this()
        {
        }

        public VertexBase Source
        {
            get { return half.Source; }
        }

        public VertexBase Target
        {
            get { return half.pair.Source; }
        }

        // TODO: Replace with a .NET 4 tuple when the time comes
        public Pair<FaceBase, FaceBase> Faces
        {
            get { return new Pair<FaceBase, FaceBase>(half.Face, (half.pair != null) ? half.pair.Face : null); }
        }

        #region ICloneable Members

        public virtual object Clone()
        {
            return new EdgeBase(this);
        }

        #endregion
    }
}
