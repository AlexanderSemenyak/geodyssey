using System;
using System.Collections;
using System.Collections.Generic;

namespace Image
{
    internal class ImageRow<T> : ImageSlice<T>
    {
        private readonly int row;

        public ImageRow(IImage<T> image, int row)
        {
            this.Parent = image;
            this.row = row;
        }

        public override int Count
        {
            get { return Parent.Width; }
        }

        public override T this[int column]
        {
            get { return Parent[column, row]; }
            set { Parent[column, row] = value; }
        }
    }
}