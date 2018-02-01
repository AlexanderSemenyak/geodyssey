/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-05-01
 * Time: 11:24
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
	/// Description of SinuousFaultGene.
	/// </summary>
	public class SinuousFaultGene : Gene
	{
	    static double borderProportion = 0.2; // The random placement border width as a proportion of the domain diagonal length
	    static double tipTangentAngle = 30.0; // The angle that the tangent at the dip is allowed to diverge from the centreline
        static double minFaultDip     = 60.0; // Faults are listric so this is the maximum dip
	    static double maxFaultDip     = 70.0; //   on any given fault
        static double minFaultDisplacementLengthRatio = 10.0;
	    
	    private enum DimIndex
        {
            X0, Y0, Phi0,
            X1, Y1, Phi1,
            MaxHeave,
            DetachDepth,
            Dip
        }
       	
	    #region Construction
	    
	    public static SinuousFaultGene Create(Grid2DDomain domain, double x0, double y0, double phi0,
	                                                               double x1, double y1, double phi1,
	                                                               double maxHeave, double detachDepth, double dip)
	    {
	        Debug.Assert(dip > 0.0 && dip <= 90.0);
		    return new SinuousFaultGene(domain, x0, y0, phi0, x1, y1, phi1, maxHeave, detachDepth, dip);
	    }
	    
        //public static SinuousFaultGene CreateRandom(Grid2DDomain domain)
        //{
        //    double border   = Border(domain);
        //    double x0   = Rng.ContinuousUniform(domain.Min.X - border, domain.Max.X + border);
        //    double y0   = Rng.ContinuousUniform(domain.Min.Y - border, domain.Max.Y + border);
        //    double phi0 = Rng.ContinuousUniform(-tipTangentAngle, +tipTangentAngle);
        //    double x1   = Rng.ContinuousUniform(domain.Min.X - border, domain.Max.X + border);
        //    double y1   = Rng.ContinuousUniform(domain.Min.Y - border, domain.Max.Y + border);
        //    double phi1 = Rng.ContinuousUniform(-tipTangentAngle, +tipTangentAngle);
        //    double detachDepth = Rng.ContinuousUniform(MinDetachDepth(domain), MaxDetachDepth(domain));
        //    double maxHeave    = Rng.ContinuousUniform(MinMaxHeave(domain), MaxMaxHeave(domain), false);
        //    double dip         = Rng.ContinuousUniform(minFaultDip, maxFaultDip);
        //    return Create(domain, x0, y0, phi0, x1, y1, phi1, maxHeave, detachDepth, dip);
        //}

        public static SinuousFaultGene CreateRandom(Grid2DDomain domain)
        {
            // Select a point to be on the centre of a straight line
            // joining the fault tips
            double border = Border(domain);
            Point2D centre = new Point2D(Rng.ContinuousUniform(domain.Min.X - border, domain.Max.X + border),
                                          Rng.ContinuousUniform(domain.Min.Y - border, domain.Max.Y + border));

            // Select a fault orientation - select a dip azimuth from the
            // distribution, and convert it into a fault strike.
            double dipAzimuth = domain.RandomHistogramDipAzimuth();
            // Fault strike should be 90° anti-clockwise of surface dip
            double faultStrike = dipAzimuth - Math.PI / 2.0;

            // Select a fault length from the domain length scale distribution
            double faultLength = Rng.ContinuousUniformZeroToN((domain.Max - domain.Min).Magnitude);

            Vector2D halfCentreLine = new Vector2D(Math.Cos(faultStrike) * faultLength / 2.0,
                                                   Math.Sin(faultStrike) * faultLength / 2.0);

            Point2D p0 = centre - halfCentreLine;
            Point2D p1 = centre + halfCentreLine;

            double phi0 = Rng.ContinuousUniform(-tipTangentAngle, +tipTangentAngle);
            double phi1 = Rng.ContinuousUniform(-tipTangentAngle, +tipTangentAngle);

            double detachDepth = Rng.ContinuousUniform(MinDetachDepth(domain), MaxDetachDepth(domain));

            double maxFaultHeave = faultLength / minFaultDisplacementLengthRatio;
            double lowerFaultHeave = MinMaxHeave(domain);
            double upperFaultHeave = Math.Max(Math.Min(maxFaultHeave, detachDepth / 2.0) / 10.0, lowerFaultHeave);
            double maxHeave = Rng.ContinuousUniform(lowerFaultHeave, upperFaultHeave, false);
            double dip = Rng.ContinuousUniform(minFaultDip, maxFaultDip);

            return Create(domain, p0.X, p0.Y, phi0, p1.X, p1.Y, phi1, maxHeave, detachDepth, dip);
        }

	    private SinuousFaultGene(Grid2DDomain domain, double x0, double y0, double phi0,
	                                                  double x1, double y1, double phi1,
	                                                  double maxHeave, double detachDepth, double dip) :
	        base(domain, System.Enum.GetValues(typeof(DimIndex)).Length)
	    {
	        Dimensions[(int)DimIndex.X0]    = new Dimension(x0, XMutationStrategy(domain), XRangeStrategy(domain));
	        Dimensions[(int)DimIndex.Y0]    = new Dimension(y0, YMutationStrategy(domain), YRangeStrategy(domain));
	        Dimensions[(int)DimIndex.Phi0]  = new Dimension(phi0, PhiMutationStrategy(domain), PhiRangeStrategy(domain));
	        Dimensions[(int)DimIndex.X1]    = new Dimension(x1, XMutationStrategy(domain), XRangeStrategy(domain));
	        Dimensions[(int)DimIndex.Y1]    = new Dimension(y1, YMutationStrategy(domain), YRangeStrategy(domain));
	        Dimensions[(int)DimIndex.Phi1]  = new Dimension(phi1, PhiMutationStrategy(domain), PhiRangeStrategy(domain));
	        Dimensions[(int)DimIndex.DetachDepth] = new Dimension(detachDepth, DetachDepthMutationStrategy(domain), DetachDepthRangeStrategy(domain));
	        Dimensions[(int)DimIndex.MaxHeave] = new Dimension(maxHeave, MaxHeaveMutationStrategy(domain), MaxHeaveRangeStrategy(domain));
	        Dimensions[(int)DimIndex.Dip] = new Dimension(dip, DipMutationStrategy(domain), DipRangeStrategy(domain));
	    }
	    
	    private SinuousFaultGene(SinuousFaultGene other) :
	        base(other)
	    {
	    }
	    
	    #endregion
	    
	    #region Properties
	    
		public double X0
		{
		    get { return Dimensions[(int)DimIndex.X0].Value; }
		}
		
		public double Y0
		{
		    get { return Dimensions[(int)DimIndex.Y0].Value; }
		}
		
		public double Phi0
		{
		    get { return Dimensions[(int)DimIndex.Phi0].Value; }
		}
		
        public double X1
		{
		    get { return Dimensions[(int)DimIndex.X1].Value; }
		}
		
		public double Y1
		{
		    get { return Dimensions[(int)DimIndex.Y1].Value; }
		}
		
		public double Phi1
		{
		    get { return Dimensions[(int)DimIndex.Phi1].Value; }
		}
		
		public double DetachDepth
		{
		    get { return Dimensions[(int)DimIndex.DetachDepth].Value; }
		}
		
		public double MaxHeave
		{
		    get { return Dimensions[(int)DimIndex.MaxHeave].Value; }
		}
		
		public double Dip
		{
		    get { return Dimensions[(int)DimIndex.Dip].Value; }
		}
		
		#endregion
	    
	    #region Methods
	    
	    private static double Border(Grid2DDomain domain)
	    {
	        double diagonal = (domain.Max - domain.Min).Magnitude;
	        return diagonal * borderProportion;
	    }
	    
	    private static double MinDetachDepth(Grid2DDomain domain)
	    {
	        return 1000.0; // metres - Must be greater than 0.0
	    }
	    
	    private static double MaxDetachDepth(Grid2DDomain domain)
	    {
	         // TODO: Should have 3D bounds on domain
	        return Border(domain) / 2.0;
	    }
	    
	    private static double MinMaxHeave(Grid2DDomain domain)
	    {
	        return 25.0; // metres
	    }
	    
	    private static double MaxMaxHeave(Grid2DDomain domain)
	    {
	        return MaxDetachDepth(domain) / 20.0; // Reduced by factor of ten 
	    }
	    
	    private AbstractMutationStrategy XMutationStrategy(Grid2DDomain domain)
	    {
	        double range = (domain.Max.X - domain.Min.X) / 2.0; // Half model width
	        double resolution = 1.0;
	        return PowerLawMutationStrategy.Create(range, resolution);
	    }
	    
	    private AbstractRangeStrategy XRangeStrategy(Grid2DDomain domain)
	    {
	        double border = Border(domain);
	        return ClipRangeStrategy.Create(domain.Min.X - border, domain.Max.X + border);
	    }
	    
	    private AbstractMutationStrategy YMutationStrategy(Grid2DDomain domain)
	    {
	        double range = (domain.Max.Y - domain.Min.Y) / 2.0; // Half model width
	        double resolution = 1.0;
	        return PowerLawMutationStrategy.Create(range, resolution);
	    }
	    
	    private AbstractRangeStrategy YRangeStrategy(Grid2DDomain domain)
	    {
	        double border = Border(domain);
	        return ClipRangeStrategy.Create(domain.Min.Y - border, domain.Max.Y + border);
	    }
	    
	    private AbstractMutationStrategy PhiMutationStrategy(Grid2DDomain domain)
	    {
	        return PowerLawMutationStrategy.Create(tipTangentAngle, 1.0); // 1° resolution
	    }
	    
	    private AbstractRangeStrategy PhiRangeStrategy(Grid2DDomain domain)
	    {
	        return ModalRangeStrategy.Create(-tipTangentAngle, tipTangentAngle);
	    }
	    
	    private AbstractMutationStrategy DetachDepthMutationStrategy(Grid2DDomain domain)
	    {
	        double range = MaxDetachDepth(domain) / 2.0;
	        double resolution = 1.0;
	        return PowerLawMutationStrategy.Create(range, resolution);
	    }
	    
	    private AbstractRangeStrategy DetachDepthRangeStrategy(Grid2DDomain domain)
	    {
	        return ClipRangeStrategy.Create(MinDetachDepth(domain), MaxDetachDepth(domain));
	    }
	    
	    private AbstractMutationStrategy MaxHeaveMutationStrategy(Grid2DDomain domain)
	    {
	        double range = MaxMaxHeave(domain) / 2.0;
	        double resolution = 1.0f;
	        return PowerLawMutationStrategy.Create(range, resolution);
	    }
	    
	    private AbstractRangeStrategy MaxHeaveRangeStrategy(Grid2DDomain domain)
	    {
	        return ClipRangeStrategy.Create(MinMaxHeave(domain), MaxMaxHeave(domain));
	    }
	    
	    private AbstractMutationStrategy DipMutationStrategy(Grid2DDomain domain)
	    {
	        // Should have ~1° resolution
	        double range = (maxFaultDip - minFaultDip) / 2.0;
	        double resolution = 1.0;
	        return PowerLawMutationStrategy.Create(range, resolution);
	    }
	    
	    private AbstractRangeStrategy DipRangeStrategy(Grid2DDomain domain)
	    {
	        return ReflectRangeStrategy.Create(minFaultDip, maxFaultDip);
	    }
	    
		public override bool Accept(GeneVisitor gv)
		{
		    ISinuousFaultGeneVisitor v = gv as ISinuousFaultGeneVisitor;
		    if (v != null)
		    {
		        v.Visit(this);
		        return true;
		    }
		    return false;
		}
		
		public override object Clone()
		{
		    return new SinuousFaultGene(this);
		}
		
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SinuousFaultGene [");
            sb.AppendFormat("\n  X0          = {0}", X0.ToString());
            sb.AppendFormat("\n  Y0          = {0}", Y0.ToString());
            sb.AppendFormat("\n  Phi0        = {0}", Phi0.ToString());
            sb.AppendFormat("\n  X1          = {0}", X1.ToString());
            sb.AppendFormat("\n  Y1          = {0}", Y1.ToString());
            sb.AppendFormat("\n  Phi1        = {0}", Phi1.ToString());
            sb.AppendFormat("\n  DetachDepth = {0}", DetachDepth.ToString());
            sb.AppendFormat("\n  MaxHeave    = {0}", MaxHeave.ToString());
            sb.AppendFormat("\n  Dip         = {0}", Dip.ToString());
            sb.Append("\n ]\n");
            return sb.ToString();
        }
		
        #endregion
	}
}
