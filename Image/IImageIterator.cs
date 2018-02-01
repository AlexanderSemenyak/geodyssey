using System;
using System.Collections.Generic;
using System.Text;

namespace Image
{
    public interface IImageIterator<T>
    {
        int Column { get; }
        T Current { get; set;}
        IImage<T> Image { get; }
        int Length { get; }
        bool MoveNext();
        bool MovePrevious();
        int Row { get; }
        int Difference(IImageIterator<T> rhs);
        bool Advance(int steps);
        IImageIterator<T> Offset(int steps);
    }
}
