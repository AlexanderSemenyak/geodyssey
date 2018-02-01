using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utility.Extensions.System.Collections.Generic;

namespace Utility.Collections
{
    using NUnit.Framework;

    [TestFixture]
    public class EnumerableExtensionTests
    {
        [Test]
        public void IsSortedEmpty()
        {
            var list = new List<int> {};
            Assert.IsTrue(list.IsSorted());
        }

        [Test]
        public void IsSortedOne()
        {
            var list = new List<int> { 1 };
            Assert.IsTrue(list.IsSorted());    
        }

        [Test]
        public void IsSortedPositive()
        {
            List<int> list = new List<int> {1, 2, 4, 8, 16, 32, 64, 128};
            Assert.IsTrue(list.IsSorted());
        }

        [Test]
        public void IsSortedNegative1()
        {
            List<int> list = new List<int> { 2, 1, 4, 8, 16, 32, 64, 128 };
            Assert.IsFalse(list.IsSorted());
        }

        [Test]
        public void IsSortedNegative2()
        {
            List<int> list = new List<int> { 1, 2, 4, 8, 16, 32, 128, 64 };
            Assert.IsFalse(list.IsSorted());
        }

        [Test]
        public void IsSortedNegative3()
        {
            List<int> list = new List<int> { 1, 2, 4, 16, 8, 32, 64, 128 };
            Assert.IsFalse(list.IsSorted());
        }

        [Test]
        public void MergeSortedEqualLength()
        {
            var a = new List<int> { 1, 2, 3, 4,  5,  6,  7,  8,    9 };
            var b = new List<int> { 1, 2, 4, 8, 16, 32, 64, 128, 256 };
            Assert.IsTrue(a.IsSorted());
            Assert.IsTrue(b.IsSorted());
            var c = a.MergeSorted(b);
            Assert.IsTrue(c.IsSorted());
        }

        [Test]
        public void MergeSortedUnequalLength1()
        {
            var a = new List<int> { 1, 2, 3, 4, 5 };
            var b = new List<int> { 1, 2, 4, 8, 16, 32, 64, 128, 256 };
            Assert.IsTrue(a.IsSorted());
            Assert.IsTrue(b.IsSorted());
            var c = a.MergeSorted(b);
            Assert.IsTrue(c.IsSorted());
        }

        [Test]
        public void MergeSortedUnequalLength2()
        {
            var a = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var b = new List<int> { 1, 2, 4, 8, 16 };
            Assert.IsTrue(a.IsSorted());
            Assert.IsTrue(b.IsSorted());
            var c = a.MergeSorted(b);
            Assert.IsTrue(c.IsSorted());
        }
    }
}
