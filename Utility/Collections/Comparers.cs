using System;
using System.Collections.Generic;
using System.Text;

namespace Utility.Collections
{
    public static class Comparers
    {
        /// <summary>
        /// Class to change an Comparison&lt;T&gt; to an IComparer&lt;T&gt;.
        /// </summary>
        [Serializable]
        class ComparisonComparer<T> : IComparer<T>
        {
            private Comparison<T> comparison;

            public ComparisonComparer(Comparison<T> comparison)
            { this.comparison = comparison; }

            public int Compare(T x, T y)
            { return comparison(x, y); }

            public override bool Equals(object obj)
            {
                if (obj is ComparisonComparer<T>)
                    return comparison.Equals(((ComparisonComparer<T>) obj).comparison);
                else
                    return false;
            }

            public override int GetHashCode()
            {
                return comparison.GetHashCode();
            }
        }

        /// <summary>
        /// Given an Comparison on a type, returns an IComparer on that type. 
        /// </summary>
        /// <typeparam name="T">T to compare.</typeparam>
        /// <param name="comparison">Comparison delegate on T</param>
        /// <returns>IComparer that uses the comparison.</returns>
        public static IComparer<T> ComparerFromComparison<T>(Comparison<T> comparison)
        {
            if (comparison == null)
                throw new ArgumentNullException("comparison");

            return new ComparisonComparer<T>(comparison);
        }

        /// <summary>
        /// Given an element type, check that it implements IComparable&lt;T&gt; or IComparable, then returns
        /// a IComparer that can be used to compare elements of that type.
        /// </summary>
        /// <returns>The IComparer&lt;T&gt; instance.</returns>
        /// <exception cref="InvalidOperationException">T does not implement IComparable&lt;T&gt;.</exception>
        public static IComparer<T> DefaultComparer<T>()
        {
            if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)) ||
                typeof(System.IComparable).IsAssignableFrom(typeof(T)))
            {
                return Comparer<T>.Default;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
