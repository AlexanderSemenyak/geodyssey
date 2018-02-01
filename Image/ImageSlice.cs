using System;
using System.Collections;
using System.Collections.Generic;

namespace Image
{
    internal abstract class ImageSlice<T> : IImageSlice<T>
    {
        private IImage<T> image;
        public abstract int Count { get; }

        public IImage<T> Parent
        {
            get { return image; }
            protected set { image = value; }
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if ((uint) arrayIndex < (uint) array.GetLowerBound(0))
                throw new ArgumentOutOfRangeException("arrayIndex");
            if (array.Rank != 1)
                throw new ArgumentException("Array is multidimensional", "array");
            if (array.Length - arrayIndex + array.GetLowerBound(0) < Count)
                throw new ArgumentException("number of items exceeds capacity");
            foreach (T item in this)
            {
                array[arrayIndex] = item;
                ++arrayIndex;
            }
        }

        public int IndexOf(T item)
        {
            for (int j = 0; j < Count; ++j)
            {
                if (this[j].Equals(item))
                {
                    return j;
                }
            }
            return -1;    
        }

        public abstract T this[int index] { get; set; }

        public IEnumerator<T> GetEnumerator()
        {
            for (int j = 0; j < Count; ++j)
            {
                yield return this[j];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}