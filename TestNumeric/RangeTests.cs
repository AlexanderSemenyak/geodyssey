using System;
using System.Collections.Generic;
using System.Text;

namespace Numeric
{
    using NUnit.Framework;

    [TestFixture]
    public class RangeTests
    {
        // Tests for Range.Clip(double, double, double)

        [Test]
        public void RangeClipDoubleIncreasingWithin()
        {
            Assert.AreEqual(Range.Clip(1.0, 0.0, 2.0), 1.0);
        }

        [Test]
        public void RangeClipDoubleIncreasingBelow()
        {
            Assert.AreEqual(Range.Clip(-1.0, 0.0, 2.0), 0.0);
        }

        [Test]
        public void RangeClipDoubleIncreasingAbove()
        {
            Assert.AreEqual(Range.Clip(3.0, 0.0, 2.0), 2.0);
        }

        [Test]
        public void RangeClipDoubleIncreasingLowerBoundary()
        {
            Assert.AreEqual(Range.Clip(0.0, 0.0, 2.0), 0.0);
        }

        [Test]
        public void RangeClipDoubleIncreasingUpperBoundary()
        {
            Assert.AreEqual(Range.Clip(2.0, 0.0, 2.0), 2.0);
        }

        [Test]
        public void RangeClipDoubleDecreasingWithin()
        {
            Assert.AreEqual(Range.Clip(1.0, 2.0, 0.0), 1.0);
        }

        [Test]
        public void RangeClipDoubleDecreasingBelow()
        {
            Assert.AreEqual(Range.Clip(-1.0, 2.0, 0.0), 0.0);
        }

        [Test]
        public void RangeClipDoubleDecreasingAbove()
        {
            Assert.AreEqual(Range.Clip(3.0, 2.0, 0.0), 2.0);
        }

        [Test]
        public void RangeClipDoubleDecreasingLowerBoundary()
        {
            Assert.AreEqual(Range.Clip(0.0, 2.0, 0.0), 0.0);
        }

        [Test]
        public void RangeClipDoubleDecreasingUpperBoundary()
        {
            Assert.AreEqual(Range.Clip(2.0, 2.0, 0.0), 2.0);
        }

        [Test]
        public void RangeClipDoubleEqualWithin()
        {
            Assert.AreEqual(Range.Clip(1.0, 1.0, 1.0), 1.0);
        }

        [Test]
        public void RangeClipDoubleEqualBelow()
        {
            Assert.AreEqual(Range.Clip(-1.0, 1.0, 1.0), 1.0);
        }

        [Test]
        public void RangeClipDoubleEqualAbove()
        {
            Assert.AreEqual(Range.Clip(3.0, 1.0, 1.0), 1.0);
        }

        [Test]
        public void RangeClipDoubleEqualLowerBoundary()
        {
            Assert.AreEqual(Range.Clip(0.0, 1.0, 1.0), 1.0);
        }

        [Test]
        public void RangeClipDoubleEqualUpperBoundary()
        {
            Assert.AreEqual(Range.Clip(2.0, 1.0, 1.0), 1.0);
        }

        // Tests for Range.Clip(int, int, int)

        [Test]
        public void RangeClipIntIncreasingWithin()
        {
            Assert.AreEqual(Range.Clip(1, 0, 2), 1);
        }

        [Test]
        public void RangeClipIntIncreasingBelow()
        {
            Assert.AreEqual(Range.Clip(-1, 0, 2), 0);
        }

        [Test]
        public void RangeClipIntIncreasingAbove()
        {
            Assert.AreEqual(Range.Clip(3, 0, 2), 2);
        }

        [Test]
        public void RangeClipIntIncreasingLowerBoundary()
        {
            Assert.AreEqual(Range.Clip(0, 0, 2), 0);
        }

        [Test]
        public void RangeClipIntIncreasingUpperBoundary()
        {
            Assert.AreEqual(Range.Clip(2, 0, 2), 2);
        }

        [Test]
        public void RangeClipIntDecreasingWithin()
        {
            Assert.AreEqual(Range.Clip(1, 2, 0), 1);
        }

        [Test]
        public void RangeClipIntDecreasingBelow()
        {
            Assert.AreEqual(Range.Clip(-1, 2, 0), 0);
        }

        [Test]
        public void RangeClipIntDecreasingAbove()
        {
            Assert.AreEqual(Range.Clip(3, 2, 0), 2);
        }

        [Test]
        public void RangeClipIntDecreasingLowerBoundary()
        {
            Assert.AreEqual(Range.Clip(0, 2, 0), 0);
        }

        [Test]
        public void RangeClipIntDecreasingUpperBoundary()
        {
            Assert.AreEqual(Range.Clip(2, 2, 0), 2);
        }

        [Test]
        public void RangeClipIntEqualWithin()
        {
            Assert.AreEqual(Range.Clip(1, 1, 1), 1);
        }

        [Test]
        public void RangeClipIntEqualBelow()
        {
            Assert.AreEqual(Range.Clip(-1, 1, 1), 1);
        }

        [Test]
        public void RangeClipIntEqualAbove()
        {
            Assert.AreEqual(Range.Clip(3, 1, 1), 1);
        }

        [Test]
        public void RangeClipIntEqualLowerBoundary()
        {
            Assert.AreEqual(Range.Clip(0, 1, 1), 1);
        }

        [Test]
        public void RangeClipIntEqualUpperBoundary()
        {
            Assert.AreEqual(Range.Clip(2, 1, 1), 1);
        }
    }
}
