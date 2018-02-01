using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Image
{
    /// <summary>
    /// Image storing pixel data in a single dimensional array, allowing fast
    /// 1D access through the Buffer property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class FastImage<T> : ImageBase<T>
    {
        readonly int width;
        readonly int height;
        private readonly T[] pixels;
 
        public FastImage(int width, int height)
        {
            this.width  = width;
            this.height = height;
            pixels = new T[width * height];
        }

        public FastImage(FastImage<T> other)
        {
            this.width = other.width;
            this.height = other.height;
            this.pixels = (T[]) other.pixels.Clone();
        }

        public FastImage(int width, int height, Func<int, int, T> function) :
            this(width, height)
        {
            ForEachPixel(function);
        }

        public FastImage(int width, int height, IEnumerable<T> seq)
        {
            pixels = new T[width * height];
            this.width = width;
            this.height = height;
            IEnumerator<T> e = seq.GetEnumerator();
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    if (e.MoveNext())
                    {
                        pixels[Index(i, j)] = e.Current;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Constructs an image using the provided array.  The array is copied
        /// </summary>
        /// <param name="multiarray">The array representing the image</param>
        public FastImage(T[,] multiarray)
        {
            width = multiarray.GetLength(0);
            height = multiarray.GetLength(1);
            pixels = new T[width * height];
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    pixels[Index(i, j)] = multiarray[i, j];
                }
            }
        }

        private int Index(int i, int j)
        {
            return i * height + j;
        }

        public override int Width
        {
            get { return width; }
        }

        public override int Height
        {
            get { return height; }
        }

        public override T this[int i, int j]
        {
            get
            {
                return pixels[Index(i, j)];
            }
            set
            {
                pixels[Index(i, j)] = value;
            }
        }

        public T[] Buffer
        {
            get { return pixels; }
        }

        public override void Clear()
        {
            for(int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = default(T);
            }
        }

        public override object Clone()
        {
            return new FastImage<T>(this);
        }

        public override IImage<T> CloneSize()
        {
            return new FastImage<T>(Width, Height);
        }

        public override IImage<T> CloneSize(int newWidth, int newHeight)
        {
            return new FastImage<T>(newWidth, newHeight);
        }

        /// <summary>
        /// Create a new IImage by transforming the current image with the supplied function
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public override IImage<T> CloneTransform(Func<int, int, T> function)
        {
            return new FastImage<T>(Width, Height, function);
        }

        // TODO: Provide a faster Equals here.

        public override IEnumerator<T> GetEnumerator()
        {
            foreach (T pixel in pixels)
            {
                yield return pixel;
            }
        }
    }
}
