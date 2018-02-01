using System;
using System.Collections.Generic;
using System.Text;

using Utility;

namespace Utility.Collections
{
    using NUnit.Framework;

    [TestFixture]
    public class AlgorithmsTests
    {
        [Test]
        public void IndexOfMinimumOrNullAllNull()
        {
            int?[] array = { null, null, null, null };
            int? result = Algorithms.IndexOfMinimumOrNull<int>(array);
            Assert.IsNull(result);
        }

        [Test]
        public void IndexOfMinimumOrNullAllValues()
        {
            int?[] array = { 4, 3, 2, 5 };
            int? result = Algorithms.IndexOfMinimumOrNull<int>(array);
            Assert.AreEqual(2, result);
        }

        [Test]
        public void IndexOfMinimumOrNullAllEqualValues()
        {
            int?[] array = { 3, 3, 3, 3 };
            int? result = Algorithms.IndexOfMinimumOrNull<int>(array);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void IndexOfMinimumOrNullMixed()
        {
            int?[] array = { 5, null, 3, null };
            int? result = Algorithms.IndexOfMinimumOrNull<int>(array);
            Assert.AreEqual(2, result);
        }
    }
}
