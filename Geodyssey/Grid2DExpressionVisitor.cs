/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-22
 * Time: 13:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Diagnostics;
using System.Collections.Generic;

using Numeric;

namespace Geodyssey
{
	/// <summary>
	/// Description of ExpressionVisitor.
	/// </summary>
	public class Grid2DExpressionVisitor : ExpressionVisitor,
	                                       ILinearFaultGeneVisitor,
	                                       ISinuousFaultGeneVisitor
	{
	    private Grid2DPhenotype phenotype;
	    
	    # region Construction
	    
		public Grid2DExpressionVisitor(Grid2DPhenotype phenotype)
		{
		    this.phenotype = phenotype;
		}
		
		#endregion
		
		#region Properties
		
		public override Phenotype Pheno
		{
		    get { return phenotype; }
		}
		
		#endregion
		
		#region Methods
		
		public void Visit(LinearFaultGene lfg)
		{
		    Debug.Assert(phenotype != null);
		    for(int i = 0; i < phenotype.Grid.SizeI; ++i)
		    {
		        double xCosTheta = phenotype.Grid.NodeX(i) * Math.Cos(Angle.DegreesToRadians(lfg.Theta));
		        for(int j = 0; j < phenotype.Grid.SizeJ; ++j)
		        {
		            double ySinTheta = phenotype.Grid.NodeY(j) * Math.Sin(Angle.DegreesToRadians(lfg.Theta));
		            double distToCutoff = xCosTheta + ySinTheta - lfg.Rho;
		            if (distToCutoff >= 0.0)
		            {
		                // hangingwall
		                phenotype.Grid[i, j] -= lfg.Throw / 2.0;
		            }
		            else
		            {
		                // footwall
		                phenotype.Grid[i, j] += lfg.Throw / 2.0;
		            }
		        }
		    }
		}
		
		public void Visit(SinuousFaultGene sfg)
		{
		    Debug.Assert(phenotype != null);
		    Debug.Assert(sfg.DetachDepth > 0.0, "Depth to detachment must be positive");
		    Debug.Assert(sfg.Dip > 0.0 && sfg.Dip < 90.0, "Dip must be between 0° and 90°");
		    // Determine the four Bezier control points

        	// p1 and p4 are the fault tips
        	Point2D p1 = new Point2D(sfg.X0, sfg.Y0);
        	Point2D p4 = new Point2D(sfg.X1, sfg.Y1);
        
        	Vector2D axis = p4 - p1; // Vector from tip to tip
        	Vector2D halfAxis = axis / 2.0;
            Point2D  midAxis = p1 + halfAxis;
            Vector2D unitAxis = axis.Unit;
            Vector2D unitHeave = new Vector2D(unitAxis.DeltaY, -unitAxis.DeltaX); // Perpendicular
        
        	double offset2 = halfAxis.Magnitude * Math.Tan(Angle.DegreesToRadians(sfg.Phi0));
            Point2D p2 = midAxis - unitHeave * offset2;
            
        	double offset3 = halfAxis.Magnitude * Math.Tan(Angle.DegreesToRadians(sfg.Phi1));
        	Point2D p3 = midAxis + unitHeave * offset3;
        	
        	// Start with Bezier using Bernstein polynomials for weighting functions:
            //     (1-t^3)P1 + 3t(1-t)^2P2 + 3t^2(1-t)P3 + t^3P4
            //
            // Expand and collect terms to form linear combinations of original Bezier
            // controls.  This ends up with a vector cubic in t:
            //     (-P1+3P2-3P3+P4)t^3 + (3P1-6P2+3P3)t^2 + (-3P1+3P2)t + P1
            //             /\                  /\                /\       /\
            //             ||                  ||                ||       ||
            //             c3                  c2                c1       c0
        
            // Calculate the coefficients
        
            Vector2D vp1 = new Vector2D(p1);
            Vector2D vp2 = new Vector2D(p2);
            Vector2D vp3 = new Vector2D(p3);
            Vector2D vp4 = new Vector2D(p4);
            
        	Vector2D c3;
        	{
        	    Vector2D a = vp1 * -1.0;
        	    Vector2D b = vp2 *  3.0;
        	    Vector2D c = vp3 * -3.0;
        		Vector2D d = vp4 + a + b + c;
        		c3 = d;
        	}
        
        	Vector2D c2;
        	{
        	    Vector2D a = vp1 *  3.0;
        	    Vector2D b = vp2 * -6.0;
        	    Vector2D c = vp3 *  3.0;
        		Vector2D d = a + b + c;
        		c2 = d;
        	}
        
        	Vector2D c1;
        	{
        	    Vector2D a = vp1 * -3.0;
        	    Vector2D b = vp2 *  3.0;
        		Vector2D c = a + b;
        		c1 = c;
        	}
        
        	Vector2D c0 = vp1;
		
    		for(int i = 0; i < phenotype.Grid.SizeI; ++i)
        	{
        		double px = phenotype.Grid.NodeX(i);
        		for(int j = 0; j < phenotype.Grid.SizeJ; ++j)
        		{
        			double py = phenotype.Grid.NodeY(j);
        			
        			// Find normal to line: negative inverse of original line's slope
                    Vector2D n = unitAxis;
        
        			// Construct two points on the straight line - using the
        			// query point and the heave vector
                    Point2D a1 = new Point2D(px, py);
        			Point2D a2 = a1 + unitHeave;
        
        			// Determine new c coefficient for the line
                    double c_line = a1.X * a2.Y - a2.X * a1.Y;
        
                    // ?Rotate each cubic coefficient using line for new coordinate system?
        			// Find roots of rotated cubic
        			Polynomial poly = new Polynomial( n.Dot(c3), n.Dot(c2), n.Dot(c1), n.Dot(c0) + c_line);
        			List<double> roots = poly.Roots();
        			Debug.Assert(roots.Count <= 1);

        			foreach (double t in roots)
        			    {
        				if (0.0 <= t && t <= 1.0)
        				{
        					// We're within the Bezier curve
        					// Find point on Bezier
        					Point2D p5 = p1.Lerp(ref p2, t);
        					Point2D p6 = p2.Lerp(ref p3, t);
        					Point2D p7 = p3.Lerp(ref p4, t);
        
        					Point2D p8 = p5.Lerp(ref p6, t);
        					Point2D p9 = p6.Lerp(ref p7, t);
        
        					Point2D p10 = p8.Lerp(ref p9, t);
        
                            // p10 is the intersection between the heave-parallel line
        					// through the current query point, and the cubic bezier.
        
        					// The heave will be a sinusoidal function of t, with a
        					// heave of zero at the fault tips. So we map t over the inteval
        					// [0- > 1] to u over the interval [-PI/2 -> +3PI/4] which are
        					// the minimal of the sine curve
                            double u = (-Math.PI / 2.0) + (t * 2 * Math.PI);
        					double heave = (1.0 + Math.Sin(u)) * sfg.MaxHeave / 2.0;
                            Point2D fwCutoff = p10 - unitHeave * heave / 2.0;
        
        					// Determine whether we are in the hangingwall or the footwall
        					// By determining whether we are in the same direction as the
        					// unitHeave: Compute the dot product of fwCutoff->a1 and the
        					// unitHeave vector. If the result is +ve, we are in the HW
        					Vector2D transport = a1 - fwCutoff;
        					double side = transport.Dot(unitHeave);
        					if (side >= 0.0) {
        						// We are in the hangingwall	
        						double distToCutoff = transport.Magnitude;
        						const double datum = 0.0;
        						double k = Math.Tan(Angle.DegreesToRadians(sfg.Dip));
        						double faultDepth = sfg.DetachDepth * (1.0 - Math.Exp(-k * distToCutoff / sfg.DetachDepth));
        						double faultElevation = datum - faultDepth;
        						double offset0 = distToCutoff - heave;
        						double depth0  = sfg.DetachDepth * (1.0 - Math.Exp(-k * offset0 / sfg.DetachDepth));
        						double depth1  = faultDepth - depth0;
        						double elev1 = datum - depth1;
        						double delta = Math.Max(faultElevation, elev1);			
        						phenotype.Grid[i,j] += delta;
        					}
        				}
        			}
        		}
    		}
    	}
		
		#endregion
	}
}
