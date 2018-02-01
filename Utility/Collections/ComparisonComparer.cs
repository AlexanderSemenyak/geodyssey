using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility.Collections
{
    // from Patrick Caldwell
    // http://dpatrickcaldwell.blogspot.com/2009/04/converting-comparison-to-icomparer.html

    /// <summary>
    /// A Comparer class that takes a Comparison delegate as a constructor argument
    /// </summary>
    /// <typeparam name="T">The types being compared</typeparam>
    public class ComparisonComparer<T> : IComparer<T>
    {
        private readonly Comparison<T> comparison;

        /// <param name="comparison">The comparison delegate used to construct the comparer</param>
        public ComparisonComparer(Comparison<T> comparison)
        {
            this.comparison = comparison;
        }

        public int Compare(T x, T y)
        {
            return comparison(x, y);
        }
    }
}
