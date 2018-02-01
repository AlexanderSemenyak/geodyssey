using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geometry.RedBlue
{
    /// <summary>
    /// Very simple singly linked-list of Segments
    /// </summary>
    // TODO: Replace this with LinkedList from the BCL
    internal class SingleLL// : IEnumerable<Segment>
    {
        public Segment item;
        public SingleLL next;

        public SingleLL(Segment i, SingleLL n)
        {
            item = i;
            next = n;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(item);
            if (next != null)
            {
                sb.Append(", ");
                sb.Append(next);
            }
            return sb.ToString();
        }
    }
}
