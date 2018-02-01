using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility.Collections
{
    // from Patrick Caldwell - renamed from ComparerProxy
    // http://dpatrickcaldwell.blogspot.com/2009/04/icomparer-proxy-to-sort-by-multiple.html

    /// <summary>
    /// Lexicographical comparison using multiple comparers in sequence
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MultipleComparer<T> : IComparer<T>
    {
        private readonly IComparer<T>[] comparers;

        /// <summary>
        /// Construct a MultipleComparer from the supplied IComparers
        /// </summary>
        /// <param name="comparers"></param>
        public MultipleComparer(params IComparer<T>[] comparers)
        {
            this.comparers = comparers;
        }

        public int Compare(T x, T y)
        {
            int retVal = 0, i = 0;

            while (retVal == 0 && i < comparers.Length)
                retVal = comparers[i++].Compare(x, y);

            return retVal;
        }
    }
}


