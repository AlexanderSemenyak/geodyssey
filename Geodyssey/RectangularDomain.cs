using System;
using System.Collections.Generic;
using System.Text;

using Numeric;

namespace Geodyssey
{
    public abstract class RectangularDomain : Domain
    {

        private Point2D min;
        private Point2D max;

        #region Construction

        protected RectangularDomain(Point2D min, Point2D max)
        {
            this.min = min;
            this.max = max;
        }

        protected RectangularDomain(RectangularDomain domain) :
            this(domain.min, domain.max)
        {
        }

        protected RectangularDomain(Phenotype phenotype) :
            base(phenotype)
        {
        }

        #endregion

        #region Properties

        public Point2D Min
        {
            get { return min; }
            protected set { min = value; }
        }

        public Point2D Max
        {
            get { return max; }
            protected set { max = value; }
        }

        #endregion
    }
}
