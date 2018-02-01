using Numeric;

namespace FaultMapper
{
    /// <summary>
    /// Marks a null point with zero finite displacement on a fault.
    /// </summary>
    public class NullPointNode : JoinNode
    {
        private SegmentNode segmentOne;
        private SegmentNode segmentTwo;

        public NullPointNode(Point2D position) :
            base(position)
        {
        }

        public override int Degree
        {
            get { return 2; }
        }
    }
}