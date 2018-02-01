using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Image
{
    // Implementation of basic services for an Image; this class allocates
    // no image storage. Use as a base class for concete image implementation. 
    public abstract class ImageBase<T> :IImage<T>, IEquatable<ImageBase<T>>
    {
        public abstract IImage<T> CloneSize();
        public abstract IImage<T> CloneSize(int newWidth, int newHeight);

        public virtual IImage<T> CloneTransform(Func<int, int, T> function)
        {
            IImage<T> result = CloneSize();
            result.ForEachPixel(function);
            return result;
        }

        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract T this[int column, int row] { get; set; }

        public abstract void Clear();

        /// <summary>
        /// Determine whether the supplied (i,j) coordinate pair
        /// lies within the image
        /// </summary>
        /// <param name="i">The column coordinate</param>
        /// <param name="j">The row coordinate</param>
        /// <returns>True is the coordinate is within the image</returns>
        public virtual bool IsInRange(int i, int j)
        {
            return (i >= 0) && (i < Width) && (j >= 0) && (j < Height);
        }

        public virtual ImageRowIterator<T> GetRowIterator(int row)
        {
            return new ImageRowIterator<T>(this, row);
        }

        public virtual ImageRowIterator<T> GetRowIterator(int column, int row)
        {
            return new ImageRowIterator<T>(this, column, row);
        }

        public virtual ImageColumnIterator<T> GetColumnIterator(int column)
        {
            return new ImageColumnIterator<T>(this, column);
        }

        public virtual ImageColumnIterator<T> GetColumnIterator(int column, int row)
        {
            return new ImageColumnIterator<T>(this, column, row);
        }

        public virtual IImageSlice<T> Row(int row)
        {
            return new ImageRow<T>(this, row);
        }

        public virtual IImageSlice<T> Column(int column)
        {
            return new ImageColumn<T>(this, column);
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Width; ++i)
            {
                for (int j = 0; j < Height; ++j)
                {
                    yield return this[i, j];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            ImageBase<T> rhs = obj as ImageBase<T>;
            return rhs != null && Equals(rhs);
        }

        public bool Equals(ImageBase<T> rhs)
        {
            return Equals((IImage<T>) rhs) ;
        }

        public override int GetHashCode()
        {
            int hashcode = Width.GetHashCode() ^ Height.GetHashCode();
            foreach (T pixel in this)
            {
                hashcode ^= pixel.GetHashCode();
            }
            return hashcode;
        }

        public abstract object Clone();


        public bool Equals(IImage<T> rhs)
        {
            if (rhs == null)
            {
                return false;
            }

            if (Width != rhs.Width)
            {
                return false;
            }

            if (Height != rhs.Height)
            {
                return false;
            }

            for (int i = 0; i < Width; ++i)
            {
                for (int j = 0; j < Height; ++j)
                {
                    if (!this[i, j].Equals(rhs[i, j]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override string ToString()
        {
            // TODO: This could be specialized for bool images
            StringBuilder sb = new StringBuilder();
            for (int j = Height - 1; j >= 0; --j)
            {
                for (int i = 0; i < Width; ++i)
                {
                    sb.Append(this[i, j]);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Applies the given function to each (i, j) coordinate and assign the
        /// result of the function to that pixel
        /// </summary>
        /// <param name="function">A delgate accepting i and j</param>
        public void ForEachPixel(Func<int, int, T> function)
        {
            for (int i = 0; i < Width; ++i)
            {
                for (int j = 0 ; j < Height ; ++j)
                {
                    this[i, j] = function(i, j);
                }
            }
        }
    }
}
