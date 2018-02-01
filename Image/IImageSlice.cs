using System.Collections.Generic;

namespace Image
{
    public interface IImageSlice<T> : IEnumerable<T>
    {
        bool Contains(T item);
        void CopyTo(T[] array, int arrayIndex);
        int Count { get; }
        int IndexOf(T item);
        T this[int index] { get; set; }
    }
}