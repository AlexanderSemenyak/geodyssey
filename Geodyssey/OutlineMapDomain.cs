using System;
using System.Collections.Generic;
using System.Text;

using Numeric;

namespace Geodyssey
{
    public class OutlineMapDomain : RectangularDomain
    {
        public OutlineMapDomain() :
	        this(0.0, 0.0, 0.0, 0.0)
		{
		}
		
		public OutlineMapDomain(double minX, double minY, double maxX, double maxY) :
            base(new Point2D(minX, minY), new Point2D(maxX, maxY))
		{
		}

        public OutlineMapDomain(OutlineMap map) :
            base(new OutlineMapPhenotype(map))
        {
            Target.Domain = this;
            Min = new Point2D(map.MinX, map.MinY);
		    Max = new Point2D(map.MaxX, map.MaxY);
        }

        public OutlineMapDomain(RectangularDomain domain) :
            base(domain)
        {
        }

        public override Gene CreateRandomGene()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override Phenotype CreatePhenotype()
        {
            return new OutlineMapPhenotype(this);
        }

        public override ExpressionVisitor CreateExpressionVisitor()
        {
            return new OutlineMapExpressionVisitor((OutlineMapPhenotype) CreatePhenotype());
        }

        public override double[] Fitness(Phenotype trial)
        {
            return trial.Compare(Target);
        }
    }
}
