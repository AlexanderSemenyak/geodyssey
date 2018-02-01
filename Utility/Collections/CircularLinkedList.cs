// Authors:
//    David Waite
//    Robert Smallshire
//
// (C) 2005 David Waite (mass@akuma.org)
// (C) 2008 Robert Smallshire (robert@smallshire.org.uk)

//
// Copyright (C) 2005 David Waite
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Utility.Collections
{
    /// <summary>
    /// A linked list which can either be circular or linear.
    /// 
    /// Iteration:
    /// 
    /// The most convenient way to traverse the elements within the list, irrespective of list topology
    /// is to use a standard foreach loop
    /// 
    /// foreach (T t in list)
    /// {
    /// }
    /// 
    /// If it is required to visit each CircularLinkedListNode within the the, use the following construct
    /// 
    /// CircularLinkedListNode&lt;T&gt; node = list.First;
    /// do
    /// {
    /// }
    /// while (node != list.Last.Next);
    /// </summary>
    /// <typeparam name="T">The type to be stored in the list</typeparam>
    [Serializable, ComVisible(false)]
    public class CircularLinkedList<T> :ICollection<T>, ICollection, ISerializable, IDeserializationCallback
    {
        const string DataArrayKey = "DataArray";
        const string VersionKey = "version";
        uint count, version;
        readonly object syncRoot;
        bool isCircular = true;
        // Internally always circular list - first.back == last
        internal CircularLinkedListNode<T> first;
        internal SerializationInfo si;

        public CircularLinkedList()
        {
            syncRoot = new object();
            first = null;
            count = version = 0;
        }

        public CircularLinkedList(IEnumerable<T> collection)
            : this()
        {
            foreach (T item in collection)
                AddLast(item);
        }

        protected CircularLinkedList(SerializationInfo info, StreamingContext context)
            : this()
        {
            si = info;
            syncRoot = new object();
        }

        void VerifyReferencedNode(CircularLinkedListNode<T> node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.List != this)
                throw new InvalidOperationException();
        }

        static void VerifyBlankNode(CircularLinkedListNode<T> newNode)
        {
            if (newNode == null)
                throw new ArgumentNullException("newNode");

            if (newNode.List != null)
                throw new InvalidOperationException();
        }

        public CircularLinkedListNode<T> AddAfter(CircularLinkedListNode<T> node, T value)
        {
            VerifyReferencedNode(node);
            CircularLinkedListNode<T> newNode = new CircularLinkedListNode<T>(this, value, node, node.forward);
            count++;
            version++;
            return newNode;
        }

        public void AddAfter(CircularLinkedListNode<T> node, CircularLinkedListNode<T> newNode)
        {
            VerifyReferencedNode(node);
            VerifyBlankNode(newNode);
            newNode.InsertBetween(node, node.forward, this);
            count++;
            version++;
        }

        public CircularLinkedListNode<T> AddBefore(CircularLinkedListNode<T> node, T value)
        {
            VerifyReferencedNode(node);
            CircularLinkedListNode<T> newNode = new CircularLinkedListNode<T>(this, value, node.back, node);
            count++;
            version++;

            if (node == first)
                first = newNode;
            return newNode;
        }

        public void AddBefore(CircularLinkedListNode<T> node, CircularLinkedListNode<T> newNode)
        {
            VerifyReferencedNode(node);
            VerifyBlankNode(newNode);
            newNode.InsertBetween(node.back, node, this);
            count++;
            version++;

            if (node == first)
                first = newNode;
        }

        public void AddFirst(CircularLinkedListNode<T> node)
        {
            VerifyBlankNode(node);
            if (first == null)
                node.SelfReference(this);
            else
                node.InsertBetween(first.back, first, this);
            count++;
            version++;
            first = node;
        }

        public CircularLinkedListNode<T> AddFirst(T value)
        {
            CircularLinkedListNode<T> newNode;
            if (first == null)
                newNode = new CircularLinkedListNode<T>(this, value);
            else
                newNode = new CircularLinkedListNode<T>(this, value, first.back, first);
            count++;
            version++;
            first = newNode;
            return newNode;
        }

        public CircularLinkedListNode<T> AddLast(T value)
        {
            CircularLinkedListNode<T> newNode;
            if (first == null)
            {
                newNode = new CircularLinkedListNode<T>(this, value);
                first = newNode;
            }
            else
                newNode = new CircularLinkedListNode<T>(this, value, first.back, first);
            count++;
            version++;
            return newNode;
        }

        public void AddLast(CircularLinkedListNode<T> node)
        {
            VerifyBlankNode(node);
            if (first == null)
            {
                node.SelfReference(this);
                first = node;
            }
            else
                node.InsertBetween(first.back, first, this);
            count++;
            version++;
        }

        /// <summary>
        /// Removes nodes from the arguments list and inserts them at the beginning of the target list.
        /// </summary>
        /// <param name="srcFirst">The first node in the source list.</param>
        /// <param name="srcLast">The last node in the source list.</param>
        public void SpliceFirst(CircularLinkedListNode<T> srcFirst, CircularLinkedListNode<T> srcLast)
        {
            if (srcFirst == null)
            {
                throw new ArgumentNullException("srcFirst");
            }
            if (srcLast == null)
            {
                throw new ArgumentNullException("srcLast");
            }
            if (srcFirst.List != srcLast.List)
            {
                throw new InvalidOperationException("source nodes not in same list");
            }
            CircularLinkedList<T> srcList = srcFirst.List;
            CircularLinkedListNode<T> terminator = srcFirst.Previous;
            CircularLinkedListNode<T> node = srcLast;
            do
            {
                CircularLinkedListNode<T> previousNode = node.Previous;
                srcList.Remove(node);
                this.AddFirst(node);
                node = previousNode;
            }
            while (node != terminator);
        }

        /// <summary>
        /// Removes nodes from the arguments list and inserts them at the end of the target list.
        /// </summary>
        /// <param name="srcFirst">The first node in the source list.</param>
        /// <param name="srcLast">The last node in the source list.</param>
        public void SpliceLast(CircularLinkedListNode<T> srcFirst, CircularLinkedListNode<T> srcLast)
        {
            if (srcFirst == null)
            {
                throw new ArgumentNullException("srcFirst");
            }
            if (srcLast == null)
            {
                throw new ArgumentNullException("srcLast");
            }
            if (srcFirst.List != srcLast.List)
            {
                throw new InvalidOperationException("source nodes not in same list");
            }
            CircularLinkedList<T> srcList = srcFirst.List;
            CircularLinkedListNode<T> terminator = srcLast.Next;
            CircularLinkedListNode<T> node = srcFirst;
            do
            {
                CircularLinkedListNode<T> nextNode = node.Next;
                srcList.Remove(node);
                this.AddLast(node);
                node = nextNode;
            }
            while (node != terminator);
        }

        public void Clear()
        {
            count = 0;
            first = null;
            version++;
        }

        public bool Contains(T value)
        {
            CircularLinkedListNode<T> node = first;
            if (node == null)
                return false;
            do
            {
                if (value.Equals(node.Value))
                    return true;
                node = node.forward;
            }
            while (node != first);

            return false;
        }

        public void CopyTo(T[] array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if ((uint) index < (uint) array.GetLowerBound(0))
                throw new ArgumentOutOfRangeException("index");
            if (array.Rank != 1)
                throw new ArgumentException("Array is multidimensional", "array");
            if (array.Length - index + array.GetLowerBound(0) < count)
                throw new ArgumentException("number of items exceeds capacity");

            CircularLinkedListNode<T> node = first;
            if (first == null)
                return;
            do
            {
                array[index] = node.Value;
                index++;
                node = node.forward;
            }
            while (node != first);
        }

        public CircularLinkedListNode<T> Find(T value)
        {
            CircularLinkedListNode<T> node = first;
            if (node == null)
                return null;
            do
            {

                // TODO: Correct the treatment of null here
                if (Equals(value, node.Value))
                {
                    return node;
                }
                node = node.forward;
            }
            while (node != first);

            return null;
        }

        public CircularLinkedListNode<T> FindPair(T value1, T value2)
        {
            uint startVersion = version;
            CircularLinkedListNode<T> node = this.First;
            if (node != null)
            {
                do
                {
                    if (node.Next != null)
                    {
                        bool firstEqual = Equals(value1, node.Value);
                        bool secondEqual = Equals(value2, node.Next.Value);
                        if (firstEqual && secondEqual)
                        {
                            return node;
                        }
                    }
                    if (version != startVersion)
                    {
                        throw new InvalidOperationException("list modified");
                    }
                    node = node.Next;
                }
                while (node != null && node != this.Last.Next);
            }
            return null;
        }

        public CircularLinkedListNode<T> Find(Predicate<T> match)
        {
            uint startVersion = version;
            CircularLinkedListNode<T> node = first;
            if (node != null)
            {
                do
                {
                    if (match(node.Value))
                    {
                        return node;
                    }
                    if (version != startVersion)
                    {
                        throw new InvalidOperationException("list modified");
                    }
                    node = node.forward;
                }
                while (node != first);
            }
            return null;
        }

        public CircularLinkedListNode<T> FindPair(Func<T, T, bool> predicate)
        {
            uint startVersion = version;
            CircularLinkedListNode<T> node = this.First;
            if (node != null)
            {
                do
                {
                    if (node.Next != null)
                    {
                        if (predicate(node.Value, node.Next.Value))
                        {
                            return node;
                        }
                    }
                    if (version != startVersion)
                    {
                        throw new InvalidOperationException("list modified");
                    }
                    node = node.Next;
                }
                while (node != null && node != this.Last.Next);
            }
            return null;
        }

        public CircularLinkedListNode<T> FindNode(Predicate<CircularLinkedListNode<T>> match)
        {
            return FindNodeFrom(first, match);
        }

        public CircularLinkedListNode<T> FindNodeFrom(CircularLinkedListNode<T> node, Predicate<CircularLinkedListNode<T>> match)
        {
            uint startVersion = version;
            if (node != null)
            {
                do
                {
                    if (match(node))
                    {
                        return node;
                    }
                    if (version != startVersion)
                    {
                        throw new InvalidOperationException("list modified");
                    }
                    node = node.forward;
                }
                while (node != first);
            }
            return null;
        }

        public CircularLinkedListNode<T> FindNodePair(Func<CircularLinkedListNode<T>, CircularLinkedListNode<T>, bool> predicate)
        {
            uint startVersion = version;
            CircularLinkedListNode<T> node = this.First;
            if (node != null)
            {
                do
                {
                    if (node.Next != null)
                    {
                        if (predicate(node, node.Next))
                        {
                            return node;
                        }
                    }
                    if (version != startVersion)
                    {
                        throw new InvalidOperationException("list modified");
                    }
                    node = node.Next;
                }
                while (node != null && node != this.Last.Next);
            }
            return null;
        }

        public CircularLinkedListNode<T> FindLast(T value)
        {
            CircularLinkedListNode<T> node = first;
            if (node == null)
                return null;
            do
            {
                node = node.back;
                if (value.Equals(node.Value))
                    return node;
            }
            while (node != first);

            return null;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            T[] data = new T[count];
            CopyTo(data, 0);
            info.AddValue(DataArrayKey, data, typeof(T[]));
            info.AddValue(VersionKey, version);
        }

        public virtual void OnDeserialization(object sender)
        {
            if (si != null)
            {
                T[] data = (T[]) si.GetValue(DataArrayKey, typeof(T[]));
                if (data != null)
                    foreach (T item in data)
                        AddLast(item);
                version = si.GetUInt32(VersionKey);
                si = null;
            }
        }

        public bool Remove(T value)
        {
            CircularLinkedListNode<T> node = Find(value);
            if (node == null)
                return false;
            Remove(node);
            return true;
        }

        public void Remove(CircularLinkedListNode<T> node)
        {
            VerifyReferencedNode(node);
            count--;
            if (count == 0)
                first = null;

            if (node == first)
// ReSharper disable PossibleNullReferenceException
                first = first.forward;
// ReSharper restore PossibleNullReferenceException

            version++;
            node.Detach();
        }

        public void RemoveFirst()
        {
            if (first != null)
                Remove(first);
        }

        public void RemoveLast()
        {
            if (first != null)
                Remove(first.back);
        }

        public void ForEach(Action<T> action)
        {
            uint startVersion = version;
            CircularLinkedListNode<T> node = this.First;
            if (node != null)
            {
                do
                {   
                    action(node.Value);
                    if (version != startVersion)
                    {
                        throw new InvalidOperationException("list modified");
                    }
                    node = node.Next;
                }
                while (node != this.Last.Next);
            }
        }

        public void ForEachNode(Action<CircularLinkedListNode<T>> action)
        {
            uint startVersion = version;
            CircularLinkedListNode<T> node = this.First;
            if (node != null)
            {
                do
                {
                    action(node);
                    if (version != startVersion)
                    {
                        throw new InvalidOperationException("list modified");
                    }
                    node = node.Next;
                }
                while (node != this.Last.Next);
            }
        }

        public void ForEachPair(Action<T, T> action)
        {
            uint startVersion = version;
            CircularLinkedListNode<T> node = this.First;
            if (node != null)
            {
                do
                {
                    if (node.Next != null)
                    {
                        action(node.Value, node.Next.Value);
                    }
                    if (version != startVersion)
                    {
                        throw new InvalidOperationException("list modified");
                    }
                    node = node.Next;
                }
                while (node != null && node != this.Last.Next);
            }
        }

        public void ForEachNodePair(Action<CircularLinkedListNode<T>, CircularLinkedListNode<T>> action)
        {
            uint startVersion = version;
            CircularLinkedListNode<T> node = this.First;
            if (node != null)
            {
                do
                {
                    if (node.Next != null)
                    {
                        action(node, node.Next);
                    }
                    if (version != startVersion)
                    {
                        throw new InvalidOperationException("list modified");
                    }
                    node = node.Next;
                }
                while (node != null && node != this.Last.Next);
            }
        }

        void ICollection<T>.Add(T value)
        {
            AddLast(value);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            T[] Tarray = array as T[];
            if (Tarray == null)
                throw new ArgumentException("array");
            CopyTo(Tarray, index);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return (int) count; }
        }

        public bool IsCircular
        {
            get { return isCircular; }
            set
            {
                ++version;
                isCircular = value;
            }
        }

        public CircularLinkedListNode<T> First
        {
            get { return first; }
        }

        public CircularLinkedListNode<T> Last
        {
            get { return (first != null) ? first.back : null; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return syncRoot; }
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Enumerator :IEnumerator<T>, IDisposable, IEnumerator
#if !NET_2_1
, ISerializable, IDeserializationCallback
#endif
        {
            const String VersionKey = "version";
            const String IndexKey = "index";
            const String ListKey = "list";

            CircularLinkedList<T> list;
            CircularLinkedListNode<T> current;
            int index;
            readonly uint version;
#if !NET_2_1
            SerializationInfo si;

            internal Enumerator(SerializationInfo info, StreamingContext context)
            {
                si = info;
                list = (CircularLinkedList<T>) si.GetValue(ListKey, typeof(CircularLinkedList<T>));
                index = si.GetInt32(IndexKey);
                version = si.GetUInt32(VersionKey);
                current = null;
            }
#endif

            internal Enumerator(CircularLinkedList<T> parent)
            {
#if !NET_2_1
                si = null;
#endif
                this.list = parent;
                current = null;
                index = -1;
                version = parent.version;
            }

            public T Current
            {
                get
                {
                    if (list == null)
                        throw new ObjectDisposedException(null);
                    if (current == null)
                        throw new InvalidOperationException();
                    return current.Value;
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                if (list == null)
                    throw new ObjectDisposedException(null);
                if (version != list.version)
                    throw new InvalidOperationException("list modified");

                if (current == null)
                    current = list.first;
                else
                {
                    current = current.forward;
                    if (current == list.first)
                        current = null;
                }
                if (current == null)
                {
                    index = -1;
                    return false;
                }
                ++index;
                return true;
            }

            void IEnumerator.Reset()
            {
                if (list == null)
                    throw new ObjectDisposedException(null);
                if (version != list.version)
                    throw new InvalidOperationException("list modified");

                current = null;
                index = -1;
            }

            public void Dispose()
            {
                if (list == null)
                    throw new ObjectDisposedException(null);
                current = null;
                list = null;
            }

#if !NET_2_1
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (list == null)
                    throw new ObjectDisposedException(null);
                info.AddValue(VersionKey, version);
                info.AddValue(IndexKey, index);
            }

            void IDeserializationCallback.OnDeserialization(object sender)
            {
                if (si == null)
                    return;

                if (list.si != null)
                    ((IDeserializationCallback) list).OnDeserialization(this);

                si = null;

                if (version == list.version && index != -1)
                {
                    CircularLinkedListNode<T> node = list.First;

                    for (int i = 0; i < index; i++)
                        node = node.forward;

                    current = node;
                }
            }
#endif
        }
    }
}

