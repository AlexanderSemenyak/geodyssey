using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Numeric
{
    // TODO: Come up with a much better class name
    public static class Limits
    {
        public static T Min<T>(T a, T b, IComparer<T> comparer)
        {
            int cmp = comparer.Compare(a, b);
            return cmp < 0 ? a : b;
        }

        public static T Max<T>(T a, T b, IComparer<T> comparer)
        {
            int cmp = comparer.Compare(a, b);
            return cmp > 0 ? a : b;
        }
    }
}
