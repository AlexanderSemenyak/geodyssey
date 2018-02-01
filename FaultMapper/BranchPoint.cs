using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Wintellect.PowerCollections;

using Numeric;

namespace FaultMapper
{
    public class BranchPoint : FaultJoinNode
    {
        // TODO: May be ordered sets in future - based upon azimuth
        private Set<FaultSegment> outSegments = new Set<FaultSegment>();
        private Set<FaultSegment> inSegments = new Set<FaultSegment>();

        public BranchPoint(Point2D position) :
            base(position)
        {
            // TODO: Using the discrete node coordinates is a temporary
            //       measure during testing
        }

        public override int Degree
        {
            get { return outSegments.Count + inSegments.Count; }
        }

        internal void AddOutSegment(FaultSegment faultSegment)
        {
            Debug.Assert(faultSegment.PreviousBranch == this);
            outSegments.Add(faultSegment);
        }

        internal void AddInSegment(FaultSegment faultSegment)
        {
            Debug.Assert(faultSegment.NextBranch == this);
            inSegments.Add(faultSegment);
        }

        internal void RemoveOutSegment(FaultSegment faultSegment)
        {
            Debug.Assert(faultSegment.PreviousBranch != this);
            Debug.Assert(outSegments.Contains(faultSegment));
            outSegments.Remove(faultSegment);
        }

        internal void RemoveInSegment(FaultSegment faultSegment)
        {
            Debug.Assert(faultSegment.NextBranch != this);
            Debug.Assert(inSegments.Contains(faultSegment));
            inSegments.Remove(faultSegment);
        }

        internal bool ContainsInSegment(FaultSegment segment)
        {
            return inSegments.Contains(segment);
        }

        internal bool ContainsOutSegment(FaultSegment segment)
        {
            return outSegments.Contains(segment);
        }

        /// <summary>
        /// Given a fault segment which is either an InSegment or an OutSegment
        /// of this BranchPoint, determine the next in or out segment in an anticlockwise
        /// direction.
        /// </summary>
        /// <param name="segment">The query segment, must be connected to this BranchPoint</param>
        /// <returns>The next segment anticlockwise from the query segment</returns>
        internal FaultSegment AnticlockwiseSegmentFrom(FaultSegment segment)
        {
            // TODO: Efficiency - re-building the list and re-sorting the segments every time is wasteful
            Debug.Assert(ContainsInSegment(segment) || ContainsOutSegment(segment));
            List<Pair<double, FaultSegment>> segments = new List<Pair<double, FaultSegment>>(inSegments.Count + outSegments.Count);

            foreach (FaultSegment inSegment in inSegments)
            {
                Point2D pointAlongSegment = inSegment.Count == 0 ? inSegment.PreviousBranch.Position : inSegment.Last.Position;
                // TODO: Have reversed sense of arguments in Direction2D constructor call - possibly need reversing here too
                Direction2D direction = new Direction2D(Position, pointAlongSegment);
                segments.Add(new Pair<double,FaultSegment>(direction.Bearing, inSegment));
            }

            foreach (FaultSegment outSegment in outSegments)
            {
                Point2D pointAlongSegment = outSegment.Count == 0 ? outSegment.NextBranch.Position : outSegment.First.Position;
                // TODO: Have reversed sense of arguments in Direction2D constructor call - possibly need reversing here too
                Direction2D direction = new Direction2D(Position, pointAlongSegment);
                segments.Add(new Pair<double,FaultSegment>(direction.Bearing, outSegment));
            }

            segments.Sort();
            int index = segments.FindIndex(delegate(Pair<double, FaultSegment> bearingSegment) { return bearingSegment.Second == segment; });
            int nextIndex = (index + 1) % segments.Count;
            return segments[nextIndex].Second;
        }
    }
}
