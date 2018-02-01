using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utility.Collections;

namespace Utility.Extensions.System.Collections.Generic
{
    // from Patrick Caldwell
    // http://dpatrickcaldwell.blogspot.com/2009/04/icomparer-extension-methods-for-fluent.html
    public static class IComparerExtensions
    {
        public static IComparer<T> Then<T>(this IComparer<T> priority, IComparer<T> then)
        {
            return new MultipleComparer<T>(priority, then);
        }

        /// <summary>
        /// Invert the sense of a comparer
        /// </summary>
        /// <typeparam name="T">The type of element being compared</typeparam>
        /// <param name="comparer">The comparer to be inverted</param>
        /// <returns>The inverted comparer</returns>
        public static IComparer<T> Invert<T>(this IComparer<T> comparer)
        {
            return new ComparisonComparer<T>((x, y) => comparer.Compare(x, y) * -1);
        }

        public static IComparer<TSource> Transform<TSource, TResult>(this IComparer<TResult> comparer, Func<TSource, TResult> transformation)
        {
            return new ComparisonComparer<TSource>((x, y) => comparer.Compare(transformation(x), transformation(y)));
        }
    }
}
