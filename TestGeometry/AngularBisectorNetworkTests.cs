using System;
using System.Collections.Generic;
using System.Text;

using Numeric;
using Geometry;
using Geometry.HalfEdge;

namespace Geometry
{
    using NUnit.Framework;

    [TestFixture]
    public class AngularBisectorNetworkTests
    {
        const double epsilon = 0.000001;

        [Test]
        public void ConstructFromTwoPointsStraight()
        {
            Point2D a = new Point2D(0.0, 0.0);
            Point2D b = new Point2D(1.0, 0.0);
            var mesh = AngularBisectorNetwork.CreateFromPolyline(new Point2D[] { a, b });

            Assert.AreEqual(2, mesh.VertexCount);
            Assert.AreEqual(0, mesh.EdgeCount);

            var aFirst = mesh.Find(new AngularBisectorNetwork.BisectorVertex(a));
            Assert.AreEqual(a, aFirst.Position);
            Assert.AreEqual(new Direction2D(0.0, -1.0), aFirst.Direction);

            var bFirst = mesh.Find(new AngularBisectorNetwork.BisectorVertex(b));
            Assert.AreEqual(b, bFirst.Position);
            Assert.AreEqual(new Direction2D(0.0, -1.0), bFirst.Direction);
        }

        [Test]
        public void ConstructFromThreePointsStraight()
        {
            Point2D a = new Point2D(0.0, 0.0);
            Point2D b = new Point2D(1.0, 0.0);
            Point2D c = new Point2D(2.0, 0.0);
            var mesh = AngularBisectorNetwork.CreateFromPolyline(new Point2D[] { a, b, c });

            Assert.AreEqual(3, mesh.VertexCount);
            Assert.AreEqual(0, mesh.EdgeCount);

            var aFirst = mesh.Find(new AngularBisectorNetwork.BisectorVertex(a));
            Assert.AreEqual(a, aFirst.Position);
            Assert.AreEqual(new Direction2D(0.0, -1.0), aFirst.Direction);

            var bFirst = mesh.Find(new AngularBisectorNetwork.BisectorVertex(b));
            Assert.AreEqual(b, bFirst.Position);
            Assert.AreEqual(new Direction2D(0.0, -1.0), bFirst.Direction);

            var cFirst = mesh.Find(new AngularBisectorNetwork.BisectorVertex(c));
            Assert.AreEqual(c, cFirst.Position);
            Assert.AreEqual(new Direction2D(0.0, -1.0), cFirst.Direction);
        }

        [Test]
        public void ConstructFromThreePoints()
        {
            Point2D a = new Point2D(0.0, 0.0);
            Point2D b = new Point2D(1.0, 0.0);
            Point2D c = new Point2D(2.0, -1.0);

            var mesh = AngularBisectorNetwork.CreateFromPolyline(new Point2D[] { a, b, c });

            Assert.AreEqual(5, mesh.VertexCount);
            Assert.AreEqual(4, mesh.EdgeCount);

            var aNode = mesh.Find(new AngularBisectorNetwork.BisectorVertex(a));
            Assert.AreEqual(a, aNode.Position);

            var bNode = mesh.Find(new AngularBisectorNetwork.BisectorVertex(b));
            Assert.AreEqual(b, bNode.Position);

            var cNode = mesh.Find(new AngularBisectorNetwork.BisectorVertex(c));
            Assert.AreEqual(c, cNode.Position);

            Assert.AreEqual(1, aNode.Degree);
            Assert.AreEqual(1, bNode.Degree);
            Assert.AreEqual(1, cNode.Degree);

            var aEnumerator = aNode.Edges.GetEnumerator();
            aEnumerator.MoveNext();
            EdgeBase apEdge = aEnumerator.Current;

            var bEnumerator = bNode.Edges.GetEnumerator();
            bEnumerator.MoveNext();
            EdgeBase bpEdge = bEnumerator.Current;

            Assert.AreSame(apEdge.Target, bpEdge.Target);

            var pNode = apEdge.Target as AngularBisectorNetwork.BisectorVertex;

            Assert.AreEqual(3, pNode.Degree);
            Assert.AreEqual(0.0, pNode.Position.X, epsilon);
            Assert.AreEqual(-Math.Sqrt(2.0) - 1.0, pNode.Position.Y, epsilon);

            var cEnumerator = cNode.Edges.GetEnumerator();
            cEnumerator.MoveNext();
            EdgeBase cqEdge = cEnumerator.Current;

            var qNode = cqEdge.Target as AngularBisectorNetwork.BisectorVertex;
            Assert.IsNotNull(qNode);
            Assert.AreEqual(2, qNode.Degree);

            EdgeBase pqEdge = mesh.Find(pNode, qNode);
            Assert.IsNotNull(pqEdge);

            Assert.IsNotNull(qNode.Direction);
            Assert.AreEqual(-1.9634954084936209, qNode.Direction.Value.Bearing, epsilon);
        }

        [Test]
        public void ConstructFromThreePoints2()
        {
            Point2D a = new Point2D(200.0, 200.0);
            Point2D b = new Point2D(250.0, 160.0);
            Point2D c = new Point2D(300.0, 200.0);

            var mesh = AngularBisectorNetwork.CreateFromPolyline(new Point2D[] { a, b, c });
        }

        [Test]
        public void ConstructFromFourPoints1()
        {
            Point2D a = new Point2D(0.0, 0.0);
            Point2D b = new Point2D(1.0, 1.0);
            Point2D c = new Point2D(3.0, 1.0);
            Point2D d = new Point2D(4.0, 0.0);

            var mesh = AngularBisectorNetwork.CreateFromPolyline(new Point2D[] { a, b, c, d });

            Assert.AreEqual(6, mesh.VertexCount);
            Assert.AreEqual(5, mesh.EdgeCount);

            var aNode = mesh.Find(new AngularBisectorNetwork.BisectorVertex(a));
            Assert.AreEqual(a, aNode.Position);
            Assert.IsNull(aNode.Direction);

            var bNode = mesh.Find(new AngularBisectorNetwork.BisectorVertex(b));
            Assert.AreEqual(b, bNode.Position);
            Assert.IsNull(bNode.Direction);

            var cNode = mesh.Find(new AngularBisectorNetwork.BisectorVertex(c));
            Assert.AreEqual(c, cNode.Position);
            Assert.IsNull(cNode.Direction);

            var dNode = mesh.Find(new AngularBisectorNetwork.BisectorVertex(d));
            Assert.AreEqual(d, dNode.Position);
            Assert.IsNull(dNode.Direction);

            Assert.AreEqual(1, aNode.Degree);
            Assert.AreEqual(1, bNode.Degree);
            Assert.AreEqual(1, cNode.Degree);
            Assert.AreEqual(1, dNode.Degree);

            var bEnumerator = bNode.Edges.GetEnumerator();
            bEnumerator.MoveNext();
            EdgeBase bpEdge = bEnumerator.Current;

            var cEnumerator = cNode.Edges.GetEnumerator();
            cEnumerator.MoveNext();
            EdgeBase cpEdge = cEnumerator.Current;

            Assert.AreSame(bpEdge.Target, cpEdge.Target);
            var pNode = bpEdge.Target as AngularBisectorNetwork.BisectorVertex;
            Assert.AreEqual(2.0, pNode.Position.X);
            Assert.AreEqual(-Math.Sqrt(2.0), pNode.Position.Y, epsilon);
            Assert.IsNull(pNode.Direction);

            var aEnumerator = aNode.Edges.GetEnumerator();
            aEnumerator.MoveNext();
            EdgeBase aqEdge = aEnumerator.Current;

            var dEnumerator = dNode.Edges.GetEnumerator();
            dEnumerator.MoveNext();
            EdgeBase dqEdge = dEnumerator.Current;

            Assert.AreSame(aqEdge.Target, dqEdge.Target);
            var qNode = aqEdge.Target as AngularBisectorNetwork.BisectorVertex;
            Assert.AreEqual(2.0, qNode.Position.X);
            Assert.AreEqual(-2.0, qNode.Position.Y, epsilon);

            EdgeBase pqEdge = mesh.Find(pNode, qNode);
            Assert.IsNotNull(pqEdge);

            Assert.IsNotNull(qNode.Direction);
            Assert.AreEqual(new Direction2D(0.0, -1.0), qNode.Direction);
        }

        [Test]
        public void ConstructFromFourPoints2()
        {
            Point2D a = new Point2D(200.0, 200.0);
            Point2D b = new Point2D(250.0, 160.0);
            Point2D c = new Point2D(300.0, 200.0);
            Point2D d = new Point2D(400.0, 200.0);

            var mesh = AngularBisectorNetwork.CreateFromPolyline(new Point2D[] { a, b, c, d });
        }

        [Test]
        public void ConstructFromFourPoints3()
        {
            Point2D a = new Point2D(200.0, 200.0);
            Point2D b = new Point2D(348.0, 88.0);
            Point2D c = new Point2D(357.0, 277.0);
            Point2D d = new Point2D(477.0, 181.0);

            var mesh = AngularBisectorNetwork.CreateFromPolyline(new Point2D[] { a, b, c, d });
        }

        [Test]
        public void ConstructFromFourPoints4()
        {
            Point2D a = new Point2D(200.0, 200.0);
            Point2D b = new Point2D(246.0, 242.0);
            Point2D c = new Point2D(300.0, 201.0);
            Point2D d = new Point2D(400.0, 200.0);

            var mesh = AngularBisectorNetwork.CreateFromPolyline(new Point2D[] { a, b, c, d });
        }

        [Test]
        public void ConstructFromFourPoints5()
        {
            Point2D a = new Point2D(200.0, 100.0);
            Point2D b = new Point2D(100.0, 200.0);
            Point2D c = new Point2D(400.0, 200.0);
            Point2D d = new Point2D(300.0, 100.0);

            var mesh = AngularBisectorNetwork.CreateFromPolyline(new Point2D[] { a, b, c, d });
        }

        [Test]
        public void ConstructFromFivePointsReflex()
        {
            Point2D a = new Point2D(0.0, 0.0);
            Point2D b = new Point2D(1.0, 0.0);
            Point2D c = new Point2D(2.0, -1.0);
            Point2D d = new Point2D(3.0, 0.0);
            Point2D e = new Point2D(4.0, 0.0);

            var mesh = AngularBisectorNetwork.CreateFromPolyline(new Point2D[] { a, b, c, d, e });

            Assert.AreEqual(7, mesh.VertexCount);
            Assert.AreEqual(4, mesh.EdgeCount);

            var aNode = mesh.Find(new AngularBisectorNetwork.BisectorVertex(a));
            Assert.AreEqual(a, aNode.Position);
            Assert.IsNull(aNode.Direction);

            var bNode = mesh.Find(new AngularBisectorNetwork.BisectorVertex(b));
            Assert.AreEqual(b, bNode.Position);
            Assert.IsNull(bNode.Direction);

            var cNode = mesh.Find(new AngularBisectorNetwork.BisectorVertex(c));
            Assert.AreEqual(c, cNode.Position);
            Assert.IsNotNull(cNode.Direction);
            Assert.AreEqual(new Direction2D(0.0, -1.0), cNode.Direction);

            var dNode = mesh.Find(new AngularBisectorNetwork.BisectorVertex(d));
            Assert.AreEqual(d, dNode.Position);
            Assert.IsNull(dNode.Direction);

            var eNode = mesh.Find(new AngularBisectorNetwork.BisectorVertex(e));
            Assert.AreEqual(e, eNode.Position);
            Assert.IsNull(eNode.Direction);

            Assert.AreEqual(1, aNode.Degree);
            Assert.AreEqual(1, bNode.Degree);
            Assert.AreEqual(0, cNode.Degree);
            Assert.AreEqual(1, dNode.Degree);
            Assert.AreEqual(1, eNode.Degree);

            var aEnumerator = aNode.Edges.GetEnumerator();
            aEnumerator.MoveNext();
            EdgeBase apEdge = aEnumerator.Current;

            var bEnumerator = bNode.Edges.GetEnumerator();
            bEnumerator.MoveNext();
            EdgeBase bpEdge = bEnumerator.Current;

            Assert.AreSame(apEdge.Target, bpEdge.Target);
            var pNode = apEdge.Target as AngularBisectorNetwork.BisectorVertex;
            Assert.AreEqual(0.0, pNode.Position.X);
            Assert.AreEqual(-1.0 -Math.Sqrt(2.0), pNode.Position.Y, epsilon);
            Assert.IsNotNull(pNode.Direction);
            Assert.AreEqual(2, pNode.Degree);
            Assert.AreEqual(-1.9634954084936209, pNode.Direction.Value.Bearing, epsilon);

            var dEnumerator = dNode.Edges.GetEnumerator();
            dEnumerator.MoveNext();
            EdgeBase dqEdge = dEnumerator.Current;

            var eEnumerator = eNode.Edges.GetEnumerator();
            eEnumerator.MoveNext();
            EdgeBase eqEdge = eEnumerator.Current;

            Assert.AreSame(dqEdge.Target, eqEdge.Target);
            var qNode = dqEdge.Target as AngularBisectorNetwork.BisectorVertex;
            Assert.AreEqual(4.0, qNode.Position.X);
            Assert.AreEqual(-1.0 - Math.Sqrt(2.0), qNode.Position.Y, epsilon);
            Assert.IsNotNull(qNode.Direction);
            Assert.AreEqual(2, qNode.Degree);
            Assert.AreEqual(-1.1780972450961724, qNode.Direction.Value.Bearing, epsilon);
        }

        [Test]
        public void ConstructFromSevenPointsReflex1()
        {
            var mesh = AngularBisectorNetwork.CreateFromPolyline(new Point2D[] { new Point2D(2.0, 2.0),
            new Point2D(2.5, 1.6),
            new Point2D(3.0, 2.0),
            new Point2D(4.0, 2.0),
            new Point2D(3.0, 1.0),
            new Point2D(2.0, 1.0),
            new Point2D(1.0, 2.0), });
        }

        [Test]
        public void ConstructFromSevenPointsReflex2()
        {
            var mesh = AngularBisectorNetwork.CreateFromPolyline(new Point2D[] { new Point2D(200, 200),
            new Point2D(250, 160),
            new Point2D(300, 200),
            new Point2D(400, 200),
            new Point2D(300, 100),
            new Point2D(200, 100),
            new Point2D(100, 200), });
        }

        [Test]
        public void ConstructFromSevenPointsReflex3()
        {
            var mesh = AngularBisectorNetwork.CreateFromPolyline(new Point2D[] { new Point2D(100.0, 100.0),
            new Point2D(200.0, 200.0),
            new Point2D(200.0, 400.0),
            new Point2D(300.0, 500.0),
            new Point2D(400.0, 500.0),
            new Point2D(500.0, 300.0),
            new Point2D(550.0, 350.0) });
        }

        [Test]
        public void ConstructFromSevenPointsReflex4()
        {
            var mesh = AngularBisectorNetwork.CreateFromPolyline(
                new Point2D[] { new Point2D(200.0, 200.0),
                                new Point2D(250.0, 160.0),
                                new Point2D(354.0, 334.0),
                                new Point2D(400.0, 200.0),
                                new Point2D(300.0, 100.0),
                                new Point2D(200.0, 100.0),
                                new Point2D(100.0, 200.0) });

            Assert.AreEqual(12, mesh.EdgeCount);
        }

        [Test]
        public void ConstructFromSevenPointsReflex5()
        {
            var mesh = AngularBisectorNetwork.CreateFromPolyline(
                new Point2D[] { new Point2D(200.0, 200.0),
                                new Point2D(250.0, 160.0),
                                new Point2D(300.0, 200.0),
                                new Point2D(400.0, 200.0),
                                new Point2D(300.0, 100.0),
                                new Point2D(200.0, 100.0),
                                new Point2D(100.0, 200.0) });

            Assert.AreEqual(12, mesh.EdgeCount);
        }

        [Test]
        public void ConstructFromSevenPointsReflex6()
        {
            var mesh = AngularBisectorNetwork.CreateFromPolyline(
                new Point2D[] { new Point2D(160.0, 335.0),
                                new Point2D(213.0, 333.0),
                                new Point2D(385.0, 426.0),
                                new Point2D(488.0, 77.0),
                                new Point2D(273.0, 69.0),
                                new Point2D(263.0, 212.0),
                                new Point2D(100.0, 200.0) });

            Assert.AreEqual(12, mesh.EdgeCount);
        }

        [Test]
        public void ConstructFromSevenPointsReflex7()
        {
            var mesh = AngularBisectorNetwork.CreateFromPolyline(
                new Point2D[] { new Point2D(79, 444),
                                new Point2D(188, 434),
                                new Point2D(418, 465),
                                new Point2D(681, 541),
                                new Point2D(880, 276),
                                new Point2D(443, 151),
                                new Point2D(76, 174) });

            Assert.AreEqual(12, mesh.EdgeCount);
        }

        [Test]
        public void ConstructFromSevenPoints()
        {
            var mesh = AngularBisectorNetwork.CreateFromPolyline(
                new Point2D[] { new Point2D(362, 113),
                                new Point2D(204, 158),
                                new Point2D(235, 256),
                                new Point2D(275, 353),
                                new Point2D(412, 355),
                                new Point2D(459, 181),
                                new Point2D(418, 184) });

            Assert.AreEqual(12, mesh.EdgeCount);
        }

        private static EdgeBase FindEdge(Mesh<AngularBisectorNetwork.BisectorVertex, EdgeBase, FaceBase> mesh, Point2D a, Point2D b)
        {
            return mesh.Find(new AngularBisectorNetwork.BisectorVertex(a),
                             new AngularBisectorNetwork.BisectorVertex(b));
        }
    }
}
