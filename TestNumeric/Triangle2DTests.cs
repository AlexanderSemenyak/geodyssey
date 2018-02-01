using System;
using System.Collections.Generic;
using System.Text;

namespace Numeric
{
    using NUnit.Framework;

    [TestFixture]
    public class Triangle2DTests
    {
        const double epsilon = 0.000001;

        [Test]
        public void Area()
        {
            Triangle2D triangle = new Triangle2D(new Point2D(0.0, 0.0),
                                                 new Point2D(1.0, 0.0),
                                                 new Point2D(0.0, 1.0));
            Assert.AreEqual(0.5, triangle.Area, epsilon);
        }

        [Test]
        public void SignedArea()
        {
            Triangle2D triangle = new Triangle2D(new Point2D(0.0, 0.0),
                                                 new Point2D(1.0, 0.0),
                                                 new Point2D(0.0, 1.0));
            Assert.AreEqual(0.5, triangle.SignedArea, epsilon);
        }

        [Test]
        public void HandednessCounterclockwise()
        {
            Triangle2D triangle = new Triangle2D(new Point2D(0.0, 0.0),
                                                 new Point2D(1.0, 0.0),
                                                 new Point2D(0.0, 1.0));
            Assert.AreEqual(Sense.Counterclockwise, triangle.Handedness);
        }

        [Test]
        public void HandednessClockwise()
        {
            Triangle2D triangle = new Triangle2D(new Point2D(0.0, 0.0),
                                                 new Point2D(0.0, 1.0),
                                                 new Point2D(1.0, 0.0));
            Assert.AreEqual(Sense.Clockwise, triangle.Handedness);
        }

        [Test]
        public void IsNotDegenerate()
        {
            Triangle2D triangle = new Triangle2D(new Point2D(0.0, 0.0),
                                                 new Point2D(0.0, 1.0),
                                                 new Point2D(1.0, 0.0));
            Assert.IsFalse(triangle.IsDegenerate);
        }

        [Test]
        public void IsDegenerateCoincident()
        {
            Triangle2D triangle = new Triangle2D(new Point2D(0.0, 0.0),
                                                 new Point2D(0.0, 0.0),
                                                 new Point2D(1.0, 0.0));
            Assert.IsTrue(triangle.IsDegenerate);
        }

        [Test]
        public void IsDegenerateCollinear()
        {
            Triangle2D triangle = new Triangle2D(new Point2D(0.0, 0.0),
                                                 new Point2D(1.0, 0.0),
                                                 new Point2D(2.0, 0.0));
            Assert.IsTrue(triangle.IsDegenerate);
        }

        [Test]
        public void AnglesTest345()
        {
            Triangle2D triangle = new Triangle2D(new Point2D(0.0, 0.0),
                                                 new Point2D(3.0, 0.0),
                                                 new Point2D(0.0, 4.0));
            Assert.AreEqual(1.57079633, triangle.AngleA, epsilon); // 90°
            Assert.AreEqual(0.92729521800161219, triangle.AngleB, epsilon); // 53.12°
            Assert.AreEqual(0.64350110879328437, triangle.AngleC, epsilon); // 36.86°
        }
    }
}
