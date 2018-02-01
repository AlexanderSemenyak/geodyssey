using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Image
{
    public interface IImage<T> : ICloneable, IEnumerable<T>, IEquatable<IImage<T>>
    {

        /// <summary>
        /// Create a new image instance of the same type, with default pixel
        /// content, and the same size as this IImage.
        /// </summary>
        /// <returns></returns>
        IImage<T> CloneSize();

        /// <summary>
        /// Create a new image instance of the same type, but with the specified
        /// size, and default pixel content.
        /// </summary>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <returns></returns>
        IImage<T> CloneSize(int newWidth, int newHeight);

        /// <summary>
        /// Create a new IImage by transforming the current image with the supplied function
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        IImage<T> CloneTransform(Func<int, int, T> function);

        int Width
        {
            get;
        }

        int Height
        {
            get;
        }

        // TODO: Is row increasing from bottom up?
        T this[int column, int row]
        {
            get;
            set;
        }

        // Set all the values in the image to the default
        void Clear();

        // Do the specified i
        bool IsInRange(int i, int j);

        // TODO: These aren't very C#/.NET-ish - try to replace with the
        // views below
        ImageRowIterator<T> GetRowIterator(int row);
        ImageRowIterator<T> GetRowIterator(int column, int row);
        ImageColumnIterator<T> GetColumnIterator(int column);
        ImageColumnIterator<T> GetColumnIterator(int column, int row);

        IImageSlice<T> Row(int row);
        IImageSlice<T> Column(int colum);

        /// <summary>
        /// Applies the given function to each (i, j) coordinate and assign the
        /// result of the function to that pixel
        /// </summary>
        /// <param name="function">A delgate accepting i and j</param>
        void ForEachPixel(Func<int, int, T> function);
    }
}
