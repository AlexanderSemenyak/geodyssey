using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Image
{
    public enum BorderTreatmentMode
    {
        BorderTreatmentClip,
        BorderTreatmentRepeat,
    };

    public class Kernel1D
    {
        #region Fields
        private readonly List<double> kernel = new List<double>(1);
        private int left = 0;
        private int right = 0;
        private double norm;
        private BorderTreatmentMode borderTreatment = BorderTreatmentMode.BorderTreatmentClip;
        #endregion

        #region Construction
        public static Kernel1D CreateGaussian(double standardDeviation)
        {
            Kernel1D k = new Kernel1D();
            k.InitGaussian(standardDeviation);
            return k;
        }

        public static Kernel1D CreateGaussianDerivative(double standardDeviation, int order)
        {
            return CreateGaussianDerivative(standardDeviation, order, 1.0);
        }

        public static Kernel1D CreateGaussianDerivative(double standardDeviation, int order, double norm)
        {
            Kernel1D k = new Kernel1D();
            k.InitGaussianDerivative(standardDeviation, order, norm);
            return k;
        }

        public Kernel1D()
        {
            this.norm = 1;
            this.kernel.Add(this.norm);
        }
        #endregion

        #region Properties
        public int Left
        {
            get { return left; }
        }

        public int Right
        {
            get { return right; }
        }

        public BorderTreatmentMode BorderTreatment
        {
            get { return borderTreatment; }
        }

        public double this[int index]
        {
            get
            {
                Debug.Assert(index >= Left);
                Debug.Assert(index <= Right);
                // TODO: Verify this!
                return kernel[index - Left];
            }
        }
        #endregion

        #region Methods
        public void InitGaussian(double standardDeviation, double norm)
        {
            Debug.Assert(standardDeviation >= 0.0);
            Gaussian gauss = new Gaussian(standardDeviation);
            if (standardDeviation > 0.0)
            {
                int radius = (int) (3 * standardDeviation + 0.5);
                if (radius == 0)
                {
                    radius = 1;
                }
                kernel.Clear();
                kernel.Capacity = radius * 2 + 1;
                for (int x = -radius; x <= radius; ++x)
                {
                    kernel.Add(gauss.Value(x));
                }
                left = -radius;
                right = radius;
            }
            else
            {
                kernel.Clear();
                kernel.Add(1.0);
                left = 0;
                right = 0;
            }

            if (norm != 0.0)
            {
                Normalize(norm);
            }
            else
            {
                this.norm = 1.0;
            }

            // Best border treatment for Gaussians is clip
            borderTreatment = BorderTreatmentMode.BorderTreatmentClip;
        }

        private void InitGaussianDerivative(double standardDeviation, int order, double norm)
        {
            Debug.Assert(order >= 0);
            if (order == 0)
            {
                InitGaussian(standardDeviation, norm);
                return;
            }

            Debug.Assert(standardDeviation > 0.0);
            Gaussian gauss = new Gaussian(standardDeviation, order);

            // First calculate the required kernel size
            int radius = (int) (3.0 * standardDeviation + 0.5 * order + 0.5);
            if (radius == 0)
            {
                radius = 1;
            }
            kernel.Clear();
            kernel.Capacity = radius * 2 + 1;

            // Fill the kernel and calculate the DC component
            // introduced by truncation of the Gaussian
            double dc = 0.0;
            for (int x = -radius; x <= radius; ++x)
            {
                kernel.Add(gauss.Value(x));
                dc += kernel[kernel.Count - 1];
            }
            dc /= 2.0 * radius + 1.0;

            // Remove the DC component, but only if kernel correction
            // is permitted by a non-zero value for the norm
            if (norm != 0.0)
            {
                for (int i = 0; i < kernel.Count; ++i)
                {
                    kernel[i] -= dc;
                }
            }

            left = -radius;
            right = radius;

            if (norm != 0.0)
            {
                Normalize(norm, order);
            }
            else
            {
                this.norm = 1.0;
            }

            // The best border treatment for Gaussian derivatives in repeat
            borderTreatment = BorderTreatmentMode.BorderTreatmentRepeat;
        }

        private void Normalize()
        {
            Normalize(1.0);
        }

        private void Normalize(double normal)
        {
            Normalize(normal, 0, 0.0);
        }

        private void Normalize(double normal, int derivativeOrder)
        {
            Normalize(normal, derivativeOrder, 0.0);
        }

        private void Normalize(double normal, int derivativeOrder, double offset)
        {
            Debug.Assert(derivativeOrder >= 0);
            double sum = 0.0;
            if (derivativeOrder == 0)
            {
                foreach (double k in kernel)
                {
                    sum += k;
                }
            }
            else
            {
                int faculty = 1;
                for (int i = 2; i <= derivativeOrder; ++i)
                {
                    faculty *= i;
                }
                double x = left + offset;
                foreach (double k in kernel)
                {
                    sum += k * Math.Pow(-x, derivativeOrder) / faculty;
                    x += 1.0;
                }
            }
            Debug.Assert(sum != 0.0);
            sum = normal / sum;
            for (int i = 0; i < kernel.Count; ++i)
            {
                kernel[i] *= sum;
            }
            norm = normal;
        }

        public void InitGaussian(double standardDeviation)
        {
            InitGaussian(standardDeviation, 1);
        }
        #endregion
    }
}
