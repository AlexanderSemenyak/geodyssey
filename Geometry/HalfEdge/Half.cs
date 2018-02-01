using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Geometry.HalfEdge
{
    [DebuggerDisplayAttribute("id = {id}")]
    public class Half
    {
        internal static int sid = 0;
        internal int id;

        internal Half next = null;
        internal Half previous = null;
        internal Half pair = null;

        private VertexBase source = null;
        private EdgeBase edge = null;
        private FaceBase face = null;

        public Half()
        {
            id = sid;
            ++sid;
        }

        /// <summary>
        /// The target vertex at the end of this half-edge
        /// </summary>
        public VertexBase Target
        {
            get { return pair.source; }
        }

        /// <summary>
        /// The source vertex at the start of this half-edge
        /// </summary>
        public VertexBase Source
        {
            get { return source; }
            set { source = value; }
        }

        /// <summary>
        /// The next outgoing half-edge around the Source vertex
        /// </summary>
        internal Half VertexNext
        {
            get { return pair.next; }
        }

        /// <summary>
        /// The previous outgoing half-edge around the Source vertex
        /// </summary>
        internal Half VertexPrevious
        {
            get { return previous.pair;  }
        }

        /// <summary>
        /// True if this half edge is not incident to a Face, otherwise False
        /// </summary>
        public bool IsFree
        {
            get { return face == null; }
        }

        public EdgeBase Edge
        {
            get { return edge; }
            internal set { edge = value; }
        }

        public FaceBase Face
        {
            get { return face; }
            internal set { face = value; }
        }
    }
}
