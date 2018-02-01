/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-19
 * Time: 00:19
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Diagnostics;
using System.Text;

using Numeric;

namespace Geodyssey
{
	/// <summary>
	/// Description of LinearFaultGene.
	/// </summary>
	public class LinearFaultGene : Gene
	{
        private enum DimIndex
        {
            Rho,
            Theta,
            Throw
        }
       	    
	    public static LinearFaultGene Create(Grid2DDomain domain, double x, double y, double strike, double faultThrow)
	    {
	        Debug.Assert(strike >= 0.0 && strike < 360.0);
		    
		    double theta = 360.0 - strike;
		    double rho   = x * Math.Cos(theta) + y * Math.Sin(theta);
		    return new LinearFaultGene(domain, rho, theta, faultThrow);
	    }
	    
	    public static LinearFaultGene CreateRandom(Grid2DDomain domain)
	    {
	        double x = Rng.ContinuousUniform(domain.Min.X, domain.Max.X);
	        double y = Rng.ContinuousUniform(domain.Min.Y, domain.Max.Y);
	        double strike = Rng.ContinuousUniform(0.0, 360.0);
	        double faultThrow = Rng.ContinuousUniform(0.0, 50.0);
	        return Create(domain, x, y, strike, faultThrow);
	    }
	    
	    private LinearFaultGene(Grid2DDomain domain, double rho, double theta, double faultThrow) :
	        base(domain, System.Enum.GetValues(typeof(DimIndex)).Length)
	    {
	        Dimensions[(int)DimIndex.Rho]   = new Dimension(rho, RhoMutationStrategy(domain), RhoRangeStrategy(domain));
	        Dimensions[(int)DimIndex.Theta] = new Dimension(theta, ThetaMutationStrategy(domain), ThetaRangeStrategy(domain));
	        Dimensions[(int)DimIndex.Throw] = new Dimension(faultThrow, ThrowMutationStrategy(domain), ThrowRangeStrategy(domain));
	    }
	    
	    private LinearFaultGene(LinearFaultGene other) :
	        base(other)
	    {
	    }
	    
	    private AbstractMutationStrategy RhoMutationStrategy(Grid2DDomain domain)
	    {
	        double r = (domain.Max - domain.Min).Magnitude / 2.0; // Half model diagonal
	        double k = 9.0; // ~ 1 metre
	        return PowerLawMutationStrategy.Create(r, k);
	    }
	    
	    private AbstractRangeStrategy RhoRangeStrategy(Grid2DDomain domain)
	    {
	        double diagonal = (domain.Max - domain.Min).Magnitude;
	        return ClipRangeStrategy.Create(-diagonal, diagonal);
	    }
	    
	    private AbstractMutationStrategy ThetaMutationStrategy(Grid2DDomain domain)
	    {
	        // Half-circle range, with 1° resolution
	        return PowerLawMutationStrategy.Create(180.0, 7.49f);
	    }
	    
	    private AbstractRangeStrategy ThetaRangeStrategy(Grid2DDomain domain)
	    {
	        return ModalRangeStrategy.Create(0.0, 360.0);
	    }
	    
	    private AbstractMutationStrategy ThrowMutationStrategy(Grid2DDomain domain)
	    {
	        return PowerLawMutationStrategy.Create(50.0, 9.0);
	    }
	    
	    private AbstractRangeStrategy ThrowRangeStrategy(Grid2DDomain domain)
	    {
	        return ClipRangeStrategy.Create(0.0, 200.0);
	    }
	    
		public double Rho
		{
		    get { return Dimensions[(int)DimIndex.Rho].Value; }
		}
		
		public double Theta
		{
		    get { return Dimensions[(int)DimIndex.Theta].Value; }
		}
		
		public double Throw
		{
		    get { return Dimensions[(int)DimIndex.Throw].Value; }
		}
		
		public override bool Accept(GeneVisitor gv)
		{
		    ILinearFaultGeneVisitor v = gv as ILinearFaultGeneVisitor;
		    if (v != null)
		    {
		        v.Visit(this);
		        return true;
		    }
		    return false;
		}
		
		public override object Clone()
		{
		    return new LinearFaultGene(this);
		}
		
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("LinearFaultGene [");
            sb.Append("\n  Rho   = ");
            sb.Append(Dimensions[(int)DimIndex.Rho].ToString());
            sb.Append("\n  Theta = ");
            sb.Append(Dimensions[(int)DimIndex.Theta].ToString());
            sb.Append("\n  Throw = ");
            sb.Append(Dimensions[(int)DimIndex.Throw].ToString());
            sb.Append(" ]\n");
            return sb.ToString();
        }
	}
}
