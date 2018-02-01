using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Utility.Collections
{
    /// <summary>
    /// Top-down splay tree.
    /// </summary>
    /// <typeparam name="T">The stored element type</typeparam>
    public class SplayTree<T> : ICollection<T>
    {
        // Translated from Java code by Danny Sleator, one of the
        // original splay-tree inventors from an original in Java
        // http://www.link.cs.cmu.edu/link/ftp-site/splaying/SplayTree.java
        //
        // Modification included making the class a generic container using
        // the features of C# 2.0

        private class BinaryNode
        {
            internal T element;
            internal BinaryNode left;
            internal BinaryNode right;

            internal BinaryNode(T element) :
                this(element, null, null)
            {

            }

            internal BinaryNode(T element, BinaryNode left, BinaryNode right)
            {
                this.element = element;
                this.left = left;
                this.right = right;
            }
        }

        #region Fields
        private readonly IComparer<T> comparer;
        private BinaryNode root;
        // TODO: These statics prevent thread-safety
        private static readonly BinaryNode nullNode = null;
        private static BinaryNode newNode = null;
        private static readonly BinaryNode header = new BinaryNode(default(T));
        private int? count = 0;

        #endregion

        #region Construction
        static SplayTree()
        {
            nullNode = new BinaryNode(default(T));
            nullNode.left = nullNode.right = nullNode;
        }

        public SplayTree() :
            this(Comparers.DefaultComparer<T>())
        {
        }

        public SplayTree(IComparer<T> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            this.comparer = comparer;
            root = nullNode;
        }

        public SplayTree(IEnumerable<T> collection) :
            this(collection, Comparers.DefaultComparer<T>())
        {
        }

        public SplayTree(IEnumerable<T> collection, IComparer<T> comparer) :
            this(comparer)
        {
            Wintellect.PowerCollections.Algorithms.ForEach(collection, Add);
        }
        #endregion

        /// <summary>
        /// The Root element, is the element which was most recently accessed.
        /// It can be accessed in O(1) time.
        /// </summary>
        public T Root
        {
            get { return root.element; }
        }

        /// <summary>
        /// The Left element is the left branch from the root element, and
        /// contains the element less than or equal to the Root element.
        /// </summary>
        public T Left
        {
            get { return root.left.element; }
        }

        /// <summary>
        /// The Right element is the right branch from the root element, and
        /// contains the element greater than or equal to the Root element.
        /// </summary>
        public T Right
        {
            get { return root.right.element; }
        }

        /// <summary>
        /// Merge two splay trees, assumes all of the items in rhs are
        /// greater than last.
        /// All of the items in rhs are transferred to this, replacing all
        /// elements in this tree greater than last. After the
        /// operator rhs IsEmpty.
        /// </summary>
        /// <param name="last">The last (greatest) element of this tree that will be preserved</param>
        /// <param name="rhs">The tree to be merged in</param>
        /// <returns>The merged result tree</returns>
        /// <exception cref="ArgumentNullException">Throws in rhs is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if all elements in rhs are not greater than last.</exception>
        public SplayTree<T> Merge(T last, SplayTree<T> rhs)
        {
            if (rhs == null)
            {
                throw new ArgumentNullException("rhs", "Merge attemped with null rhs SplayTree");
            }

            if (rhs.IsEmpty)
            {
                return this;
            }

            if (comparer.Compare(last, rhs.Minimum) > 0)
            {
                throw new ArgumentOutOfRangeException("rhs", rhs, "All elements in the rhs SplayTree must be greater than last.");
            }
            Debug.Assert(comparer.Compare(last, rhs.Minimum) <= 0);

            if (IsEmpty)
            {
                root = rhs.root;
                count = rhs.count;
                rhs.Clear();
                return this;
            }
            Contains(last);
            root.right = rhs.root;
            count = null; // Invalidate count
            rhs.Clear();
            return this;
        }

        /// <summary>
        /// Split the splay tree into to at the given value.  Elements less than value
        /// will remain in this tree, elements greater than value will be in the returned tree.
        /// </summary>
        /// <param name="value">The value at which the split is to be made.</param>
        /// <param name="keep">Controls whether elements equal to value are remain in this tree (true)
        /// or are moved to the returned tree (false).</param>
        /// <returns></returns>
        public SplayTree<T> SplitAt(T value, bool keep)
        {
            SplayTree<T> leftover = new SplayTree<T>();
            Contains(value);
            int comp = comparer.Compare(value, root.element);
            if (comp > 0 || comp == 0 && keep)
            {
                leftover.root = root.right;
                root.right = nullNode;
            }
            else
            {
                leftover.root = root;
                root = root.left;
                leftover.root.left = nullNode;
            }
            count = null; // Invalidate count
            leftover.count = null; // Invalidate count
            return leftover;
        }

        /// <summary>
        /// Add (insert) into the tree
        /// </summary>
        /// <param name="item">The item to insert</param>
        public void Add(T item)
        {
            if (newNode == null)
            {
                newNode = new BinaryNode(default(T));
            }
            newNode.element = item;
            if (root == nullNode)
            {
                newNode.left = newNode.right = nullNode;
                root = newNode;
                ++count;
            }
            else
            {
                root = Splay(item, root);
                int order = comparer.Compare(item, root.element);
                if (order < 0)
                {
                    newNode.left = root.left;
                    newNode.right = root;
                    root.left = nullNode;
                    root = newNode;
                    ++count;
                }
                else if (order > 0)
                {
                    newNode.right = root.right;
                    newNode.left = root;
                    root.right = nullNode;
                    root = newNode;
                    ++count;
                }
                else
                {
                    return;
                }
            }
            newNode = null;
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="array" /> is multidimensional.-or-<paramref name="arrayIndex" /> is equal to or greater than the length of <paramref name="array" />.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.-or-Type <paramref name="T" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
        public void CopyTo(T[] array, int arrayIndex)
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

           foreach (T item in this)
           {
               array[arrayIndex] = item;
               ++arrayIndex;
           }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// Complexity O(1) time.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public int Count
        {
            get
            {
                if (count == null)
                {
                    count = 0;
                    foreach (T item in this)
                    {
                        ++count;
                    }
                }
                return count.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public bool Remove(T item)
        {
            if (IsReadOnly)
            {
                throw new NotSupportedException("SplayTree is read-only");
            }

            root = Splay(item, root);

            if (comparer.Compare(root.element, item) != 0)
            {
                return false;
            }
            BinaryNode newTree;
            if (root.left == nullNode)
            {
                newTree = root.right;
            }
            else
            {
                newTree = root.left;
                newTree = Splay(item, newTree);
                newTree.right = root.right;
            }
            root = newTree;
            --count;
            return true;
        }

        /// <summary>
        /// Find the smallest item in the tree. Throws IndexOutOfRangeException if the tree is empty.
        /// </summary>
        /// <returns>The smallest item in the tree.</returns>
        public T Minimum
        {
            get
            {
                if (IsEmpty)
                {
                    throw new IndexOutOfRangeException();
                }
                BinaryNode ptr = root;
                while (ptr.left != nullNode)
                {
                    ptr = ptr.left;
                }
                root = Splay(ptr.element, root);
                return ptr.element;
            }
        }

        /// <summary>
        /// Find the largest item in the tree. Throws IndexOutOfRangeException if the tree is empty.
        /// </summary>
        /// <returns>The largest item in the tree.</returns>
        public T Maximum
        {
            get
            {
                if (IsEmpty)
                {
                    throw new IndexOutOfRangeException();
                }
                BinaryNode ptr = root;
                while (ptr.right != nullNode)
                {
                    ptr = ptr.right;
                }
                root = Splay(ptr.element, root);
                return ptr.element;
            }
        }

        /// <summary>
        /// Try to find the required value, returning it in result if it exists.
        /// This is useful because, depending on the Comparer provided when the tree
        /// was constructed, the returned value may compare equal with the value, but
        /// may not be identical.
        /// </summary>
        /// <param name="value">The value to find</param>
        /// <param name="result">The value found.</param>
        /// <returns>True if value was found, otherwise false.</returns>
        public bool TryGetValue(T value, out T result)
        {
            if (Contains(value))
            {
                result = Root;
                return true;
            }
            result = default(T);
            return false;
        }

        /// <summary>
        /// Finds the smallest element above to the given value. 
        /// </summary>
        /// <param name="value">The query value</param>
        /// <param name="result">The smallest element greater-than the value, if the method succeeded,
        /// or the default value of T if it failed.</param>
        /// <returns>True if an element smaller than value was found; otherwise false</returns>
        public bool TryGetSmallestAbove(T value, out T result)
        {
            BinaryNode aptr = nullNode;
            BinaryNode ptr;
            for (ptr = root; ptr != nullNode; )
            {
                if (comparer.Compare(ptr.element, value) > 0)
                {
                    aptr = ptr;
                    ptr = ptr.left;
                }
                else
                {
                    ptr = ptr.right;
                }
            }

            ptr = aptr;
            root = ptr != nullNode ? Splay(ptr.element, root) : Splay(value, root);
            if (IsEmpty || ptr == nullNode || comparer.Compare(ptr.element, value) <= 0)
            {
                result = default(T);
                return false;
            }
            result = root.element;
            return true;
        }

        public T SmallestAbove(T value)
        {
            T result;
            bool success = TryGetSmallestAbove(value, out result);
            if (!success)
            {
                throw new IndexOutOfRangeException();
            }
            return result;
        }

        /// <summary>
        /// Finds the largest element below the given value
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown if there is no such element.
        /// </exception>
        /// <param name="item">The query value.</param>
        /// <param name="result">The largest element less-than the value if the method succeeded; or the default
        /// value if it failed.</param>
        /// <returns>true if an element larger than value was found; otherwise false</returns>
        public bool TryGetLargestBelow(T item, out T result)
        {
            BinaryNode aptr = nullNode;
            BinaryNode ptr;
            for (ptr = root; ptr != nullNode; )
            {
                if (comparer.Compare(ptr.element, item) < 0)
                {
                    aptr = ptr;
                    ptr = ptr.right;
                }
                else
                {
                    ptr = ptr.left;
                }
            }

            ptr = aptr;
            root = ptr != nullNode ? Splay(ptr.element, root) : Splay(item, root);
            if (IsEmpty || ptr == nullNode || comparer.Compare(ptr.element, item) >= 0)
            {
                result = default(T);
                return false;
            }
            result = root.element;
            return true;
        }

        public T LargestBelow(T item)
        {
            T result;
            bool success = TryGetLargestBelow(item, out result);
            if (!success)
            {
                throw new IndexOutOfRangeException();
            }
            return result;
        }

        public void Clear()
        {
            root = nullNode;
            count = 0;
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// Immediately following a call to this method, the Root property will refer
        /// to the requested element if it exists.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public bool Contains(T item)
        {
            root = Splay(item, root);
            return !(IsEmpty || comparer.Compare(root.element, item) != 0);
        }

        public bool IsEmpty
        {
            get { return root == nullNode; }
        }

        private BinaryNode Splay(T item, BinaryNode t)
        {
            header.left = header.right = nullNode;
            BinaryNode rightTreeMin = header;
            BinaryNode leftTreeMax = header;
            nullNode.element = item;
            do
            {
                int comp = comparer.Compare(item, t.element);
                if (comp < 0)
                {
                    if (comparer.Compare(item, t.left.element) < 0)
                    {
                        t = RotateWithLeftChild(t);
                    }
                    if (t.left == nullNode)
                    {
                        break;
                    }
                    rightTreeMin.left = t;
                    rightTreeMin = t;
                    t = t.left;
                    continue;
                }
                if (comp <= 0) // TODO: Factor this comparison out
                {
                    break;
                }
                if (comparer.Compare(item, t.right.element) > 0)
                {
                    t = RotateWithRightChild(t);
                }
                if (t.right == nullNode)
                {
                    break;
                }
                leftTreeMax.right = t;
                leftTreeMax = t;
                t = t.right;
            }
            while (true);
            leftTreeMax.right = t.left;
            rightTreeMin.left = t.right;
            t.left = header.right;
            t.right = header.left;
            nullNode.element = default(T);
            return t;
        }

        private static BinaryNode RotateWithLeftChild(BinaryNode k2)
        {
            BinaryNode k1 = k2.left;
            k2.left = k1.right;
            k1.right = k2;
            return k1;
        }

        private static BinaryNode RotateWithRightChild(BinaryNode k1)
        {
            BinaryNode k2 = k1.right;
            k1.right = k2.left;
            k2.left = k1;
            return k2;
        }

        private static IEnumerable<T> ScanInOrder(BinaryNode node)
        {
            if (node != nullNode)
            {
                if (node.left != null && node.left != nullNode)
                {
                    foreach (T item in ScanInOrder(node.left))
                    {
                        yield return item;
                    }
                }
                yield return node.element;
                if (node.right != null && node.right != nullNode)
                {
                    foreach (T item in ScanInOrder(node.right))
                    {
                        yield return item;
                    }
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (T item in ScanInOrder(root))
            {
                yield return item;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return ToString(root);
        }

        private static string ToString(BinaryNode node)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            if (node != nullNode)
            {
                sb.Append(" ");
                sb.Append(ToString(node.left));
                sb.Append(" ");
                sb.Append(node.element);
                sb.Append(" ");
                sb.Append(ToString(node.right));
                sb.Append(" ");
            }
            sb.Append(")");
            return sb.ToString();
        }
    }
}
