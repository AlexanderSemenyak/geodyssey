using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Wintellect.PowerCollections;

using Numeric;
using BitImage;

namespace FaultMapper
{
    



    /// <summary>
    /// Contains the geometric representation of the fault network
    /// </summary>
    //public class FaultNetwork
    //{
    //    private List<FaultNetworkNode> components; // Reference to one node in each connected component of the fault network
    //}

    public abstract class FaultNetworkNode
    {
        public abstract int Degree
        {
            get;
        }

    }

    public abstract class FaultJoinNode : FaultNetworkNode
    {
        private Point2D position;

        protected FaultJoinNode(Point2D position)
        {
            this.position = position;
        }

        public Point2D Position
        {
            get { return position; }
        }
    }

    //public class NullPoint : FaultJoinNode
    //{
    //    private FaultSegment segmentOne;
    //    private FaultSegment segmentTwo;

    //    public override int Degree
    //    {
    //        get { return 2; }
    //    }
    //}



    public class FaultPoint
    {
        private Point2D position;
        private Point2D leftCutoffPosition;
        private Point2D rightCutoffPosition;
        private double? rightCutoff; // TODO: Should probably be a 3D point
        private double? leftCutoff;  // TODO: Should probably be a 3D point

        public FaultPoint(Point2D position)
        {
            this.position = position;
        }

        public Point2D Position
        {
            get { return position; }
        }

        public Point2D RightCutoffPosition
        {
            get { return rightCutoffPosition; }
            set { rightCutoffPosition = value; }
        }

        public Point2D LeftCutoffPosition
        {
            get { return leftCutoffPosition; }
            set { leftCutoffPosition = value; }
        }

        public double? CutoffRight
        {
            get { return rightCutoff; }
            set { rightCutoff = value;  }
        }

        public double? CutoffLeft
        {
            get { return leftCutoff; }
            set { leftCutoff = value; }
        }

        public double? Throw
        {
            get { return rightCutoff - leftCutoff; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}\t{1}", position.X, position.Y);
            return sb.ToString();
        }
    }
}
