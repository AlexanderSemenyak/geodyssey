using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Wintellect.PowerCollections;

using Numeric;

namespace FaultMapper
{
    public class BranchNode : JoinNode
    {
        // TODO: May be ordered sets in future - based upon azimuth
        private readonly Set<SegmentNode> outSegments = new Set<SegmentNode>();
        private readonly Set<SegmentNode> inSegments = new Set<SegmentNode>();

        public BranchNode(Point2D position) :
            base(position)
        {
            // TODO: Using the discrete node coordinates is a temporary
            //       measure during testing
        }

        public override int Degree
        {
            get { return outSegments.Count + inSegments.Count; }
        }

        internal void AddOutSegment(SegmentNode segment)
        {
            Debug.Assert(segment.PreviousBranch == this);
            outSegments.Add(segment);
        }

        internal void AddInSegment(SegmentNode segment)
        {
            Debug.Assert(segment.NextBranch == this);
            inSegments.Add(segment);
        }

        internal void RemoveOutSegment(SegmentNode segment)
        {
            Debug.Assert(segment.PreviousBranch != this);
            Debug.Assert(outSegments.Contains(segment));
            outSegments.Remove(segment);
        }

        internal void RemoveInSegment(SegmentNode segment)
        {
            Debug.Assert(segment.NextBranch != this);
            Debug.Assert(inSegments.Contains(segment));
            inSegments.Remove(segment);
        }

        internal bool ContainsInSegment(SegmentNode segment)
        {
            return inSegments.Contains(segment);
        }

        internal bool ContainsOutSegment(SegmentNode segment)
        {
            return outSegments.Contains(segment);
        }

        ///// <summary>
        ///// Given a fault segment which is either an InSegment or an OutSegment
        ///// of this BranchPoint, determine the next in or out segment in an anticlockwise
        ///// direction.
        ///// </summary>
        ///// <param name="segment">The query segment, must be connected to this BranchPoint</param>
        ///// <returns>The next segment anticlockwise from the query segment</returns>
        //internal SegmentNode AnticlockwiseSegmentFrom(SegmentNode segment)
        //{
        //    // TODO: Efficiency - re-building the list and re-sorting the segments every time is wasteful
        //    Debug.Assert(ContainsInSegment(segment) || ContainsOutSegment(segment));
        //    List<Pair<double, SegmentNode>> segments = new List<Pair<double, SegmentNode>>(inSegments.Count + outSegments.Count);

        //    foreach (SegmentNode inSegment in inSegments)
        //    {
        //        Point2D pointAlongSegment = inSegment.Count == 0 ? inSegment.PreviousBranch.Position : inSegment.LastInterior.Position;
        //        // TODO: Have reversed sense of arguments in Direction2D constructor call - possibly need reversing here too
        //        Direction2D direction = new Direction2D(Position, pointAlongSegment);
        //        segments.Add(new Pair<double,SegmentNode>(direction.Bearing, inSegment));
        //    }

        //    foreach (SegmentNode outSegment in outSegments)
        //    {
        //        Point2D pointAlongSegment = outSegment.Count == 0 ? outSegment.NextBranch.Position : outSegment.FirstInterior.Position;
        //        // TODO: Have reversed sense of arguments in Direction2D constructor call - possibly need reversing here too
        //        Direction2D direction = new Direction2D(Position, pointAlongSegment);
        //        segments.Add(new Pair<double,SegmentNode>(direction.Bearing, outSegment));
        //    }

        //    segments.Sort();
        //    int index = segments.FindIndex(bearingSegment => bearingSegment.Second == segment);
        //    int nextIndex = (index + 1) % segments.Count;
        //    return segments[nextIndex].Second;
        //}
    }
}
