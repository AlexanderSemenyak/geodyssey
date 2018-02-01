using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility.Collections
{
    using NUnit.Framework;

    [TestFixture]
    public class SplayTreeTests
    {
        private SplayTree<int> tree;

        [Test]
        public void DefaultConstructor()
        {
            tree = new SplayTree<int>();
            Assert.IsTrue(tree.IsEmpty);
            Assert.AreEqual(0, tree.Count);
        }

        [Test]
        public void ConstructFromEnumerable()
        {
            int[] array = new int[] {41, 92, 71, 12, 20};
            tree = new SplayTree<int>(array);
            Assert.AreEqual(array.Length, tree.Count);
            foreach (int item in array)
            {
                Assert.IsTrue(tree.Contains(item));
            }
        }

        [Test]
        public void EmptinessTest()
        {
            tree = new SplayTree<int>();
            tree.Add(42);
            Assert.IsFalse(tree.IsEmpty);
        }

        [Test]
        public void EnumerationTest()
        {
            int[] array = new int[] {64, 3, 16, 73, 67};
            tree = new SplayTree<int>(array);
            IEnumerator<int> e = tree.GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual(3, e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual(16, e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual(64, e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual(67, e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual(73, e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        [Test]
        public void EnumerationEmptyTest()
        {
            tree = new SplayTree<int>();
            IEnumerator<int> e = tree.GetEnumerator();
            Assert.IsFalse(e.MoveNext());
        }

        [Test]
        public void ClearTest()
        {
            int[] array = new int[] { 100, 29, 77, 59, 61 };
            tree = new SplayTree<int>(array);
            Assert.IsFalse(tree.IsEmpty);
            tree.Clear();
            Assert.IsTrue(tree.IsEmpty);
            Assert.AreEqual(0, tree.Count);
        }

        [Test]
        public void CountTest()
        {
            tree = new SplayTree<int>();
            tree.Add(5);
            Assert.AreEqual(1, tree.Count);
            tree.Add(3);
            Assert.AreEqual(2, tree.Count);
            tree.Remove(5);
            Assert.AreEqual(1, tree.Count);
            tree.Add(7);
            Assert.AreEqual(2, tree.Count);
            tree.Add(9);
            Assert.AreEqual(3, tree.Count);
            tree.Add(5);
            Assert.AreEqual(4, tree.Count);
            tree.Remove(7);
            Assert.AreEqual(3, tree.Count);
        }

        [Test]
        public void RootTest()
        {
            tree = new SplayTree<int>{5, 3, 7};
            Assert.IsTrue(tree.Contains(5));
            Assert.AreEqual(5, tree.Root);
            Assert.AreEqual(7, tree.Right);
            Assert.AreEqual(3, tree.Minimum);
            Assert.AreEqual(7, tree.Maximum);
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void MinimumFailTest()
        {
            tree = new SplayTree<int>();
            int dummy = tree.Minimum;
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void MaximumFailTest()
        {
            tree = new SplayTree<int>();
            int dummy = tree.Maximum;
        }

        [Test]
        public void MergeTest()
        {
            tree = new SplayTree<int> {5, 3, 7, 12};
            SplayTree<int> tree2 = new SplayTree<int> {11, 8, 13};
            tree.Merge(7, tree2);
            Assert.AreEqual(6, tree.Count);
            Assert.AreEqual(0, tree2.Count);
            Assert.IsTrue(tree.Contains(3));
            Assert.IsTrue(tree.Contains(5));
            Assert.IsTrue(tree.Contains(7));
            Assert.IsTrue(tree.Contains(8));
            Assert.IsTrue(tree.Contains(11));
            Assert.IsFalse(tree.Contains(12));
            Assert.IsTrue(tree.Contains(13));
        }

        [Test]
        public void SplitKeepTest()
        {
            tree = new SplayTree<int> {5, 11, 8, 13, 3, 7, 12};
            SplayTree<int> leftover = tree.SplitAt(11, true);
            Assert.AreEqual(5, tree.Count);
            Assert.AreEqual(2, leftover.Count);
            Assert.IsTrue(tree.Contains(3));
            Assert.IsTrue(tree.Contains(5));
            Assert.IsTrue(tree.Contains(7));
            Assert.IsTrue(tree.Contains(8));
            Assert.IsTrue(tree.Contains(11));
            Assert.IsFalse(tree.Contains(12));
            Assert.IsFalse(tree.Contains(13));
            Assert.IsTrue(leftover.Contains(12));
            Assert.IsTrue(leftover.Contains(13));
        }

        [Test]
        public void SplitNotKeepTest()
        {
            tree = new SplayTree<int> { 5, 11, 8, 13, 3, 7, 12 };
            SplayTree<int> leftover = tree.SplitAt(11, false);
            Assert.AreEqual(4, tree.Count);
            Assert.AreEqual(3, leftover.Count);
            Assert.IsTrue(tree.Contains(3));
            Assert.IsTrue(tree.Contains(5));
            Assert.IsTrue(tree.Contains(7));
            Assert.IsTrue(tree.Contains(8));
            Assert.IsFalse(tree.Contains(11));
            Assert.IsFalse(tree.Contains(12));
            Assert.IsFalse(tree.Contains(13));
            Assert.IsTrue(leftover.Contains(11));
            Assert.IsTrue(leftover.Contains(12));
            Assert.IsTrue(leftover.Contains(13));
        }

        [Test]
        public void TryGetValuePositiveTest()
        {
            int[] array = new int[] {64, 3, 16, 73, 67};
            tree = new SplayTree<int>(array);
            int result;
            bool success = tree.TryGetValue(16, out result);
            Assert.IsTrue(success);
            Assert.AreEqual(16, result);
        }

        [Test]
        public void TryGetValueNegativeTest()
        {
            int[] array = new int[] { 64, 3, 16, 73, 67 };
            tree = new SplayTree<int>(array);
            int result;
            bool success = tree.TryGetValue(15, out result);
            Assert.IsFalse(success);
        }

        [Test]
        public void FindLargestBelowTest()
        {
            tree = new SplayTree<int> { 5, 11, 8, 13, 3, 7, 12 };
            Assert.AreEqual(8, tree.LargestBelow(10));
        }

        [Test]
        public void FindSmallestAboveTest()
        {
            tree = new SplayTree<int> { 5, 11, 8, 13, 3, 7, 12 };
            Assert.AreEqual(11, tree.SmallestAbove(8));
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void FindLargestBelowTestFail()
        {
            tree = new SplayTree<int> { 5, 11, 8, 13, 3, 7, 12 };
            tree.LargestBelow(3);
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void FindSmallestAboveTestFail()
        {
            tree = new SplayTree<int> { 5, 11, 8, 13, 3, 7, 12 };
            tree.SmallestAbove(13);
        }

        [Test]
        public void RemoveFailTest()
        {
            tree = new SplayTree<int> { 77, 85, 66, 56, 74, 94, 27, 61, 48, 57 };
            Assert.IsFalse(tree.Remove(34));
        }

        [Test]
        public void WeissTest()
        {
            tree = new SplayTree<int>();
            const int NUMS = 40000;
            const int GAP = 307;

            for (int i = GAP; i != 0 ; i = (i + GAP) % NUMS)
            {
                tree.Add(i);
            }

            for (int i = 1; i < NUMS; i+=2)
            {
                tree.Remove(i);
            }

            Assert.AreEqual(2, tree.Minimum);
            Assert.AreEqual(NUMS - 2, tree.Maximum);

            for (int i = 2; i < NUMS ; i += 2)
            {
                Assert.IsTrue(tree.Contains(i));
            }

            for (int i = 1; i < NUMS ; i += 2)
            {
                Assert.IsFalse(tree.Contains(i));
            }
        }
    }
}
