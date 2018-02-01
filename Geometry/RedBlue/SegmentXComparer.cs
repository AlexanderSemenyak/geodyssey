using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geometry.RedBlue
{
    internal class SegmentXComparer : IComparer<Segment>
    {
        /// <summary>
        /// Compares two segments according to their x co-ordinates of their start points.
        /// 
        /// </returns>
        /// <param name="lhs">The first segment to compare.</param>
        /// <param name="rhs">The second segment to compare.</param>
        public int Compare(Segment lhs, Segment rhs)
        {
            if (lhs.startPt.X < rhs.startPt.X)
            {
                return -1;
            }
            if (lhs.startPt.X == rhs.startPt.X)
            {
                if (lhs.startPt.Y > rhs.startPt.Y)
                {
                    return 1;
                }
                if (lhs.startPt.Y < rhs.startPt.Y)
                {
                    return -1;
                }
                return lhs.CompareTo(rhs);
            }
            return 1;
        }
    }
}
