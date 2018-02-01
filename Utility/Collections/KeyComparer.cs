using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility.Collections
{
    /// <summary>
    /// Comparer for comparing KeyValuePair objects by key only.
    /// </summary>
    /// <typeparam name="K">The key type of KeyValuePair</typeparam>
    /// <typeparam name="V">The value type of KeyValuePair</typeparam>
    public class KeyComparer<K, V> : IComparer<KeyValuePair<K, V>>
    {
        private readonly IComparer<K> comparer;

        /// <summary>
        /// Use the default comparer for the K type.
        /// </summary>
        public KeyComparer()
        {
            comparer = Comparers.DefaultComparer<K>();
        }

        /// <summary>
        /// Provide a specific IComparer for the key type K.
        /// </summary>
        /// <param name="comparer">A specific comparer for type K.</param>
        public KeyComparer(IComparer<K> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            this.comparer = comparer;
        }

        ///<summary>
        ///Compares two objects by their keys and returns a value indicating whether one is less than, equal to, or greater than the other.
        ///</summary>
        ///
        ///<returns>
        ///Key Condition Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
        ///</returns>
        ///
        ///<param name="x">The first object to compare.</param>
        ///<param name="y">The second object to compare.</param>
        public int Compare(KeyValuePair<K, V> x, KeyValuePair<K, V> y)
        {
            return comparer.Compare(x.Key, y.Key);
        }
    }
}
