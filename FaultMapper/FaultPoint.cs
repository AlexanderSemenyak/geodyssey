using System;
using System.Text;
using Numeric;

namespace FaultMapper
{
    /// <summary>
    /// A point on the fault map for a fault centerline. Includes other
    /// attributes such as the locations of fault horizon cutoffs.
    /// </summary>
    public class FaultPoint : IFaultMapPoint
    {
        private readonly SegmentNode parentNode;
        private readonly Point2D position;
        private Point3D? leftCutoff;
        private Point3D? rightCutoff;

        public FaultPoint(Point2D position, SegmentNode parentNode)
        {
            this.position = position;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other">The FaultMapPoint to be copied</param>
        FaultPoint(FaultPoint other)
        {
            position = other.position;
            leftCutoff = other.leftCutoff;
            rightCutoff = other.rightCutoff;
        }

        public Point2D Position
        {
            get { return position; }
        }

        public Point3D? RightCutoff
        {
            get { return rightCutoff; }
            set { rightCutoff = value; }
        }

        public Point3D? LeftCutoff
        {
            get { return leftCutoff; }
            set { leftCutoff = value; }
        }

        public double? Throw
        {
            get
            {
                if (leftCutoff.HasValue && rightCutoff.HasValue)
                {
                    return Math.Abs(leftCutoff.Value.Z - rightCutoff.Value.Z);
                }
                return null;
            }
        }

        public double? Heave
        {
            get
            {
                if (leftCutoff.HasValue && rightCutoff.HasValue)
                {
                    return (leftCutoff.Value.XY - rightCutoff.Value.XY).Magnitude;
                }
                return null;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            FaultPoint faultPoint = obj as FaultPoint;
            if (faultPoint == null)
            {
                return false;
            }

            return position.Equals(faultPoint.position)
                && leftCutoff.Equals(faultPoint.leftCutoff)
                && rightCutoff.Equals(faultPoint.rightCutoff);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ position.GetHashCode() ^ leftCutoff.GetHashCode() ^ rightCutoff.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}\t{1}", position.X, position.Y);
            return sb.ToString();
        }
    }
}