using System;
using System.Collections.Generic;
using System.Text;

namespace Numeric
{
    using NUnit.Framework;

    [TestFixture]
    public class Line2DTests
    {
        [Test]
        public void ConstructFromPointsHorizonatal()
        {
            Point2D a = new Point2D(0.0, 2.5);
            Point2D b = new Point2D(1.0, 2.5);
            Line2D line = new Line2D(a, b);
            Assert.AreEqual(0.0, line.A);
            Assert.AreEqual(1.0, line.B);
            Assert.AreEqual(-2.5, line.C);
        }

        [Test]
        public void ConstructFromPointsVertical()
        {
            Point2D a = new Point2D(3.5, 0.0);
            Point2D b = new Point2D(3.5, 1.0);
            Line2D line = new Line2D(a, b);
            Assert.AreEqual(-1.0, line.A);
            Assert.AreEqual(0.0, line.B);
            Assert.AreEqual(3.5, line.C);
        }

        [Test]
        public void ConstructFromPointsXDominant()
        {
            Point2D a = new Point2D(0.0, 0.0);
            Point2D b = new Point2D(2.0, 1.0);
            Line2D line = new Line2D(a, b);
            Assert.AreEqual(-1.0, line.A);
            Assert.AreEqual(2.0, line.B);
            Assert.AreEqual(0.0, line.C);
        }

        [Test]
        public void ConstructFromPointsYDominant()
        {
            Point2D a = new Point2D(0.0, 0.0);
            Point2D b = new Point2D(1.0, 2.0);
            Line2D line = new Line2D(a, b);
            Assert.AreEqual(-2.0, line.A);
            Assert.AreEqual(1.0, line.B);
            Assert.AreEqual(0.0, line.C);
        }

        [Test]
        public void SolveGradientOne()
        {
            Point2D a = new Point2D(0.0, 0.0);
            Point2D b = new Point2D(1.0, 1.0);
            Line2D line = new Line2D(a, b);
            Assert.AreEqual(line.SolveForX(0.0), line.SolveForY(0.0));
            Assert.AreEqual(line.SolveForX(1.0), line.SolveForY(1.0));
            Assert.AreEqual(line.SolveForX(2.0), line.SolveForY(2.0));
            Assert.AreEqual(line.SolveForX(3.0), 3.0);
            Assert.AreEqual(line.SolveForX(-3.0), -3.0);
            Assert.AreEqual(line.SolveForY(3.0), 3.0);
            Assert.AreEqual(line.SolveForY(-3.0), -3.0);
        }

        [Test]
        public void SolveGradientTwo()
        {
            Point2D a = new Point2D(0.0, 0.0);
            Point2D b = new Point2D(1.0, 2.0);
            Line2D line = new Line2D(a, b);
            Assert.AreEqual(line.SolveForX(2.0), 1.0);
            Assert.AreEqual(line.SolveForX(-3.0), -1.5);
            Assert.AreEqual(line.SolveForY(3.0), 6.0);
            Assert.AreEqual(line.SolveForY(-3.0), -6.0);
        }

        [Test]
        public void SolveGradientHalf()
        {
            Point2D a = new Point2D(0.0, 0.0);
            Point2D b = new Point2D(2.0, 1.0);
            Line2D line = new Line2D(a, b);
            Assert.AreEqual(line.SolveForX(2.0), 4.0);
            Assert.AreEqual(line.SolveForX(-3.0), -6.0);
            Assert.AreEqual(line.SolveForY(3.0), 1.5);
            Assert.AreEqual(line.SolveForY(-3.0), -1.5);
        }

        [Test]
        public void DistanceToVerticalLine()
        {
            Point2D a = new Point2D(1.0, 0.0);
            Point2D b = new Point2D(1.0, 1.0);
            Line2D line = new Line2D(a, b);
            Assert.AreEqual(-1.0, line.DistanceTo(new Point2D(2.0, 3.0)));
            Assert.AreEqual(1.0, line.DistanceTo(new Point2D(0.0, 3.0)));
        }

        [Test]
        public void DistanceToHorizontalLine()
        {
            Point2D a = new Point2D(0.0, 1.0);
            Point2D b = new Point2D(1.0, 1.0);
            Line2D line = new Line2D(a, b);
            Assert.AreEqual(line.DistanceTo(new Point2D(3.0, 2.0)), 1.0);
            Assert.AreEqual(line.DistanceTo(new Point2D(3.0, 0.0)), -1.0);
        }

        [Test]
        public void DistanceToDiagonalLine()
        {
            Point2D a = new Point2D(0.0, 0.0);
            Point2D b = new Point2D(1.0, 1.0);
            Line2D line = new Line2D(a, b);
            Assert.AreEqual(line.DistanceTo(new Point2D(1.0, 0.0)), -0.7071067, 0.0000001);
        }

        [Test]
        public void IsVerticalPositive()
        {
            Point2D a = new Point2D(0.0, 0.0);
            Point2D b = new Point2D(0.0, 1.0);
            Line2D line = new Line2D(a, b);
            Assert.IsTrue(line.IsVertical);
        }

        [Test]
        public void IsVerticalNegative()
        {
            Point2D a = new Point2D(0.0, 0.0);
            Point2D b = new Point2D(1.0, 1.0);
            Line2D line = new Line2D(a, b);
            Assert.IsFalse(line.IsVertical);
        }

        [Test]
        public void IsHorizontalPositive()
        {
            Point2D a = new Point2D(0.0, 0.0);
            Point2D b = new Point2D(1.0, 0.0);
            Line2D line = new Line2D(a, b);
            Assert.IsTrue(line.IsHorizontal);
        }

        [Test]
        public void IsHorizontalNegative()
        {
            Point2D a = new Point2D(0.0, 0.0);
            Point2D b = new Point2D(1.0, 1.0);
            Line2D line = new Line2D(a, b);
            Assert.IsFalse(line.IsHorizontal);
        }

        [Test]
        public void OrientedSideNegative1()
        {
            Point2D a = new Point2D(0.0, 1.0);
            Point2D b = new Point2D(1.0, 2.0);
            Line2D line = new Line2D(a, b);
            Point2D q = new Point2D(0.0, 0.0); // To the right
            Assert.AreEqual(OrientedSide.Negative, line.Side(q));
        }

        [Test]
        public void OrientedSidePositive1()
        {
            Point2D a = new Point2D(0.0, 1.0);
            Point2D b = new Point2D(1.0, 2.0);
            Line2D line = new Line2D(b, a);
            Point2D q = new Point2D(0.0, 0.0); // To the left
            Assert.AreEqual(OrientedSide.Positive, line.Side(q));
        }

        [Test]
        public void OrientedSidePositive2()
        {
            Point2D a = new Point2D(1.0, 0.0);
            Point2D b = new Point2D(2.0, 1.0);
            Line2D line = new Line2D(a, b);
            Point2D q = new Point2D(0.0, 0.0); // To the left
            Assert.AreEqual(OrientedSide.Positive, line.Side(q));
        }

        [Test]
        public void OrientedSideNegative2()
        {
            Point2D a = new Point2D(1.0, 0.0);
            Point2D b = new Point2D(2.0, 1.0);
            Line2D line = new Line2D(b, a);
            Point2D q = new Point2D(0.0, 0.0); // To the right
            Assert.AreEqual(OrientedSide.Negative, line.Side(q));
        }
    }
}
