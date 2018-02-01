using System;
using System.Collections.Generic;
using System.Text;

using Numeric;
using Image;

namespace FaultMapper
{

    using NUnit.Framework;

    [TestFixture]
    public class CellularRayTracerTests
    {
        IImage<bool> image;
        CellularRayTracer tracer;

        [SetUp]
        protected void Setup()
        {
            bool[,] pattern = new bool[,] { {false, false, false, false, false, false, false},
                                            {false, true,  true,  true,  true,  true,  false},
                                            {false, true,  false, false, false, true,  false},
                                            {false, true,  false, true,  false, true,  false},
                                            {false, true,  false, false, false, true,  false},
                                            {false, true,  true,  true,  true,  true,  false},
                                            {false, false, false, false, false, false, false} };
            image = new FastImage<bool>(pattern);
            tracer = new CellularRayTracer(image);
        }

        [Test]
        public void TraceNorth()
        {
            Ray2D ray = new Ray2D(new Point2D(3.5, 3.5), new Direction2D(0.0, 1.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual( new Point2D(3, 5), result);
        }

        [Test]
        public void TraceNorthNorthEast()
        {
            Ray2D ray = new Ray2D(new Point2D(3.5, 3.5), new Direction2D(2.0, 3.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(4, 5), result);
        }

        [Test]
        public void TraceNorthEast()
        {
            Ray2D ray = new Ray2D(new Point2D(3.5, 3.5), new Direction2D(1.0, 1.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual( new Point2D(5, 4), result);
        }

        [Test]
        public void TraceEastNorthEast()
        {
            Ray2D ray = new Ray2D(new Point2D(3.5, 3.5), new Direction2D(3.0, 2.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(5, 4), result);
        }

        [Test]
        public void TraceEast()
        {
            Ray2D ray = new Ray2D(new Point2D(3.5, 3.5), new Direction2D(1.0, 0.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(5, 3), result);
        }

        [Test]
        public void TraceEastSouthEast()
        {
            Ray2D ray = new Ray2D(new Point2D(3.5, 3.5), new Direction2D(3.0, -2.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(5, 2), result);
        }

        [Test]
        public void TraceSouthEast()
        {
            Ray2D ray = new Ray2D(new Point2D(3.5, 3.5), new Direction2D(1.0, -1.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(5, 2), result);
        }

        [Test]
        public void TraceSouthSouthEast()
        {
            Ray2D ray = new Ray2D(new Point2D(3.5, 3.5), new Direction2D(2.0, -3.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(4, 1), result);
        }

        [Test]
        public void TraceSouth()
        {
            Ray2D ray = new Ray2D(new Point2D(3.5, 3.5), new Direction2D(0.0, -1.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(3, 1), result);
        }

        [Test]
        public void TraceSouthSouthWest()
        {
            Ray2D ray = new Ray2D(new Point2D(3.5, 3.5), new Direction2D(-2.0, -3.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(2, 1), result);
        }

        [Test]
        public void TraceSouthWest()
        {
            Ray2D ray = new Ray2D(new Point2D(3.5, 3.5), new Direction2D(-1.0, -1.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(1, 2), result);
        }

        [Test]
        public void TraceWestSouthWest()
        {
            Ray2D ray = new Ray2D(new Point2D(3.5, 3.5), new Direction2D(-3.0, -2.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(1, 2), result);
        }

        [Test]
        public void TraceWest()
        {
            Ray2D ray = new Ray2D(new Point2D(3.5, 3.5), new Direction2D(-1.0, 0.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(1, 3), result);
        }

        [Test]
        public void TraceWestNorthWest()
        {
            Ray2D ray = new Ray2D(new Point2D(3.5, 3.5), new Direction2D(-3.0, 2.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(1, 4), result);
        }

        [Test]
        public void TraceNorthWest()
        {
            Ray2D ray = new Ray2D(new Point2D(3.5, 3.5), new Direction2D(-1.0, 1.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(1, 4), result);
        }

        [Test]
        public void TraceNorthNorthWest()
        {
            Ray2D ray = new Ray2D(new Point2D(3.5, 3.5), new Direction2D(-2.0, 3.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(2, 5), result);
        }

        [Test]
        public void TraceXDominatedPositive()
        {
            Ray2D ray = new Ray2D(new Point2D(0.5, 0.5), new Direction2D(6.0, 1.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(3, 1), result);
        }

        [Test]
        public void TraceYDominatedPositive()
        {
            Ray2D ray = new Ray2D(new Point2D(0.5, 0.5), new Direction2D(1.0, 6.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(1, 3), result);
        }

        [Test]
        public void TraceXDominatedNegative()
        {
            Ray2D ray = new Ray2D(new Point2D(6.5, 6.5), new Direction2D(-6.0, -1.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(3, 5), result);
        }

        [Test]
        public void TraceYDominatedNegative()
        {
            Ray2D ray = new Ray2D(new Point2D(6.5, 6.5), new Direction2D(-1.0, -6.0));
            Point2D result = tracer.Trace(ray);
            Assert.AreEqual(new Point2D(5, 3), result);
        }

        [Test]
        public void TraceNoImageChange()
        {
            IImage<bool> copy = (IImage<bool>) image.Clone();
            Ray2D ray = new Ray2D(new Point2D(3.5, 3.5), new Direction2D(1.0, 0.0));
            Point2D result = tracer.Trace(ray);
            Assert.IsTrue(copy.Equals(image));
        }

    }
}
