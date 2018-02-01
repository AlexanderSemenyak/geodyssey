using System;
using System.Collections.Generic;
using System.Text;

using Numeric;
using Geometry;

namespace Geometry
{
    using NUnit.Framework;

    [TestFixture]
    public class VectorGeometryTests
    {
        [Test]
        public void AcuteBisector0()
        {
            Vector2D v1 = new Vector2D(0.0, 1.0);
            Vector2D v2 = new Vector2D(0.0, 1.0);
            Direction2D d = VectorGeometry.Bisector(ref v1, ref v2);
            Assert.AreEqual(new Direction2D(1.0, 0.0), d);
        }

        [Test]
        public void AcuteBisector72()
        {
            Vector2D v1 = new Vector2D(-4.0, 3.0);
            Vector2D v2 = new Vector2D(4.0, 3.0);
            Direction2D d = VectorGeometry.Bisector(ref v1, ref v2);
            Assert.AreEqual(new Direction2D(1.0, 0.0), d);
        }

        [Test]
        public void AcuteBisector72Reflex()
        {
            Vector2D v1 = new Vector2D(4.0, 3.0);
            Vector2D v2 = new Vector2D(-4.0, 3.0);
            Direction2D d = VectorGeometry.Bisector(ref v1, ref v2);
            Assert.AreEqual(new Direction2D(1.0, 0.0), d);
        }

        [Test]
        public void AcuteBisector90()
        {
            Vector2D v1 = new Vector2D(0.0, 1.0);
            Vector2D v2 = new Vector2D(1.0, 0.0);
            Direction2D d = VectorGeometry.Bisector(ref v1, ref v2);
            Assert.AreEqual(new Direction2D(1.0, -1.0), d);
        }

        [Test]
        public void AcuteBisector180()
        {
            Vector2D v1 = new Vector2D(1.0, 0.0);
            Vector2D v2 = new Vector2D(-1.0, 0.0);
            Direction2D d = VectorGeometry.Bisector(ref v1, ref v2);
            Assert.AreEqual(new Direction2D(-1.0, 0.0), d);
        }

    }
}
