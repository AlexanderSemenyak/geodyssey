using System;
using System.Collections.Generic;
using System.Text;

namespace Image
{
    public struct ImageRowIterator<T> :IImageIterator<T>
    {
        #region Fields
        private readonly IImage<T> image;
        private readonly int row;
        private int column;
        #endregion

        #region Construction
        public ImageRowIterator(IImage<T> image, int row)
            :
            this(image, 0, row)
        {
        }

        public ImageRowIterator(IImage<T> image, int column, int row)
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
            set { column = value; }
        }

        public int Row
        {
            get { return row; }
        }

        public int Length
        {
            get { return image.Width; }
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
            ++column;
            return column < image.Width;
        }

        public bool MovePrevious()
        {
            --column;
            return column >= 0;
        }

        public bool Advance(int steps)
        {
            column += steps;
            return column >= 0 && column <= image.Width;
        }

        public IImageIterator<T> Offset(int steps)
        {
            ImageRowIterator<T> result = this;
            result.Advance(steps);
            return result;
        }

        public int Difference(IImageIterator<T> obj)
        {
            if (obj is ImageRowIterator<T>)
            {
                ImageRowIterator<T> rhs = (ImageRowIterator<T>) obj;
                return Difference(rhs);
            }
            throw new ArgumentException();
        }

        public int Difference(ImageRowIterator<T> rhs)
        {
            if (image == rhs.image
             && row == rhs.row)
            {
                return this.column - rhs.column;
            }
            throw new ArgumentException();
        }

        public override bool Equals(object obj)
        {
            if (obj is ImageRowIterator<T>)
            {
                ImageRowIterator<T> rhs = (ImageRowIterator<T>) obj;
                return Equals(rhs);
            }
            return false;
        }

        public bool Equals(ImageRowIterator<T> rhs)
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
