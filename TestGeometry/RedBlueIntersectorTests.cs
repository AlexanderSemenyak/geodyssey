using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Numeric;

namespace Geometry
{
    using NUnit.Framework;
    using RedBlue;

    [TestFixture]
    public class RedBlueIntersectorTests
    {
        [Test]
        public void TwoNonIntersectingSegments()
        {
            Segment2D a = new Segment2D(new Point2D(0.0, 0.0), new Point2D(0.0, 1.0));
            Segment2D[] red = { a };
            Segment2D b = new Segment2D(new Point2D(1.0, 0.0), new Point2D(1.0, 1.0));
            Segment2D[] blue = { b };
            var rbi = new RedBlueIntersector(red, blue);
            Assert.AreEqual(0, rbi.IntersectingSegments(a).Count());
            Assert.AreEqual(0, rbi.IntersectingSegments(b).Count());
        }

        [Test]
        public void TwoIntersectingSegments()
        {
            Segment2D a = new Segment2D(new Point2D(0.0, 0.0), new Point2D(1.0, 1.0));
            Segment2D[] red  = { a };
            Segment2D b = new Segment2D(new Point2D(0.0, 1.0), new Point2D(1.0, 0.0));
            Segment2D[] blue = { b };
            var rbi = new RedBlueIntersector(red, blue);
            Assert.AreEqual(1, rbi.IntersectingSegments(a).Count());
            Assert.AreEqual(1, rbi.IntersectingSegments(b).Count());
            Assert.AreEqual(b, rbi.IntersectingSegments(a).First());
            Assert.AreEqual(a, rbi.IntersectingSegments(b).First());
        }

        [Test]
        public void TwoEndToEndSegments()
        {
            Segment2D a = new Segment2D(new Point2D(0.0, 0.0), new Point2D(1.0, 1.0));
            Segment2D[] red = { a };
            Segment2D b = new Segment2D(new Point2D(1.0, 1.0), new Point2D(2.0, 0.0));
            Segment2D[] blue = { b };
            var rbi = new RedBlueIntersector(red, blue);
            Assert.AreEqual(1, rbi.IntersectingSegments(a).Count());
            Assert.AreEqual(1, rbi.IntersectingSegments(b).Count());
            Assert.AreEqual(b, rbi.IntersectingSegments(a).First());
            Assert.AreEqual(a, rbi.IntersectingSegments(b).First());
        }
        
        [Test]
        public void ThreeNonIntersectingSegments()
        {
            Segment2D a = new Segment2D(new Point2D(0.0, 0.0), new Point2D(0.5, 0.0));
            Segment2D b = new Segment2D(new Point2D(0.5, 1.0), new Point2D(1.0, 1.0));
            Segment2D[] red = { a,
                                b};
            Segment2D c = new Segment2D(new Point2D(0.25, 0.5), new Point2D(0.75, 0.5));
            Segment2D[] blue = { c };

            var rbi = new RedBlueIntersector(red, blue);
            Assert.AreEqual(0, rbi.IntersectingSegments(a).Count());
            Assert.AreEqual(0, rbi.IntersectingSegments(b).Count());
            Assert.AreEqual(0, rbi.IntersectingSegments(c).Count());
        }

        [Test]
        public void IntersectingRedNoPurple()
        {
            Segment2D a = new Segment2D(new Point2D(0.0, 0.5), new Point2D(1.0, 0.5));
            Segment2D b = new Segment2D(new Point2D(0.5, 0.0), new Point2D(0.5, 1.0));
            Segment2D[] red = { a,
                                b };
            Segment2D c = new Segment2D(new Point2D(0.0, 0.0), new Point2D(0.25, 0.25));
            Segment2D[] blue = { c };

            var rbi = new RedBlueIntersector(red, blue);

            Assert.AreEqual(0, rbi.IntersectingSegments(a).Count());
            Assert.AreEqual(0, rbi.IntersectingSegments(b).Count());
            Assert.AreEqual(0, rbi.IntersectingSegments(c).Count());
        }

        [Test]
        public void IntersectingBlueNoPurple()
        {
            Segment2D a = new Segment2D(new Point2D(0.0, 0.0), new Point2D(0.25, 0.25));
            Segment2D[] red = { a };
            Segment2D b = new Segment2D(new Point2D(0.0, 0.5), new Point2D(1.0, 0.5));
            Segment2D c = new Segment2D(new Point2D(0.5, 0.0), new Point2D(0.5, 1.0));
            Segment2D[] blue = { b,
                                c };

            var rbi = new RedBlueIntersector(red, blue);
            Assert.AreEqual(0, rbi.IntersectingSegments(a).Count());
            Assert.AreEqual(0, rbi.IntersectingSegments(b).Count());
            Assert.AreEqual(0, rbi.IntersectingSegments(c).Count());
        }

        [Test]
        public void CollinearRedBlue()
        {
            Segment2D[] red = { new Segment2D(new Point2D(0.0, 0.0), new Point2D(0.75, 0.0)) };
            Segment2D[] blue = { new Segment2D(new Point2D(0.25, 0.0), new Point2D(1.0, 0.0)) };

            var rbi = new RedBlueIntersector(red, blue);
        }

        [Test]
        public void IntersectingSquare()
        {
            Segment2D a = new Segment2D(new Point2D(0.3, 0.3), new Point2D(1.0, 0.3));
            Segment2D b = new Segment2D(new Point2D(0.3, 0.3), new Point2D(0.3, 1.0));
            Segment2D[] red = { a, b };
            Segment2D c = new Segment2D(new Point2D(0.0, 0.7), new Point2D(0.7, 0.7));
            Segment2D d = new Segment2D(new Point2D(0.7, 0.0), new Point2D(0.7, 0.7));
            Segment2D[] blue = { c, d };

            var rbi = new RedBlueIntersector(red, blue);
            Assert.AreEqual(4, rbi.AllIntersectingSegments.Count());
            foreach (Segment2D segment in rbi.AllIntersectingSegments)
            {
                Assert.IsTrue(blue.Contains(segment) || red.Contains(segment));
            }
            Assert.AreEqual(2, rbi.AllRedIntersectingSegments.Count());
            foreach (Segment2D segment in rbi.AllRedIntersectingSegments)
            {
                Assert.IsTrue(red.Contains(segment));
            }
            Assert.AreEqual(2, rbi.AllBlueIntersectingSegments.Count());
            foreach (Segment2D segment in rbi.AllBlueIntersectingSegments)
            {
                Assert.IsTrue(blue.Contains(segment));
            }
        }

        [Test]
        public void IntersectingZigZag()
        {
            Segment2D a = new Segment2D(new Point2D(0.0, 0.3), new Point2D(0.3, 0.3));
            Segment2D b = new Segment2D(new Point2D(0.3, 0.3), new Point2D(0.5, 0.7));
            Segment2D c = new Segment2D(new Point2D(0.5, 0.7), new Point2D(0.7, 0.3));
            Segment2D d = new Segment2D(new Point2D(0.7, 0.3), new Point2D(1.0, 0.3));
            Segment2D[] red = { a, b, c, d };
            Segment2D e = new Segment2D(new Point2D(0.2, 0.7), new Point2D(0.3, 0.7));
            Segment2D f = new Segment2D(new Point2D(0.3, 0.7), new Point2D(0.5, 0.3));
            Segment2D g = new Segment2D(new Point2D(0.5, 0.3), new Point2D(0.8, 0.7));
            Segment2D h = new Segment2D(new Point2D(0.8, 0.7), new Point2D(0.9, 0.7));
            Segment2D[] blue = { e, f, g, h };

            var rbi = new RedBlueIntersector(red, blue);
            Assert.AreEqual(4, rbi.AllIntersectingSegments.Count());
            foreach (Segment2D segment in rbi.AllIntersectingSegments)
            {
                Assert.IsTrue(blue.Contains(segment) || red.Contains(segment));
            }
            Assert.AreEqual(2, rbi.AllRedIntersectingSegments.Count());
            foreach (Segment2D segment in rbi.AllRedIntersectingSegments)
            {
                Assert.IsTrue(red.Contains(segment));
            }
            Assert.AreEqual(2, rbi.AllBlueIntersectingSegments.Count());
            foreach (Segment2D segment in rbi.AllBlueIntersectingSegments)
            {
                Assert.IsTrue(blue.Contains(segment));
            }

            Assert.AreEqual(f, rbi.IntersectingSegments(b).First());
            Assert.AreEqual(g, rbi.IntersectingSegments(c).First());
        }

        [Test]
        public void IntersectingGrid10()
        {
            List<Segment2D> red = new List<Segment2D>(11);
            for (int i = 0; i <= 10 ; ++i)
            {
                double x = i / 10.0;
                Segment2D redSeg = new Segment2D(new Point2D(x, 0.0), new Point2D(x, 1.0));
                red.Add(redSeg);
            }

            List<Segment2D> blue = new List<Segment2D>(11);
            for (int j = 0; j <= 10 ; ++j)
            {
                double y = j / 10.0;
                Segment2D blueSeg = new Segment2D(new Point2D(0.0, y), new Point2D(1.0, y));
                blue.Add(blueSeg);
            }

            var rbi = new RedBlueIntersector(red, blue);

            Assert.AreEqual(22, rbi.AllIntersectingSegments.Count());
            foreach (Segment2D segment in rbi.AllIntersectingSegments)
            {
                Assert.IsTrue(blue.Contains(segment) || red.Contains(segment));
            }
            Assert.AreEqual(11, rbi.AllRedIntersectingSegments.Count());
            foreach (Segment2D segment in rbi.AllRedIntersectingSegments)
            {
                Assert.IsTrue(red.Contains(segment));
            }
            Assert.AreEqual(11, rbi.AllBlueIntersectingSegments.Count());
            foreach (Segment2D segment in rbi.AllBlueIntersectingSegments)
            {
                Assert.IsTrue(blue.Contains(segment));
            }
        }

        [Test]
        public void IntersectingGrid100()
        {
            List<Segment2D> red = new List<Segment2D>(101);
            for (int i = 0; i <= 100; ++i)
            {
                double x = i / 100.0;
                Segment2D redSeg = new Segment2D(new Point2D(x, 0.0), new Point2D(x, 1.0));
                red.Add(redSeg);
            }

            List<Segment2D> blue = new List<Segment2D>(101);
            for (int j = 0; j <= 100; ++j)
            {
                double y = j / 100.0;
                Segment2D blueSeg = new Segment2D(new Point2D(0.0, y), new Point2D(1.0, y));
                blue.Add(blueSeg);
            }

            var rbi = new RedBlueIntersector(red, blue);

            Assert.AreEqual(202, rbi.AllIntersectingSegments.Count());
            foreach (Segment2D segment in rbi.AllIntersectingSegments)
            {
                Assert.IsTrue(blue.Contains(segment) || red.Contains(segment));
            }
            Assert.AreEqual(101, rbi.AllRedIntersectingSegments.Count());
            foreach (Segment2D segment in rbi.AllRedIntersectingSegments)
            {
                Assert.IsTrue(red.Contains(segment));
            }
            Assert.AreEqual(101, rbi.AllBlueIntersectingSegments.Count());
            foreach (Segment2D segment in rbi.AllBlueIntersectingSegments)
            {
                Assert.IsTrue(blue.Contains(segment));
            }
        }

        [Test]
        public void IntersectingBug1()
        {
            Segment2D[] red = { new Segment2D( new Point2D(384450, 7171260),                    new Point2D(384450,           7171380)) };
            
            var one   = new Segment2D( new Point2D(393160.294372512, 7169290.29437251), new Point2D(393103.309524418, 7169266.69047558));
            var two   = new Segment2D( new Point2D(392970,           7169820),          new Point2D(392970,           7169240.58874503));
            var three = new Segment2D( new Point2D(392899.705627487, 7169220),          new Point2D(393967.645019878, 7169662.35498012)); 
        
            Segment2D[] blue = {one, two, three};

            Point2D? result = Intersector.Intersect(two, three);

            var rbi = new RedBlueIntersector(red, blue);
        }
    }
}
