using System;
using System.Collections.Generic;
using System.Text;

using Numeric;

namespace Geometry
{
    using NUnit.Framework;

    [TestFixture]
    public class IntersectorTests
    {
        [Test]
        public void RayToRayParallelIntersect()
        {
            Ray2D ray1 = new Ray2D(new Point2D(0.0, 0.0), new Direction2D(0.0, 1.0));
            Ray2D ray2 = new Ray2D(new Point2D(1.0, 0.0), new Direction2D(0.0, 1.0));

            Point2D result;
            Intersector.Intersection intersection = Intersector.Intersect(ray1, ray2, out result);
            Assert.AreEqual(Intersector.Intersection.DontIntersect, intersection);
        }

        [Test]
        public void RayToRayDivergentOverlapping()
        {
            Ray2D ray1 = new Ray2D(new Point2D(0.0, 0.0), new Direction2D(0.0, 1.0));
            Ray2D ray2 = new Ray2D(new Point2D(1.0, 2.0), new Direction2D(1.0, 1.0));

            Point2D result;
            Intersector.Intersection intersection = Intersector.Intersect(ray1, ray2, out result);
            Assert.AreEqual(Intersector.Intersection.DontIntersect, intersection);
        }

        [Test]
        public void RayToRayDivergentNonOverlapping()
        {
            Ray2D ray1 = new Ray2D(new Point2D(0.0, 0.0), new Direction2D(0.0, 1.0));
            Ray2D ray2 = new Ray2D(new Point2D(1.0, 0.0), new Direction2D(1.0, 1.0));

            Point2D result;
            Intersector.Intersection intersection = Intersector.Intersect(ray1, ray2, out result);
            Assert.AreEqual(Intersector.Intersection.DontIntersect, intersection);
        }

        [Test]
        public void RayToRayCovergentOverlapping()
        {
            Ray2D ray1 = new Ray2D(new Point2D(0.0, 0.0), new Direction2D(0.0, 1.0));
            Ray2D ray2 = new Ray2D(new Point2D(-1.0, 1.0), new Direction2D(1.0, 1.0));

            Point2D result;
            Intersector.Intersection intersection = Intersector.Intersect(ray1, ray2, out result);
            Assert.AreEqual(Intersector.Intersection.DoIntersect, intersection);
            Assert.AreEqual(new Point2D(0.0, 2.0), result);
        }

        [Test]
        public void RayToRayCovergentNonOverlapping()
        {
            Ray2D ray1 = new Ray2D(new Point2D(0.0, 0.0), new Direction2D(0.0, 1.0));
            Ray2D ray2 = new Ray2D(new Point2D(-1.0, -2.0), new Direction2D(1.0, 1.0));

            Point2D result;
            Intersector.Intersection intersection = Intersector.Intersect(ray1, ray2, out result);
            Assert.AreEqual(Intersector.Intersection.DontIntersect, intersection);
        }

        [Test]
        public void RayToRayOrderIndependence1()
        {
            Ray2D rayA = new Ray2D(new Point2D(0.0, 0.0), new Direction2D(0.0, 1.0));
            Ray2D rayB = new Ray2D(new Point2D(-1.0, 1.0), new Direction2D(1.0, 1.0));

            Point2D resultAB;
            Intersector.Intersection intersectionAB = Intersector.Intersect(rayA, rayB, out resultAB);

            Point2D resultBA;
            Intersector.Intersection intersectionBA = Intersector.Intersect(rayB, rayA, out resultBA);

            Assert.AreEqual(intersectionAB, intersectionBA);
            Assert.AreEqual(resultAB, resultBA);
        }

        [Test]
        public void RayToRayOrderIndependence2()
        {
            Ray2D rayA = new Ray2D(new Point2D(0.0, 0.0), new Direction2D(0.0, 1.0));
            Ray2D rayB = new Ray2D(new Point2D(2.0, 1.0), new Direction2D(1.0, 1.0));

            Point2D resultAB;
            Intersector.Intersection intersectionAB = Intersector.Intersect(rayA, rayB, out resultAB);

            Point2D resultBA;
            Intersector.Intersection intersectionBA = Intersector.Intersect(rayB, rayA, out resultBA);

            Assert.AreEqual(intersectionAB, intersectionBA);
            Assert.AreEqual(resultAB, resultBA);
        }

        [Test]
        public void RayToRayGlancing()
        {
            Ray2D rayA = new Ray2D(new Point2D(4.0, 0.0), new Direction2D(-1.0, -1.0));
            Ray2D rayB = new Ray2D(new Point2D(2.0, 2.0), new Direction2D(0.0, -Math.Sqrt(2.0)));

            Point2D result;
            Intersector.Intersection intersection = Intersector.Intersect(rayA, rayB, out result);
            Assert.AreEqual(Intersector.Intersection.DoIntersect, intersection);
            Assert.AreEqual(new Point2D(2.0, -2.0), result);
        }

        [Test]
        public void RayToLineParallel()
        {
            Ray2D ray = new Ray2D(new Point2D(0.0, 0.0), new Direction2D(0.0, 1.0));
            Line2D line = new Line2D(new Point2D(1.0, 0.0), new Point2D(1.0, 1.0));

            Point2D result;
            Intersector.Intersection intersection = Intersector.Intersect(ray, line, out result);
            Assert.AreEqual(Intersector.Intersection.DontIntersect, intersection);
        }

        [Test]
        public void RayToLineDiverging()
        {
            Ray2D ray = new Ray2D(new Point2D(0.0, 0.0), new Direction2D(-1.0, 1.0));
            Line2D line = new Line2D(new Point2D(1.0, 0.0), new Point2D(1.0, 1.0));

            Point2D result;
            Intersector.Intersection intersection = Intersector.Intersect(ray, line, out result);
            Assert.AreEqual(Intersector.Intersection.DontIntersect, intersection);
        }

        [Test]
        public void RayToRayConverging()
        {
            Ray2D ray = new Ray2D(new Point2D(0.0, 0.0), new Direction2D(1.0, 1.0));
            Line2D line = new Line2D(new Point2D(1.0, 0.0), new Point2D(1.0, 1.0));

            Point2D result;
            Intersector.Intersection intersection = Intersector.Intersect(ray, line, out result);
            Assert.AreEqual(Intersector.Intersection.DoIntersect, intersection);
            Assert.AreEqual(new Point2D(1.0, 1.0), result);
        }

        [Test]
        public void SegmentToSegmentIntersecting()
        {
            Segment2D segment1 = new Segment2D(new Point2D(0.0, 0.0), new Point2D(1.0, 1.0));
            Segment2D segment2 = new Segment2D(new Point2D(0.0, 1.0), new Point2D(1.0, 0.0));

            Point2D result;
            Intersector.Intersection intersection = Intersector.Intersect(segment1, segment2, out result);
            Assert.AreEqual(Intersector.Intersection.DoIntersect, intersection);
            Assert.AreEqual(new Point2D(0.5, 0.5), result);
        }

        [Test]
        public void SegmentToSegmentParallel()
        {
            Segment2D segment1 = new Segment2D(new Point2D(0.0, 0.0), new Point2D(1.0, 0.0));
            Segment2D segment2 = new Segment2D(new Point2D(0.0, 1.0), new Point2D(1.0, 1.0));

            Point2D result;
            Intersector.Intersection intersection = Intersector.Intersect(segment1, segment2, out result);
            Assert.AreEqual(Intersector.Intersection.DontIntersect, intersection);
        }
    }
}
