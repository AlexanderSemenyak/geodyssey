/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-05-01
 * Time: 14:38
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Wintellect.PowerCollections;
using System.Linq;

namespace Numeric
{
    /// <summary>
    /// Description of Polynomial.
    /// </summary>
    public class Polynomial
    {
        private const double tolerance = 1e-6;
        private readonly Deque<double> coefficients;
        private List<double> roots;
	    
        #region Construction

        public Polynomial(double a, double b)
        {
            coefficients = new Deque<double> { b, a };
        }

        public Polynomial(double a, double b, double c)
        {
            coefficients = new Deque<double> {c, b, a};
        }

        public Polynomial(double a, double b, double c, double d)
        {
            coefficients = new Deque<double> {d, c, b, a};
        }
		
        #endregion
		
        #region Properties
		
        public int Degree
        {
            get { return coefficients.Count - 1; }
        }

        // TODO: This cacheing scheme isn't very compatible with these accessors


        public List<double> Roots()
        {
            return Roots(false);
        }

        public List<double> Roots(bool polished)
        {
            if (roots == null)
            {
                ComputeRoots(polished);
            }
            return roots;
        }

        #endregion
		
        #region Methods
		
        private void Simplify()
        {
            for ( int i = Degree; i >= 0; i-- )
            {
                if ( Math.Abs( coefficients[i] ) <= tolerance )
                {
                    coefficients.RemoveFromBack();
                }
                else
                {
                    break;
                }
            }    
        }
		
        private void ComputeRoots(bool polished)
        {
            Simplify();
            Debug.Assert( Degree <= 3);
            switch( Degree )
            {
                case 0:
                    roots = new List<Double>();
                    break;
                case 1:
                    LinearRoot();
                    break;
                case 2:
                    QuadraticRoots(polished);
                    break;
                case 3:
                    CubicRoots(polished);
                    break;
                default:
                    roots = null;
                    break;
            }
        }
		
        private void LinearRoot()
        {
            Debug.Assert(Degree == 1);
            roots = new List<Double>(1);
            double a = coefficients[1];
            if (a != 0.0)
            {
                roots.Add(-coefficients[0] / a);
            }
        }
		
        private void QuadraticRoots(bool polished)
        {
            Debug.Assert( Degree == 2 );
            double a = coefficients[2];
            double b = coefficients[1] / a;
            double c = coefficients[0] / a;
            double d = b * b - 4.0 * c;
        
            Debug.Assert(d >= 0.0);
        
            roots = new List<Double>(2);
        
            if ( d > 0.0 )
            {
                double e = Math.Sqrt(d);
                roots.Add( 0.5 * (-b + e) );
                roots.Add( 0.5 * (-b - e) );
            }
            else if ( d == 0.0 )
            {
                // really two roots with same value, but we only return one
                roots.Add( 0.5 * -b );
            }
   
            if (polished)
            {
                Func<double, double> f  = x => coefficients[2] * x * x + coefficients[1] * x + coefficients[0];
                Func<double, double> fd = x => 2 * coefficients[2] * x + coefficients[1];
                PolishRoots(f, fd);
            }
        }

        private void CubicRoots(bool polished)
        {
            Debug.Assert( Degree == 3 );
            double c3 = coefficients[3];
            double c2 = coefficients[2] / c3;
            double c1 = coefficients[1] / c3;
            double c0 = coefficients[0] / c3;
        
            roots = new List<Double>(3);
        
            double a       = (3.0 * c1 - c2 * c2) / 3.0;
            double b       = (2.0 * c2 * c2 * c2 - 9.0 * c1 * c2 + 27.0 * c0) / 27.0;
            double offset  = c2 / 3.0;
            double discrim = b * b / 4.0 + a * a * a / 27.0;
            double halfB   = b / 2.0;
        
            if ( Math.Abs(discrim) <= tolerance )
            {
                discrim = 0.0;
            }
            
            if ( discrim > 0.0 )
            {
                double e = Math.Sqrt(discrim);

                double tmp = -halfB + e;
                double root = tmp >= 0.0 ? Math.Pow(tmp, 1.0/3.0) : -Math.Pow(-tmp, 1.0/3.0);

                tmp = -halfB - e;
                if ( tmp >= 0.0 )
                {
                    root += Math.Pow(tmp, 1.0 / 3.0);
                }
                else
                {
                    root -= Math.Pow(-tmp, 1.0 / 3.0);
                }
                roots.Add( root - offset );
            }
            else if ( discrim < 0.0 )
            {
                double distance = Math.Sqrt(-a / 3.0);
                double angle    = Math.Atan2( Math.Sqrt(-discrim), -halfB) / 3.0;
                double cos      = Math.Cos(angle);
                double sin      = Math.Sin(angle);
                double sqrt3    = Math.Sqrt(3.0);
        
                roots.Add( 2.0 * distance * cos - offset );
                roots.Add( -distance * (cos + sqrt3 * sin) - offset);
                roots.Add( -distance * (cos - sqrt3 * sin) - offset);
            }
            else
            {
                double tmp;
                if ( halfB >= 0.0 )
                {
                    tmp = -Math.Pow(halfB, 1.0 / 3.0);
                }
                else
                {
                    tmp = Math.Pow(-halfB, 1.0 / 3.0);
                }
                roots.Add( 2.0 * tmp - offset );
                // really should return next root twice, but we return only one
                roots.Add( -tmp - offset );
            }

            if (polished)
            {
                Func<double, double> f  = x => coefficients[3] * x * x * x + coefficients[2] * x * x + coefficients[1] * x + coefficients[0];
                Func<double, double> fd = x => 3 * coefficients[3] * x * x + 2 * coefficients[2] * x + coefficients[1];
                PolishRoots(f, fd);
            }
        }

        private void PolishRoots(Func<double, double> f, Func<double, double> fd)
        {
            roots = new List<double>(roots.Select(r => NewtonRaphson.FindRoot(r, f, fd, tolerance, 100)));
        }

        #endregion
    }
}