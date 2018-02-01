using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility.Collections
{
    public class SplayDictionary<K, V> : IDictionary<K, V>
    {
        #region Nested Types
        // TODO: Untested!
        public class KeyCollection : ICollection<K>
        {
            private readonly SplayDictionary<K, V> dictionary;

            public KeyCollection(SplayDictionary<K, V> dictionary)
            {
                this.dictionary = dictionary;
            }

            public IEnumerator<K> GetEnumerator()
            {
                foreach (KeyValuePair<K, V> kv in dictionary)
                {
                    yield return kv.Key;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// Not supported.
            /// </summary>
            /// <param name="item">Not used.</param>
            /// <exception cref="NotSupportedException">Always thrown.</exception>
            void ICollection<K>.Add(K item)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Not supported.
            /// </summary>
            /// <exception cref="NotSupportedException">Always thrown.</exception>
            void ICollection<K>.Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(K item)
            {
                return dictionary.ContainsKey(item);
            }

            ///<summary>
            ///Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
            ///</summary>
            ///
            ///<param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
            ///<param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
            ///<exception cref="T:System.ArgumentNullException"><paramref name="array" /> is null.</exception>
            ///<exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex" /> is less than 0.</exception>
            ///<exception cref="T:System.ArgumentException"><paramref name="array" /> is multidimensional.-or-<paramref name="arrayIndex" /> is equal to or greater than the length of <paramref name="array" />.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.-or-Type <paramref name="T" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
            public void CopyTo(K[] array, int arrayIndex)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }

                if (arrayIndex < 0)
                {
                    throw new ArgumentOutOfRangeException("arrayIndex");
                }

                if (arrayIndex >= array.Length)
                {
                    throw new ArgumentException("array is too short");
                }

                if (Count > array.Length - arrayIndex)
                {
                    throw new ArgumentException("array is too short");
                }

                if (array.Rank != 1)
                {
                    throw new ArgumentException("array is multidimensional");
                }

                foreach (K item in this)
                {
                    array[arrayIndex] = item;
                    ++arrayIndex;
                }
            }

            /// <summary>
            /// Not supported.
            /// </summary>
            /// <param name="item">Not used.</param>
            /// <exception cref="NotSupportedException">Always thrown.</exception>
            bool ICollection<K>.Remove(K item)
            {
                throw new NotSupportedException();
            }

            ///<summary>
            ///Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
            ///</summary>
            ///
            ///<returns>
            ///The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
            ///</returns>
            ///
            public int Count
            {
                get { return dictionary.Count; }
            }

            /// <summary>
            /// Always returns true for this implementation.
            /// </summary>
            public bool IsReadOnly
            {
                get { return true; }
            }
        }

        // TODO: Untested!
        public class ValueCollection : ICollection<V>
        {
            private readonly SplayDictionary<K, V> dictionary;

            public ValueCollection(SplayDictionary<K, V> dictionary)
            {
                this.dictionary = dictionary;
            }

            public IEnumerator<V> GetEnumerator()
            {
                foreach (KeyValuePair<K, V> kv in dictionary)
                {
                    yield return kv.Value;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            void ICollection<V>.Add(V item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Returns true if item is in the collection. This is an
            /// O(n) operation.
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool Contains(V item)
            {
                var comparer = Comparer<V>.Default;
                foreach (V value in this)
                {
                    if (comparer.Compare(value, item) == 0)
                    {
                        return true;
                    }
                }
                return false;
            }

            public void CopyTo(V[] array, int arrayIndex)
            {
                foreach (V item in this)
                {
                    array[arrayIndex] = item;
                    ++arrayIndex;
                }
            }

            bool ICollection<V>.Remove(V item)
            {
                throw new NotSupportedException();
            }

            public int Count
            {
                get { return dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }
        }
        #endregion

        #region Fields
        private readonly SplayTree<KeyValuePair<K, V>> tree;
        #endregion

        #region Construction
        public SplayDictionary()
        {
            var keyComparer = new KeyComparer<K, V>();
            tree = new SplayTree<KeyValuePair<K, V>>(keyComparer);
        }

        public SplayDictionary(IComparer<K> comparer)
        {
            var keyComparer = new KeyComparer<K, V>(comparer);
            tree = new SplayTree<KeyValuePair<K, V>>(keyComparer);
        }

        public SplayDictionary(IEnumerable<KeyValuePair<K, V>> collection) :
            this()
        {
            AddRange(collection);
        }

        public SplayDictionary(IEnumerable<KeyValuePair<K, V>> collection, IComparer<K> comparer) :
            this(comparer)
        {
            AddRange(collection);
        }

        #endregion

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return tree.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<K, V> item)
        {
            tree.Add(item);
        }

        public void AddRange(IEnumerable<KeyValuePair<K, V>> collection)
        {
            foreach (KeyValuePair<K, V> kv in collection)
            {
                Add(kv);
            }
        }

        public void Clear()
        {
            tree.Clear();
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            return tree.Contains(item);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            tree.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            return tree.Remove(item);
        }

        public int Count
        {
            get { return tree.Count; }
        }

        public bool IsReadOnly
        {
            get { return tree.IsReadOnly; }
        }

        public bool ContainsKey(K key)
        {
            return tree.Contains(DefaultValue(key));
        }

        public void Add(K key, V value)
        {
            tree.Add(new KeyValuePair<K, V>(key, value));
        }

        public bool Remove(K key)
        {
            return tree.Remove(DefaultValue(key));
        }

        public bool TryGetValue(K key, out V value)
        {
            KeyValuePair<K, V> result;
            bool success = tree.TryGetValue(DefaultValue(key), out result);
            value = result.Value;
            return success;
        }

        public bool TryGetLargestBelow(K key, out KeyValuePair<K, V> result)
        {
            return tree.TryGetLargestBelow(DefaultValue(key), out result);
        }

        public KeyValuePair<K, V> LargestBelow(K key)
        {
            return tree.LargestBelow(DefaultValue(key));
        }

        public bool TryGetSmallestAbove(K key, out KeyValuePair<K,V> result)
        {
            return tree.TryGetSmallestAbove(DefaultValue(key), out result);
        }

        public KeyValuePair<K, V> SmallestAbove(K key)
        {
            return tree.SmallestAbove(DefaultValue(key));
        }

        public V this[K key]
        {
            get
            {
                V value;
                bool success = TryGetValue(key, out value);
                if (!success)
                {
                    throw new KeyNotFoundException(key.ToString());
                }
                return value;
            }
            set
            {
                if (ContainsKey(key))
                {
                    Remove(key);
                }
                Add(key, value);
            }
        }

        public ICollection<K> Keys
        {
            get { return new KeyCollection(this); }
        }

        public ICollection<V> Values
        {
            get { return new ValueCollection(this); }
        }

        public KeyValuePair<K, V> Root
        {
            get { return tree.Root; }
        }

        public KeyValuePair<K, V> Left
        {
            get { return tree.Left; }
        }

        public KeyValuePair<K, V> Right
        {
            get { return tree.Right;  }
        }

        public KeyValuePair<K, V> Minimum
        {
            get { return tree.Minimum; }
        }

        public KeyValuePair<K, V> Maximum
        {
            get { return tree.Maximum; }
        }

        public bool IsEmpty
        {
            get { return tree.IsEmpty; }
        }

        public override string ToString()
        {
            return tree.ToString();
        }

        private static KeyValuePair<K, V> DefaultValue(K key)
        {
            return new KeyValuePair<K, V>(key, default(V));
        }

    }
}
