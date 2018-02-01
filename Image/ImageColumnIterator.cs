using System;
using System.Collections.Generic;
using System.Text;

namespace Image
{
    public struct ImageColumnIterator<T> :IImageIterator<T>
    {
        #region Fields
        private readonly IImage<T> image;
        private int row;
        private readonly int column;
        #endregion

        #region Construction
        public ImageColumnIterator(IImage<T> image, int column)
            :
            this(image, column, 0)
        {
        }

        public ImageColumnIterator(IImage<T> image, int column, int row)
        {
            this.image = image;
            this.column = column;
            this.row = row;
        }
        #endregion

        #region Properties
        public IImage<T> Image
        {
            get { return image; }
        }

        public int Column
        {
            get { return column; }
        }

        public int Row
        {
            get { return row; }
            set { row = value; }
        }

        public int Length
        {
            get { return image.Height; }
        }

        public T Current
        {
            get { return image[column, row]; }
            set { image[column, row] = value; }
        }
        #endregion

        #region Methods
        public bool MoveNext()
        {
            ++row;
            return row < image.Height;
        }

        public bool MovePrevious()
        {
            --row;
            return row >= 0;
        }

        public bool Advance(int steps)
        {
            row += steps;
            return row >= 0 && row <= image.Height;
        }

        public IImageIterator<T> Offset(int steps)
        {
            ImageColumnIterator<T> result = this;
            result.Advance(steps);
            return result;
        }

        public int Difference(IImageIterator<T> obj)
        {
            if (obj is ImageColumnIterator<T>)
            {
                ImageColumnIterator<T> rhs = (ImageColumnIterator<T>) obj;
                return Difference(rhs);
            }
            throw new ArgumentException();
        }

        public int Difference(ImageColumnIterator<T> rhs)
        {

            if (this.image == rhs.image
             && this.column == rhs.column)
            {
                return this.row - rhs.row;
            }
            throw new ArgumentException();
        }

        public override bool Equals(object obj)
        {
            if (obj is ImageColumnIterator<T>)
            {
                ImageColumnIterator<T> rhs = (ImageColumnIterator<T>) obj;
                return Equals(rhs);
            }
            return false;
        }

        public bool Equals(ImageColumnIterator<T> rhs)
        {
            return image == rhs.image && row == rhs.row && column == rhs.column;
        }

        public override int GetHashCode()
        {
            int hashcode = 0;
            if (image != null)
            {
                hashcode ^= image.GetHashCode();
            }
            hashcode ^= row;
            hashcode ^= column;
            return hashcode;
        }
        #endregion
    }
}
