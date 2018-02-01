/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-21
 * Time: 19:57
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Diagnostics;
using System.Text;

using Numeric;
using Model;

namespace Geodyssey
{
	/// <summary>
	/// Description of Domain.
	/// </summary>
	public class Grid2DDomain : RectangularDomain
	{
	    private int sizeI;
	    private int sizeJ;
        private ContinuousHistogramGenerator dipAzimuthHistogramGenerator;
	    
	    #region Construction
	    
	    public Grid2DDomain() :
	        this(0.0, 0.0, 0.0, 0.0, 0, 0)
		{
		}
		
		public Grid2DDomain(double minX, double minY, double maxX, double maxY, int sizeI, int sizeJ) :
            base(new Point2D(minX, minY), new Point2D(maxX, maxY))
		{
		    this.sizeI = sizeI;
		    this.sizeJ = sizeJ;
		}

        public Grid2DDomain(IRegularGrid2D grid) :
            base(new Grid2DPhenotype(grid))
        {
            Target.Domain = this;
            Min = new Point2D(grid.MinX, grid.MinY);
		    Max = new Point2D(grid.MaxX, grid.MaxY);
		    this.sizeI = grid.SizeI;
		    this.sizeJ = grid.SizeJ;
        }

		#endregion
		
		#region Properties
				
		public int SizeI
		{
		    get { return sizeI; }
		}
		
		public int SizeJ
		{
		    get {return sizeJ; }
		}
		
		#endregion
		
		#region Methods
		
		#region Factory Methods
		public override Gene CreateRandomGene()
		{
		    return SinuousFaultGene.CreateRandom(this);
		}
		
		public override Phenotype CreatePhenotype()
		{
		    return new Grid2DPhenotype(this);
		}
		
		public override ExpressionVisitor CreateExpressionVisitor()
		{
		    return new Grid2DExpressionVisitor((Grid2DPhenotype) CreatePhenotype());
		}
		#endregion

        public double RandomHistogramDipAzimuth()
        {
            if (dipAzimuthHistogramGenerator == null)
            {
                Grid2DPhenotype p = (Grid2DPhenotype) Target;
                IRegularGrid2D g = p.Grid;
                Histogram dipAzimuthHistogram = g.CreateDipAzimuthHistogram(24);
                dipAzimuthHistogramGenerator = new ContinuousHistogramGenerator(dipAzimuthHistogram);
            }
            double value = dipAzimuthHistogramGenerator.NextDouble();
            //Console.WriteLine("randomAzimuth = {0}", Angle.RadiansToDegrees(value));
            return value;
        }

        /// <summary>
        /// Return the multiobjective fitness vector for the supplied trial Phenotype.
        /// </summary>
        /// <param name="trial">The trial Phenotype whose fitness will be computed
        /// relative to the Domain target Phenotype.</param>
        /// <returns>A multiobjective fitness vector.</returns>
		public override double[] Fitness(Phenotype trial)
		{
		    return trial.Compare(Target);
		}

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Min ({0}, {1})\n", Min.X, Min.Y);
            sb.AppendFormat("Max ({0}, {1})\n", Max.X, Max.Y);
            sb.AppendFormat("Size ({0}, {1})\n", SizeI, SizeJ);
            return sb.ToString();
        }

		#endregion
	}
}
