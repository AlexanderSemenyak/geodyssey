using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geometry.Triangulation
{
    internal class IndexedEdge : IEquatable<IndexedEdge>
    {
        public int p1;
        public int p2;

        public IndexedEdge()
			: this(0, 0)
		{
		}

        public IndexedEdge(int point1, int point2)
		{
			p1 = point1; p2 = point2;
		}

        public bool Equals(IndexedEdge other)
		{
			return
				((this.p1 == other.p2) && (this.p2 == other.p1)) ||
				((this.p1 == other.p1) && (this.p2 == other.p2));
		}
    }
}
