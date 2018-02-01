using System;
using System.Collections.Generic;
using System.Text;

using Numeric;

namespace FaultMapper
{
    public class FaultSegment : FaultNetworkNode
    {
        // In the direction of the fault segment, which side is downthron
        public enum Polarity
        {
            Left,
            Right
        }

        private List<FaultPoint> interiorNodes = new List<FaultPoint>();
        private BranchPoint previousBranch = null;
        private BranchPoint nextBranch = null;
        private HalfStrand rightStrand = null;
        private HalfStrand leftStrand  = null;

        public override int Degree
        {
            get { return (previousBranch != null ? 1 : 0) + (nextBranch != null ? 1 : 0); }
        }

        /// <summary>
        /// The number of nodes in the segment, including branch points
        /// which may be connected at either end.
        /// </summary>
        public int Count
        {
            get { return interiorNodes.Count; }
        }

        public FaultPoint this[int index]
        {
            get { return interiorNodes[index]; }
        }

        public FaultPoint First
        {
            get { return interiorNodes[0]; }
        }

        public FaultPoint Last
        {
            get { return interiorNodes[Count - 1]; }
        }

        public BranchPoint PreviousBranch
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

        public BranchPoint NextBranch
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

        public HalfStrand RightStrand
        {
            get { return rightStrand; }
            set { rightStrand = value; }
        }

        public HalfStrand LeftStrand
        {
            get { return leftStrand; }
            set { leftStrand = value; }
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
        /// Add an existing FaultPoint instance to the end of the segment.
        /// </summary>
        /// <param name="faultPoint"></param>
        internal void Add(FaultPoint faultPoint)
        {
            interiorNodes.Add(faultPoint);
        }

        /// <summary>
        /// Add a new FaultPoint to the end of the segment.
        /// </summary>
        /// <param name="point">The position of the new FaultPoint</param>
        internal void Add(Point2D point)
        {
            FaultPoint faultPoint = new FaultPoint(point);
            Add(faultPoint);
        }
    }
}
