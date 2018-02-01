using System;
using System.Collections.Generic;
using System.Text;

namespace Numeric
{
    using NUnit.Framework;

    [TestFixture]
    public class Ray2DTests
    {
        [Test]
        public void SupportingLineHorizontal()
        {
            Ray2D ray = new Ray2D(new Point2D(0.0, 0.0), new Direction2D(1.0, 0.0));
            Line2D line = ray.SupportingLine;
            Assert.AreEqual(0.0, line.A);
            Assert.AreEqual(1.0, line.B);
            Assert.AreEqual(0.0, line.C);
        }

        [Test]
        public void SupportingLineVertical()
        {
            Ray2D ray = new Ray2D(new Point2D(0.0, 0.0), new Direction2D(0.0, 1.0));
            Line2D line = ray.SupportingLine;
            Assert.AreEqual(-1.0, line.A);
            Assert.AreEqual(0.0, line.B);
            Assert.AreEqual(0.0, line.C);
        }

        [Test]
        public void SupportLineDiagonal()
        {
            Ray2D ray = new Ray2D(new Point2D(0.0, 0.0), new Direction2D(1.0, 1.0));
            Line2D line = ray.SupportingLine;
            Assert.AreEqual(-1.0, line.A);
            Assert.AreEqual(1.0, line.B);
            Assert.AreEqual(0.0, line.C);
        }

        [Test]
        public void SupportLineDiagonalOffset()
        {
            Ray2D ray = new Ray2D(new Point2D(1.0, 0.0), new Direction2D(1.0, 1.0));
            Line2D line = ray.SupportingLine;
            Assert.AreEqual(-1.0, line.A);
            Assert.AreEqual(1.0, line.B);
            Assert.AreEqual(1.0, line.C);
        }

        [Test]
        public void SupportLineDiagonalOffset2()
        {
            Ray2D ray = new Ray2D(new Point2D(2.0, 1.0), new Direction2D(-2.0, 3.0));
            Line2D line = ray.SupportingLine;
            Assert.AreEqual(-3.0, line.A);
            Assert.AreEqual(-2.0, line.B);
            Assert.AreEqual(8.0, line.C);
        }

    }
}
