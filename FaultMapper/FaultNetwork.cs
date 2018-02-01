using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Numeric;

namespace FaultMapper
{
    /// <summary>
    /// Map of the fault network, stored as a graph. Each graph vertex has a position.
    /// Each connected component of the fault network has an integer id. 
    /// </summary>
    public class FaultNetwork
    {
        private readonly List<SegmentNode> segmentNodes = new List<SegmentNode>();
        private readonly List<BranchNode> branchNodes = new List<BranchNode>();

        public SegmentNode CreateSegmentNode()
        {
            SegmentNode node = new SegmentNode();
            segmentNodes.Add(node);
            return node;
        }

        public BranchNode CreateBranchNode(Point2D position)
        {
            BranchNode node = new BranchNode(position);
            branchNodes.Add(node);
            return node;
        }

        /// <summary>
        /// A sequence of all of the points within the fault network in
        /// no particular order
        /// </summary>
        public IEnumerable<IPositionable2D> Points
        {
            get
            {
                foreach (SegmentNode node in segmentNodes)
                {
                    foreach (FaultPoint point in node.ExclusivePoints)
                    {
                        yield return point;
                    }
                }

                foreach (BranchNode node in branchNodes)
                {
                    yield return node;
                }
            }
        }

        public IEnumerable<SegmentNode> Segments
        {
            get
            {
                return segmentNodes;
            }
        }
    }
}
