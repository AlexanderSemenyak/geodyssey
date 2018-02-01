using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Image
{
    public class Gaussian
    {
        #region Fields
        private readonly double sigma;
        private readonly double sigma2;
        private readonly double norm;
        private readonly int order;
        private readonly double[] hermitePolynomial;
        #endregion

        #region Construction

        public Gaussian()
            :
            this(1.0, 0)
        {
        }

        public Gaussian(double sigma)
            :
            this(sigma, 0)
        {
        }

        public Gaussian(double sigma, int derivativeOrder)
        {
            Debug.Assert(sigma > 0.0);
            Debug.Assert(order >= 0);
            this.sigma = sigma;
            this.sigma2 = -0.5 / sigma / sigma;
            this.norm = 0.0;
            this.order = derivativeOrder;
            this.hermitePolynomial = new double[order / 2 + 1];
            switch (order)
            {
                case 1:
                case 2:
                    norm = -1.0 / (Math.Sqrt(2.0 * Math.PI) * sigma * sigma * sigma);
                    break;
                case 3:
                    norm = 1.0 / (Math.Sqrt(2.0 * Math.PI) * sigma * sigma * sigma * sigma * sigma);
                    break;
                default:
                    norm = 1.0 / (Math.Sqrt(2.0 * Math.PI) / sigma);
                    break;
            }
            CalculateHermitePolynomial();
        }
        #endregion

        #region Methods
        public double Value(double x)
        {
            double x2 = x * x;
            double g = norm * Math.Exp(x2 * sigma2);
            switch (order)
            {
                case 0:
                    return g;
                case 1:
                    return x * g;
                case 2:
                    return (1.0 - Sq(x / sigma)) * g;
                case 3:
                    return (3.0 - Sq(x / sigma)) * x * g;
                default:
                    // TODO: Is this needed?
                    return order % 2 == 0 ? g * Horner(x2) : x * g * Horner(x2);
            }
        }

        /// <summary>
        /// Horner's Scheme for efficiently approximately
        /// evaluating polynomials.
        /// </summary>
        /// <param name="x">The value at which the Hermite Polynomial is to be evaluated</param>
        /// <returns>The value of the Hermite Polynomial at x</returns>
        private double Horner(double x)
        {
            int i = order / 2;
            double res = hermitePolynomial[i];
            for (--i; i >= 0; --i)
            {
                res = x * res + hermitePolynomial[i];
            }
            return res;
        }

        private void CalculateHermitePolynomial()
        {
            switch (order)
            {
                case 0:
                    hermitePolynomial[0] = 1.0;
                    break;
                case 1:
                    hermitePolynomial[0] = -1.0 / sigma / sigma;
                    break;
                default:
                    {
                        // calculate Hermite polynomial for requested derivative
                        // recursively according to
                        //     (0)
                        //    h   (x) = 1
                        //
                        //     (1)
                        //    h   (x) = -x / s^2
                        //
                        //     (n+1)                        (n)           (n-1)
                        //    h     (x) = -1 / s^2 * [ x * h   (x) + n * h     (x) ]
                        //
                        double s2 = -1.0 / sigma / sigma;
                        int size = 3 * order + 3;
                        double[] hn = new double[size];
                        int hn0 = 0;
                        int hn1 = hn0 + order + 1;
                        int hn2 = hn1 + order + 1;
                        hn[hn2 + 0] = 1.0;
                        hn[hn1 + 1] = s2;
                        for (uint i = 2; i <= order; ++i)
                        {
                            hn[hn0] = s2 * (i - 1) * hn[hn2];
                            for (int j = 1; j <= i; ++j)
                            {
                                hn[hn0 + j] = s2 * (hn[hn1 + j - 1] + (i - 1) * hn[hn2 + j]);
                            }
                            int ht = hn2;
                            hn2 = hn1;
                            hn1 = hn0;
                            hn0 = ht;
                        }
                        // keep only non-zero coefficients of the polynomial
                        for (int i = 0; i < hermitePolynomial.Length; ++i)
                        {
                            double coefficient = order % 2 == 0 ? hn[hn1 + 2 * i] : hn[hn1 + 2 * i + 1];
                            hermitePolynomial[i] = coefficient;
                            Debug.Assert(hermitePolynomial[i] == coefficient);
                        }
                    }
                    break;
            }
        }

        // TODO: Move to Numeric library
        private static double Sq(double x)
        {
            return x * x;
        }
        #endregion
    }
}
