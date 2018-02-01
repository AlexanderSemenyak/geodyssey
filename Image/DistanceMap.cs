using System;
using System.Collections.Generic;
using System.Text;

namespace Image
{
    public static class DistanceMap
    {
        private const double infinity = 1e20;

        public static IImage<double> EuclideanTransform(IImage<bool> bitmap)
        {
            FastImage<double> f = new FastImage<double>(bitmap.Width, bitmap.Height);
            for (int i = 0; i < bitmap.Width; ++i)
            {
                for (int j = 0; j < bitmap.Height; ++j)
                {
                    f[i, j] = bitmap[i, j] ? 0.0 : infinity;
                }
            }
            return EuclideanTransform(f);
        }

        public static IImage<double> EuclideanTransform(IImage<double> image)
        {
            IImage<double> partialResult = image.CloneSize();

            IImage<double> result = SerialTransform(image, partialResult);

            // Square-root the result
            for (int i = 0; i < result.Width; ++i)
            {
                for (int j = 0; j < result.Height; ++j)
                {
                    result[i, j] = Math.Sqrt(result[i, j]);
                }
            }
            return result;
        }

        private static IImage<double> SerialTransform(IImage<double> image, IImage<double> partialResult)
        {
            // Transform along rows
            for (int j = 0; j < image.Height; j++)
            {
                TransformSlice(image.Row(j), partialResult.Row(j));
            }

            IImage<double> result = image.CloneSize();

            // Transform along columns
            for (int i = 0; i < image.Width; ++i)
            {
                TransformSlice(partialResult.Column(i), result.Column(i));
            }
            return result;
        }

        /// <summary>
        /// Distance transform of 1D function
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private static void TransformSlice(IImageSlice<double> source, IImageSlice<double> target)
        {
            int n = source.Count;
            int[] v = new int[n];
            double[] z = new double[n + 1];

            int k = 0;
            v[0] = 0;
            // TODO: Is this correct or should we just use a large number?
            z[0] = -infinity;
            z[1] = infinity;
            for (int q = 1; q < n; ++q)
            {
                double s = S(source, q, v[k]);
                while (s <= z[k])
                {
                    --k;
                    s = S(source, q, v[k]);
                }
                ++k;
                v[k] = q;
                z[k] = s;
                z[k + 1] = infinity;
            }

            k = 0;
            for (int q = 0; q < n; ++q)
            {
                while (z[k + 1] < q)
                {
                    ++k;
                }
                target[q] = Square(q - v[k]) + source[v[k]];
            }
        }

        private static double S(IImageSlice<double> slice, int q, int vk)
        {
            return ((slice[q] + Square(q)) - (slice[vk] + Square(vk))) / (2 * q - 2 * vk);
        }

        // TODO: Move to Numeric library
        private static double Square(int n)
        {
            return n * n;
        }
    }
}
