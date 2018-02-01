using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Numeric
{
    public static class NewtonRaphson
    {
        /// <summary>
        /// Newton-Raphson root finding
        /// </summary>
        /// <param name="x">The initial guess for the root</param>
        /// <param name="f">The function for which roots are to be found</param>
        /// <param name="fd">The first derivative of f with respect to x</param>
        /// <param name="precision">The required precision of the result</param>
        /// <param name="maxIterations">The maximum number of iterations</param>
        /// <returns>The root</returns>
        public static double FindRoot(double x, Func<double, double> f, Func<double, double> fd, double precision, int maxIterations)
        {
            double term;
            do
            {
                term = f(x) / fd(x);
                x -= term;
                --maxIterations;
            }
            while (Math.Abs(term / x) > precision && (maxIterations > 0));
            return x;
        }
    }
}
