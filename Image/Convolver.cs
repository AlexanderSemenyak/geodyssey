using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Image
{
    public static class Convolver
    {
        public static void GaussianGradient(IImage<double> source, IImage<double> targetX, IImage<double> targetY, double scale)
        {
            Kernel1D smooth = Kernel1D.CreateGaussian(scale);
            Kernel1D grad = Kernel1D.CreateGaussianDerivative(scale, 1);

            IImage<double> tempor = source.CloneSize();

            SeparableConvolveX(source, tempor, grad);
            SeparableConvolveY(tempor, targetX, smooth);

            SeparableConvolveX(source, tempor, smooth);
            SeparableConvolveY(tempor, targetY, grad);
        }

        public static void HessianMatrixOfGaussian(IImage<double> source, IImage<double> targetXX, IImage<double> targetYY, IImage<double> targetXY, double scale)
        {
            Kernel1D smooth = Kernel1D.CreateGaussian(scale);
            Kernel1D deriv1 = Kernel1D.CreateGaussianDerivative(scale, 1);
            Kernel1D deriv2 = Kernel1D.CreateGaussianDerivative(scale, 2);

            IImage<double> tempor = source.CloneSize();

            SeparableConvolveX(source, tempor, deriv2);
            SeparableConvolveY(tempor, targetXX, smooth);

            SeparableConvolveX(source, tempor, smooth);
            SeparableConvolveY(tempor, targetYY, deriv2);

            SeparableConvolveX(source, tempor, deriv1);
            SeparableConvolveY(tempor, targetXY, deriv1);
        }

        public static void GaussianSmooth(IImage<double> source, IImage<double> target, double scale)
        {
            Kernel1D smooth = Kernel1D.CreateGaussian(scale);
            IImage<double> tempor = source.CloneSize();
            SeparableConvolveX(source, tempor, smooth);
            SeparableConvolveY(tempor, target, smooth);
        }

        /// <summary>
        /// Performs a one dimensional convolution in the x direction
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="kernel"></param>
        private static void SeparableConvolveX(IImage<double> source, IImage<double> target, Kernel1D kernel)
        {
            Debug.Assert(kernel.Left <= 0);
            Debug.Assert(kernel.Right >= 0);
            int w = source.Width;
            int h = source.Height;
            Debug.Assert(w >= kernel.Right - kernel.Left + 1, "Kernel cannot be longer than line");
            for (int y = 0; y < h; ++y)
            {
                ImageRowIterator<double> rs = source.GetRowIterator(y);
                ImageRowIterator<double> rsEnd = source.GetRowIterator(source.Width, y);
                ImageRowIterator<double> rt = target.GetRowIterator(y);
                ConvolveLine(rs, rsEnd, rt, kernel);
            }
        }

        /// <summary>
        /// Performs a one dimensional convolution in the y direction
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="kernel"></param>
        private static void SeparableConvolveY(IImage<double> source, IImage<double> target, Kernel1D kernel)
        {
            Debug.Assert(kernel.Left <= 0);
            Debug.Assert(kernel.Right >= 0);
            int w = source.Width;
            int h = source.Height;
            Debug.Assert(h >= kernel.Right - kernel.Left + 1, "Kernel cannot be longer than line");

            for (int x = 0; x < w; ++x)
            {
                ImageColumnIterator<double> cs = source.GetColumnIterator(x, 0);
                ImageColumnIterator<double> csEnd = source.GetColumnIterator(x, source.Height);
                ImageColumnIterator<double> ct = target.GetColumnIterator(x);
                ConvolveLine(cs, csEnd, ct, kernel);
            }
        }

        private static void ConvolveLine<Iterator>(Iterator iSource, Iterator iSourceEnd, Iterator iTarget, Kernel1D kernel)
            where Iterator :IImageIterator<double>
        {
            Debug.Assert(kernel.Left <= 0);
            Debug.Assert(kernel.Right >= 0);
            int w = iSourceEnd.Difference(iSource);
            Debug.Assert(w >= kernel.Right - kernel.Left + 1);
            switch (kernel.BorderTreatment)
            {
                case BorderTreatmentMode.BorderTreatmentRepeat:
                    ConvolveLineRepeat(iSource, iSourceEnd, iTarget, kernel);
                    break;
                case BorderTreatmentMode.BorderTreatmentClip:
                    double norm = 0.0;
                    for (int i = kernel.Left; i <= kernel.Right; ++i)
                    {
                        norm += kernel[i];
                    }
                    ConvolveLineClip(iSource, iSourceEnd, iTarget, kernel, norm);
                    break;
            }
        }

        private static void ConvolveLineClip<Iterator>(Iterator iSource, Iterator iSourceEnd, Iterator iTarget, Kernel1D kernel, double norm)
            where Iterator :IImageIterator<double>
        {
            // TODO: Our iterators should have the similar semantics to IEnumerator. i.e. They should
            //       start _before_ the collection to be iterated.
            int w = iSourceEnd.Difference(iSource);
            Iterator iBegin = iSource;
            for (int x = 0; x < w; ++x)
            {
                // Convolve each pixel
                int kernelIndex = kernel.Right;
                double sum = 0.0;
                if (x < kernel.Right)
                {
                    int x0 = x - kernel.Right;
                    double clipped = 0.0;
                    while (x0 != 0)
                    {
                        clipped += kernel[kernelIndex];
                        --kernelIndex;
                        ++x0;
                    }

                    Iterator iSampleSource = iBegin;
                    Iterator iSampleSourceEnd = (Iterator) iSource.Offset(1 - kernel.Left);
                    while (!iSampleSource.Equals(iSampleSourceEnd))
                    {
                        sum += kernel[kernelIndex] * iSampleSource.Current;
                        --kernelIndex;
                        iSampleSource.MoveNext();
                    }

                    sum = norm / (norm - clipped) * sum;
                }
                else if (w - x <= -kernel.Left)
                {
                    Iterator iSampleSource = (Iterator) iSource.Offset(-kernel.Right);
                    Iterator iSampleSourceEnd = iSourceEnd;
                    while (!iSampleSource.Equals(iSampleSourceEnd))
                    {
                        sum += kernel[kernelIndex] * iSampleSource.Current;
                        --kernelIndex;
                        iSampleSource.MoveNext();
                    }

                    double clipped = 0.0;
                    int x0 = -kernel.Left - w + x + 1;
                    while (x0 != 0)
                    {
                        clipped += kernel[kernelIndex];
                        --kernelIndex;
                        --x0;
                    }

                    sum = norm / (norm - clipped) * sum;
                }
                else
                {
                    sum += ConvolvePixel(iSource, kernel, kernelIndex);
                }

                iTarget.Current = sum;
                iSource.MoveNext();
                iTarget.MoveNext();
            }
        }

        private static void ConvolveLineRepeat<Iterator>(Iterator iSource, Iterator iSourceEnd, Iterator iTarget, Kernel1D kernel)
            where Iterator :IImageIterator<double>
        {
            int w = iSourceEnd.Difference(iSource);
            Iterator iBegin = iSource;
            for (int x = 0; x < w; ++x)
            {
                // Convolve each pixel
                int kernelIndex = kernel.Right;
                double sum = 0.0;
                if (x < kernel.Right)
                {
                    int x0 = x - kernel.Right;
                    Iterator iSampleSource = iBegin;
                    while (x0 != 0)
                    {
                        sum += kernel[kernelIndex] * iSampleSource.Current;
                        --kernelIndex;
                        ++x0;
                    }

                    Iterator iSampleSourceEnd = (Iterator) iSource.Offset(1 - kernel.Left);
                    while (!iSampleSource.Equals(iSampleSourceEnd))
                    {
                        sum += kernel[kernelIndex] * iSampleSource.Current;
                        --kernelIndex;
                        iSampleSource.MoveNext();
                    }
                }
                else if (w - x <= -kernel.Left)
                {
                    Iterator iSampleSource = (Iterator) iSource.Offset(-kernel.Right);
                    Iterator iSampleSourceEnd = iSourceEnd;
                    while (!iSampleSource.Equals(iSampleSourceEnd))
                    {
                        sum += kernel[kernelIndex] * iSampleSource.Current;
                        --kernelIndex;
                        iSampleSource.MoveNext();
                    }

                    int x0 = -kernel.Left - w + x + 1;
                    iSampleSource = (Iterator) iSourceEnd.Offset(-1);
                    while (x0 != 0)
                    {
                        sum += kernel[kernelIndex] * iSampleSource.Current;
                        --kernelIndex;
                        --x0;
                    }
                }
                else
                {
                    sum += ConvolvePixel(iSource, kernel, kernelIndex);
                }
                iTarget.Current = sum;
                iSource.MoveNext();
                iTarget.MoveNext();
            }
        }

        private static double ConvolvePixel<Iterator>(Iterator iSource, Kernel1D kernel, int kernelIndex)
            where Iterator :IImageIterator<double>
        {
            double sum = 0.0;
            Iterator iSampleSource = (Iterator) iSource.Offset(-kernel.Right);
            Iterator iSampleSourceEnd = (Iterator) iSource.Offset(1 - kernel.Left);
            while (!iSampleSource.Equals(iSampleSourceEnd))
            {
                sum += kernel[kernelIndex] * iSampleSource.Current;
                --kernelIndex;
                iSampleSource.MoveNext();
            }
            return sum;
        }
    }
}
