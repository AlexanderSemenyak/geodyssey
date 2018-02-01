using Numeric;

namespace FaultMapper
{
    /// <summary>
    /// An INode where more than one fault intersects
    /// </summary>
    public abstract class JoinNode : INode, IPositionable2D
    {
        private readonly Point2D position;

        protected JoinNode(Point2D position)
        {
            this.position = position;
        }

        public Point2D Position
        {
            get { return position; }
        }

        public abstract int Degree
        {
            get;
        }
    }
}