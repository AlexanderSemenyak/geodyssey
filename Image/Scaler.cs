using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Image
{
    public static class Scaler
    {
        /// <summary>
        /// Pixel resizing of an image. Each pixel in the source image will
        /// be replaced with factor*factor pixels in the result image
        /// </summary>
        /// <typeparam name="T">The value-type of the image</typeparam>
        /// <param name="source">The source image</param>
        /// <param name="factor">The scaling factor</param>
        /// <returns></returns>
        public static IImage<T> Downscale<T>(IImage<T> source, int factor)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (factor < 1)
            {
                throw new ArgumentException("factor must be a positive integer", "factor");
            }
            IImage<T> result = source.CloneSize(source.Width * factor, source.Height * factor);
            for (int i = 0; i < source.Width; ++i)
            {
                for (int j = 0; j < source.Height; ++j)
                {
                    T currentSource = source[i, j];
                    int pStart = i * factor;
                    int pEnd = pStart + factor;
                    for (int p = pStart; p < pEnd; ++p)
                    {
                        int qStart = j * factor;
                        int qEnd = qStart + factor;
                        for (int q = qStart; q < qEnd; ++q)
                        {
                            result[p, q] = currentSource;
                        }
                    }
                }
            }
            return result;
        }
    }
}
