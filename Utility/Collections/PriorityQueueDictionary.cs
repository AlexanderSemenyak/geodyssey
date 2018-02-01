using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Wintellect.PowerCollections;

namespace Utility.Collections
{
    // TODO: Either remove this class entirely and replace all uses with PriorityQueuePair
    //       with the appropriate comparators, or rename it, since it does not implement
    //       IDictionary.

    // TODO: The documentation in this file needs updating

    /// <summary>
    /// A heap based priority queue. Algorithms with a produced
    /// from the queue in order of decreasing priority.
    /// </summary>
    // TODO: Refactor this class to only use one type paramater and support
    // TODO: the current use case with KeyValuePair - better factoring of functionality
    public class PriorityQueueDictionary<P, T> : ICollection<T>
    {
        #region Fields
        // The comparer used to compare items. 
        private readonly IComparer<P> comparer;

        // The array of priorities and items which stores the heap
        private readonly List<Pair<P, T>> heap;
        #endregion

        #region Construction
        /// <summary>
        /// Create a new PriorityQueue which dequeues items with priorities which
        /// compare low before items with priorities which compare high.
        /// </summary>
        /// <returns>A new PriorityQueue instance.</returns>
        public static PriorityQueueDictionary<P, T> CreateLowFirstOut()
        {
            return new PriorityQueueDictionary<P, T>(
                (lhs, rhs) => Comparer<P>.Default.Compare(rhs, lhs));
        }

        /// <summary>
        /// Create a new PriorityQueue which dequeues items with priorities which
        /// compare high before items with priorities which compare low.
        /// </summary>
        /// <returns>A new PriorityQueue instance.</returns>
        public static PriorityQueueDictionary<P, T> CreateHighFirstOut()
        {
            return new PriorityQueueDictionary<P, T>();
        }

        /// <summary>
        /// Creates a new PriorityQueue. The P must implement IComparable&lt;P&gt;
        /// or IComparable. The CompareTo method of this interface will be used to
        /// prioritise items as they are inserted into the queue.
        /// </summary>
        /// <exception cref="InvalidOperationException">T does not implement IComparable&lt;TKey&gt;.</exception>
        public PriorityQueueDictionary() :
            this(Comparers.DefaultComparer<P>())
        {
        }

        /// <summary>
        /// Creates a new PriorityQueue. The passed delegate will be used to compare
        /// item priorities as they are inserted into the queue.
        /// </summary>
        /// <param name="comparison">A delegate to a method that will be used to compare priorities.</param>
        public PriorityQueueDictionary(Comparison<P> comparison) :
            this(Comparers.ComparerFromComparison(comparison))
        {
        }

        /// <summary>
        /// Creates a new PriorityQueue. The Compare method of he passed comparison object will
        /// be used to compare priorities as items are inserted into the queue.
        /// </summary>
        /// <param name="comparer">An instance of IComparer&lt;T&gt; that will be used to compare priorities.</param>
        public PriorityQueueDictionary(IComparer<P> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            this.comparer = comparer;
            heap = new List<Pair<P, T>>();
        }

        /// <summary>
        /// Creates a new PriorityQueue. The P must implement IComparable&lt;P&gt;
        /// or IComparable.
        /// The CompareTo method of this interface will be used to compare item priorities as
        /// they are inserted into the queue.
        /// The PriorityQueue is initialized with all the items in the given collection.
        /// </summary>
        /// <param name="collection">A collection with items to be placed into the PriorityQueue.</param>
        public PriorityQueueDictionary(IEnumerable<T> collection) :
            this(collection, Comparers.DefaultComparer<P>())
        {
        }

        /// <summary>
        /// Creates a new PriorityQueue. The passed delegate will be used to compare
        /// item priorities as they are inserted into the queue.
        /// The PriorityQueue is initialized with all the items in the given collection.
        /// </summary>
        /// <param name="collection">A collection with items to be placed into the PriorityQueue.</param>
        /// <param name="comparison">A delegate to a method that will be used to compare priorities.</param>
        public PriorityQueueDictionary(IEnumerable<T> collection, Comparison<P> comparison) :
            this(collection, Comparers.ComparerFromComparison(comparison))
        {
        }

        /// <summary>
        /// Creates a new PriorityQueue. The Compare method of he passed comparison object will
        /// be used to compare priorities as items are inserted into the queue.
        /// The PriorityQueue is initialized with all the items in the given collection.
        /// </summary>
        /// <param name="collection">A collection with items to be placed into the PriorityQueue</param>
        /// <param name="comparer">An instance of IComparer&lt;T&gt; that will be used to compare priorities.</param>
        public PriorityQueueDictionary(IEnumerable<T> collection, IComparer<P> comparer) :
            this(comparer)
        {
            Wintellect.PowerCollections.Algorithms.ForEach(collection, Add);
        }

        /// <summary>
        /// Creates a new PriorityQueue. The P must implement IComparable&lt;P&gt;
        /// or IComparable. The CompareTo method of this interface will be used to compare item priorities as
        /// they are inserted into the queue.
        /// The PriorityQueue is initialized with all the priorities and items in Pairs in the given collection.
        /// </summary>
        /// <param name="collection">A collection with Pairs of priorities and items to be placed into the PriorityQueue</param>
        public PriorityQueueDictionary(IEnumerable<Pair<P, T>> collection) :
            this(collection, Comparers.DefaultComparer<P>())
        {
        }

        /// <summary>
        /// Creates a new PriorityQueue. The passed delegate will be used to compare item priorities as they are inserted into the queue.
        /// The PriorityQueue is initialized with all the priorities and items in Pairs in the given collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="comparison">A delegate to a method that will be used to compare priorities.</param>
        public PriorityQueueDictionary(IEnumerable<Pair<P, T>> collection, Comparison<P> comparison) :
            this(collection, Comparers.ComparerFromComparison(comparison))
        {
        }

        /// <summary>
        /// Creates a new PriorityQueue. The Compare method of he passed comparison object will
        /// be used to compare priorities as items are inserted into the queue.
        /// The PriorityQueue is initialized with all the priorities and items in Pairs in the given collection.
        /// </summary>
        /// <param name="collection">A collection with Pairs of priorities and items to be placed into the PriorityQueue</param>
        /// <param name="comparer">An instance of IComparer&lt;T&gt; that will be used to compare priorities.</param>
        public PriorityQueueDictionary(IEnumerable<Pair<P, T>> collection, IComparer<P> comparer) :
            this(comparer)
        {
            Wintellect.PowerCollections.Algorithms.ForEach(collection, item => Enqueue(item.First, item.Second));
        }


        #endregion

        #region ICollection<T> Members

        /// <summary>
        /// Enqueue an item with the default priority value as defined by applying
        /// default(P) to the priority type P.
        /// </summary>
        /// <param name="item">The item to be enqueued.</param>
        public void Add(T item)
        {
            heap.Add(new Pair<P, T>(default(P), item));
            BubbleUp(heap.Count - 1);
        }

        /// <summary>
        /// Removes all items from the queue.
        /// </summary>
        public void Clear()
        {
            heap.Clear();
        }

        /// <summary>
        /// Tests if the queue contains the specified item.
        /// </summary>
        /// <param name="item">The item to be searched for</param>
        /// <returns>true if the queue contains the item</returns>
        public bool Contains(T item)
        {
            foreach (T value in this)
            {
                if (value.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Copy the contents of the queue to the array. The order of the items
        /// in the array is unspecified.
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="arrayIndex">The index in the target array to which the queue contents will be copied</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (T item in this)
            {
                array[arrayIndex] = item;
                ++arrayIndex;
            }
        }

        /// <summary>
        /// The number of items in the queue. Complexity O(1) time.
        /// </summary>
        public int Count
        {
            get { return heap.Count; }
        }

        /// <summary>
        /// True if the queue is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the specified item from the queue.
        /// </summary>
        /// <param name="item">The item to be removed from the queue.</param>
        /// <returns>True if the item was found and removed.</returns>
        public bool Remove(T item)
        {
            for (int index = 0; index < heap.Count; ++index)
            {
                if (heap[index].Second.Equals(item))
                {
                    RemoveAt(index);
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region IEnumerable<T> Members
        public IEnumerator<T> GetEnumerator()
        {
            return new PriorityQueueEnumerator(this);
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new PriorityQueueEnumerator(this);
        }
        #endregion

        #region Properties
        public T Front
        {
            get { return heap[0].Second; }
        }
        #endregion

        #region Methods
        public void Enqueue(P priority, T item)
        {
            heap.Add(new Pair<P, T>(priority, item));
            BubbleUp(heap.Count - 1);
        }

        public T Dequeue()
        {
            Debug.Assert(Count != 0);
            T result = heap[0].Second;
            RemoveAt(0);
            return result;
        }

        private void BubbleUp(int index)
        {
            int parent = GetParent(index);
            while ((index > 0) && (comparer.Compare(heap[parent].First, heap[index].First) < 0))
            {
                Algorithms.SwapElements(heap, index, parent);
                index = parent;
                parent = GetParent(index);
            }
        }

        private void TrickleDown(int index)
        {
            int child = GetLeftChild(index);
            while (child < Count)
            {
                if (((child + 1) < Count &&
                    (comparer.Compare(heap[child].First, heap[child + 1].First) < 0)))
                {
                    ++child;
                }
                Algorithms.SwapElements(heap, index, child);
                index = child;
                child = GetLeftChild(index);
            }
            BubbleUp(index);
        }

        private static int GetLeftChild(int index)
        {
            return (index * 2) + 1;
        }

        private static int GetParent(int index)
        {
            return (index - 1) / 2;
        }

        private void RemoveAt(int index)
        {
            heap[index] = heap[Count - 1];
            heap.RemoveAt(Count - 1);
            TrickleDown(index);
            if (heap.Count < heap.Capacity / 2)
            {
                heap.TrimExcess();
            }
        }
        #endregion

        #region Nested Classes
        private class PriorityQueueEnumerator : IEnumerator<T>
        {
            private readonly PriorityQueueDictionary<P, T> priorityQueueDictionary;
            private int index;

            public PriorityQueueEnumerator(PriorityQueueDictionary<P, T> priorityQueueDictionary)
            {
                this.priorityQueueDictionary = priorityQueueDictionary;
                Reset();
            }

            #region IEnumerator<T> Members

            public T Current
            {
                get { return priorityQueueDictionary.heap[index].Second; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                // do nothing
            }

            #endregion

            #region IEnumerator Members

            object System.Collections.IEnumerator.Current
            {
                get { return priorityQueueDictionary.heap[index].Second; }
            }

            public bool MoveNext()
            {
                if (index + 1 == priorityQueueDictionary.Count)
                {
                    return false;
                }
                ++index;
                return true;
            }

            public void Reset()
            {
                index = -1;
            }

            #endregion
        }
        #endregion
    }
}
