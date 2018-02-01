using System;
using System.Diagnostics;
using System.Collections.Generic;
using Wintellect.PowerCollections;

using Numeric;

namespace Geodyssey
{
	/// <summary>
	/// Description of ExpressionVisitor.
	/// </summary>
	public class OutlineMapExpressionVisitor : ExpressionVisitor,
	                                       ILinearFaultGeneVisitor,
	                                       ISinuousFaultGeneVisitor
	{
	    private OutlineMapPhenotype phenotype;
	    
	    # region Construction
	    
		public OutlineMapExpressionVisitor(OutlineMapPhenotype phenotype)
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
            // Draw a straight line
            //for(int i = 0; i < phenotype.Grid.SizeI; ++i)
            //{
            //    double xCosTheta = phenotype.Grid.NodeX(i) * Math.Cos(Angle.DegreesToRadians(lfg.Theta));
            //    for(int j = 0; j < phenotype.Grid.SizeJ; ++j)
            //    {
            //        double ySinTheta = phenotype.Grid.NodeY(j) * Math.Sin(Angle.DegreesToRadians(lfg.Theta));
            //        double distToCutoff = xCosTheta + ySinTheta - lfg.Rho;
            //        if (distToCutoff >= 0.0)
            //        {
            //            // hangingwall
            //            phenotype.Grid[i, j] -= lfg.Throw / 2.0;
            //        }
            //        else
            //        {
            //            // footwall
            //            phenotype.Grid[i, j] += lfg.Throw / 2.0;
            //        }
            //    }
            //}
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

            const int numSteps = 25;
            Deque<Point2D> heavePolygon = new Deque<Point2D>();
            heavePolygon.Capacity = numSteps * 2;
            for (int i = 0; i < numSteps; ++i)
            {
                double t = (double) i / (double) numSteps;
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
                // the minima of the sine curve
                double u = (-Math.PI / 2.0) + (t * 2.0 * Math.PI);
                double heave = (1.0 + Math.Sin(u)) * sfg.MaxHeave / 2.0;
                Point2D fwCutoff = p10 - unitHeave * heave / 2.0;
                Point2D hwCutoff = p10 + unitHeave * heave / 2.0;

                heavePolygon.AddToFront(fwCutoff);
                heavePolygon.AddToBack(hwCutoff);
            }
            phenotype.Map.AddPolygon(heavePolygon);
    	}
		#endregion
	}
}
