using System;
using System.Collections;
using System.Collections.Generic;

namespace Image
{
    internal class ImageColumn<T> : ImageSlice<T>
    {
        private readonly int column;

        public ImageColumn(IImage<T> image, int column)
        {
            this.Parent = image;
            this.column = column;
        }

        public override int Count
        {
            get { return Parent.Height; }
        }

        public override T this[int row]
        {
            get { return Parent[column, row]; }
            set { Parent[column, row] = value; }
        }
    }
}