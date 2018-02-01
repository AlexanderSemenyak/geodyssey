using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Numeric;

namespace FaultMapper
{
    public class SegmentNode : INode
    {
        // In the direction of the fault segment, which side is downthrown
        public enum Polarity
        {
            Left,
            Right
        }

        private readonly List<FaultPoint> interiorNodes = new List<FaultPoint>();
        private BranchNode previousBranch = null;
        private BranchNode nextBranch = null;

        public virtual int Degree
        {
            get { return (previousBranch != null ? 1 : 0) + (nextBranch != null ? 1 : 0); }
        }

        /// <summary>
        /// Sequence of the points in the segment, including the branch points
        /// at either end, if they exist
        /// </summary>
        public IEnumerable<IPositionable2D> InclusivePoints
        {
            get
            {
                if (previousBranch != null)
                {
                    yield return previousBranch;
                }

                foreach (FaultPoint interiorNode in interiorNodes)
                {
                    yield return interiorNode;
                }

                if (nextBranch != null)
                {
                    yield return nextBranch;
                }
            }
        }

        /// <summary>
        /// Sequence of the points in the segment, excluding any branch points
        /// at either end
        /// </summary>
        public IEnumerable<IFaultMapPoint> ExclusivePoints
        {
            get
            {
                return interiorNodes.Cast<IFaultMapPoint>();
            }
        }

        ///// <summary>
        ///// The number of nodes in the segment, excluding branch points
        ///// which may be connected at either end.
        ///// </summary>
        //public int Count
        //{
        //    get { return interiorNodes.Count; }
        //}

        ///// <summary>
        ///// The interior node (excluding branch points) at the supplied index
        ///// </summary>
        ///// <param name="index">The index of the interior node</param>
        ///// <returns>The fault point at that index</returns>
        //public FaultPoint this[int index]
        //{
        //    get { return interiorNodes[index]; }
        //}

        ///// <summary>
        ///// The position at the supplied index including the terminating branch points,
        ///// if they exist.
        ///// </summary>
        //public IEnumerable<Point2D> InclusivePositions
        //{
        //    get
        //    {
        //        if (previousBranch != null)
        //        {
        //            yield return previousBranch.Position;
        //        }

        //        foreach (FaultPoint interiorNode in interiorNodes)
        //        {
        //            yield return interiorNode.Position;
        //        }

        //        if (nextBranch != null)
        //        {
        //            yield return nextBranch.Position;
        //        }
        //    }
        //}

        //public FaultPoint FirstInterior
        //{
        //    get { return interiorNodes[0]; }
        //}

        //public FaultPoint LastInterior
        //{
        //    get { return interiorNodes[Count - 1]; }
        //}

        public BranchNode PreviousBranch
        {
            get { return previousBranch; }
            set
            {
                if (value != null)
                {
                    previousBranch = value;
                    previousBranch.AddOutSegment(this);
                }
                else
                {
                    if (previousBranch != null)
                    {
                        previousBranch.RemoveOutSegment(this);
                    }
                    previousBranch = value;
                }
            }
        }

        public BranchNode NextBranch
        {
            get { return nextBranch; }
            set
            {
                if (value != null)
                {
                    nextBranch = value;
                    nextBranch.AddInSegment(this);
                }
                else
                {
                    if (nextBranch != null)
                    {
                        nextBranch.RemoveInSegment(this);
                    }
                    nextBranch = value;
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (previousBranch != null)
            {
                sb.AppendFormat("{0}\t{1}", previousBranch.Position.X, previousBranch.Position.Y);
                sb.AppendLine();
            }

            foreach (FaultPoint faultPoint in interiorNodes)
            {
                sb.AppendLine(faultPoint.ToString());
            }

            if (nextBranch != null)
            {
                sb.AppendFormat("{0}\t{1}", nextBranch.Position.X, nextBranch.Position.Y);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Add a new FaultPoint to the end of the segment.
        /// </summary>
        /// <param name="point">The position of the new FaultPoint</param>
        internal void Add(Point2D point)
        {
            FaultPoint faultPoint = new FaultPoint(point, this);
            interiorNodes.Add(faultPoint);
        }
    }
}
