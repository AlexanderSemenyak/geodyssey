using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Utility.Collections
{
    public static class Algorithms
    {
        public static void SwapElements<T>(IList<T> collection, int index1, int index2)
        {
            Debug.Assert(index1 >= 0 && index1 < collection.Count);
            Debug.Assert(index2 >= 0 && index2 < collection.Count);
            T tmp = collection[index1];
            collection[index1] = collection[index2];
            collection[index2] = tmp;
        }

        /// <summary>
        /// Given a sequence containing nullable values, returns the index of the first minimum
        /// element in the sequence or, if the sequence contains only null values, null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <returns></returns>
        public static int? IndexOfMinimumOrNull<T>(IEnumerable<T?> seq)
            where T: struct, IComparable<T>
        {
            int? index = null;
            T minimum = default(T);
            int i = 0;
            foreach (T? element in seq)
            {
                if (element.HasValue)
                {
                    if (index == null) // On first non-null element
                    {
                        minimum = element.Value;
                        index = i;
                    }
                    if (minimum.CompareTo(element.Value) > 0)
                    {
                        minimum = element.Value;
                        index = i;
                    }
                }
                ++i;
            }
            return index;
        }
    }
}
