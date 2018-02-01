using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Geometry.Triangulation;
using Numeric;


namespace Geometry
{
    using NUnit.Framework;

    [TestFixture]
    public class DelaunayTriangulatorTests
    {
        

        public IEnumerable<Point2D> RandomPoints
        {
            get
            {
                Random rnd = new Random(0);
                while (true)
                {
                    double x = rnd.NextDouble();
                    double y = rnd.NextDouble();
                    Point2D point = new Point2D(x, y);
                    yield return point;
                }
            }
        }


        [Test]
        public void Test10000()
        {
            var t = DelaunayTriangulator.Triangulate(RandomPoints.Take(10000));
        }

        [Test]
        public void Test1000()
        {
            var t = DelaunayTriangulator.Triangulate(RandomPoints.Take(1000));
        }

        //[Test]
        //public void Test2()
        //{
        //}

        //[Test]
        //public void Test3()
        //{
        //}

        //[Test]
        //public void Test4()
        //{
        //}

        //[Test]
        //public void Test5()
        //{
        //}
    }
}
