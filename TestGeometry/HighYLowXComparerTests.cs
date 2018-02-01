using System;
using System.Collections.Generic;
using System.Text;

using Numeric;
using Geometry;
using Geometry.PolygonPartitioning;

namespace Geometry.PolygonPartitioning
{
    using NUnit.Framework;

    [TestFixture]
    public class HighYLowXComparerTests
    {
        [Test]
        public void LessYLessX()
        {
            Point2D a = new Point2D(1.0, 2.0);
            Point2D b = new Point2D(1.5, 2.5);
            HighYLowXComparer comparer = new HighYLowXComparer();
            Assert.Less(comparer.Compare(a, b), 0);
        }

        [Test]
        public void LessYEqualX()
        {
            Point2D a = new Point2D(1.0, 2.0);
            Point2D b = new Point2D(1.0, 2.5);
            HighYLowXComparer comparer = new HighYLowXComparer();
            Assert.Less(comparer.Compare(a, b), 0);
        }

        [Test]
        public void LessYGreaterX()
        {
            Point2D a = new Point2D(1.5, 2.0);
            Point2D b = new Point2D(1.0, 2.5);
            HighYLowXComparer comparer = new HighYLowXComparer();
            Assert.Less(comparer.Compare(a, b), 0);
        }

        [Test]
        public void EqualYLessX()
        {
            Point2D a = new Point2D(1.0, 2.0);
            Point2D b = new Point2D(1.5, 2.0);
            HighYLowXComparer comparer = new HighYLowXComparer();
            Assert.Greater(comparer.Compare(a, b), 0);
        }

        [Test]
        public void EqualYEqualX()
        {
            Point2D a = new Point2D(1.0, 2.0);
            Point2D b = new Point2D(1.0, 2.0);
            HighYLowXComparer comparer = new HighYLowXComparer();
            Assert.AreEqual(comparer.Compare(a, b), 0);
        }

        [Test]
        public void EqualYGreaterX()
        {
            Point2D a = new Point2D(1.5, 2.0);
            Point2D b = new Point2D(1.0, 2.0);
            HighYLowXComparer comparer = new HighYLowXComparer();
            Assert.Less(comparer.Compare(a, b), 0);
        }

        [Test]
        public void GreaterYLessX()
        {
            Point2D a = new Point2D(1.0, 2.5);
            Point2D b = new Point2D(1.5, 2.0);
            HighYLowXComparer comparer = new HighYLowXComparer();
            Assert.Greater(comparer.Compare(a, b), 0);
        }

        [Test]
        public void GreaterYEqualX()
        {
            Point2D a = new Point2D(1.0, 2.5);
            Point2D b = new Point2D(1.0, 2.0);
            HighYLowXComparer comparer = new HighYLowXComparer();
            Assert.Greater(comparer.Compare(a, b), 0);
        }

        [Test]
        public void GreaterYGreaterX()
        {
            Point2D a = new Point2D(1.5, 2.5);
            Point2D b = new Point2D(1.0, 2.0);
            HighYLowXComparer comparer = new HighYLowXComparer();
            Assert.Greater(comparer.Compare(a, b), 0);
        }
    }
}
