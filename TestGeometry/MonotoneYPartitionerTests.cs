using System;
using System.Collections.Generic;
using System.Text;

using Numeric;
using Geometry;
using Geometry.HalfEdge;

namespace Geometry.PolygonPartitioning
{
    using NUnit.Framework;

    [TestFixture]
    public class MonotoneYPartitionerTests
    {
        internal class Vertex : VertexBase, IPositionable2D
        {
            private readonly Point2D position;

            public Vertex(double x, double y)
            {
                position = new Point2D(x, y);
            }

            public Point2D Position
            {
                get { return position; }
            }
        }

        [Test]
        public void PolygonAntiClockwiseTest()
        {
            Vertex a = new Vertex(8, 14);
            Vertex b = new Vertex(3, 10);
            Vertex c = new Vertex(3, 3);
            Vertex d = new Vertex(8, 1);
            Vertex e = new Vertex(13, 4);
            Vertex f = new Vertex(13, 11);
            var vertices = new[] {a, b, c, d, e, f};
            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);
            mesh.AddEdge(a, b);
            mesh.AddEdge(b, c);
            mesh.AddEdge(c, d);
            mesh.AddEdge(d, e);
            mesh.AddEdge(e, f);
            mesh.AddEdge(f, a);
            // TODO: Could add face here
            var myp = new MonotoneYPartitioner<Vertex, EdgeBase, FaceBase>(mesh);
            Mesh<Vertex, EdgeBase, FaceBase> result = myp.GetResult();
            Assert.AreEqual(6, result.VertexCount);
            Assert.AreEqual(6, result.EdgeCount);
        }

        [Test]
        public void PolygonClockwiseTest()
        {
            Vertex a = new Vertex(8, 14);
            Vertex b = new Vertex(3, 10);
            Vertex c = new Vertex(3, 3);
            Vertex d = new Vertex(8, 1);
            Vertex e = new Vertex(13, 4);
            Vertex f = new Vertex(13, 11);
            var vertices = new[] { f, e, d, c, b, a };
            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);
            mesh.AddEdge(f, e);
            mesh.AddEdge(e, d);
            mesh.AddEdge(d, c);
            mesh.AddEdge(c, b);
            mesh.AddEdge(b, a);
            mesh.AddEdge(a, f);
            // TODO: Could add face here
            var myp = new MonotoneYPartitioner<Vertex, EdgeBase, FaceBase>(mesh);
            Mesh<Vertex, EdgeBase, FaceBase> result = myp.GetResult();
            Assert.AreEqual(6, result.VertexCount);
            Assert.AreEqual(6, result.EdgeCount);
        }

        [Test]
        public void NonMonotoneClockwisePolygonTest()
        {
            Vertex a = new Vertex(8, 14);
            Vertex b = new Vertex(3, 10);
            Vertex c = new Vertex(3, 6);
            Vertex d = new Vertex(6, 8);
            Vertex e = new Vertex(3, 3);
            Vertex f = new Vertex(8, 1);
            Vertex g = new Vertex(13, 4);
            Vertex h = new Vertex(13, 11);
            var vertices = new[] { h, g, f, e, d, c, b, a };
            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);
            mesh.AddEdge(h, g);
            mesh.AddEdge(g, f);
            mesh.AddEdge(f, e);
            mesh.AddEdge(e, d);
            mesh.AddEdge(d, c);
            mesh.AddEdge(c, b);
            mesh.AddEdge(b, a);
            mesh.AddEdge(a, h);
            // TODO: Could add face here
            var myp = new MonotoneYPartitioner<Vertex, EdgeBase, FaceBase>(mesh);
            Mesh<Vertex, EdgeBase, FaceBase> result = myp.GetResult();
            Assert.AreEqual(8, result.VertexCount);
            Assert.AreEqual(9, result.EdgeCount);
        }

        [Test]
        public void NonMonotoneAntiClockwisePolygonTest()
        {
            Vertex a = new Vertex(8, 14);
            Vertex b = new Vertex(3, 10);
            Vertex c = new Vertex(3, 6);
            Vertex d = new Vertex(6, 8);
            Vertex e = new Vertex(3, 3);
            Vertex f = new Vertex(8, 1);
            Vertex g = new Vertex(13, 4);
            Vertex h = new Vertex(13, 11);
            var vertices = new[] { a, b, c, d, e, f, g, h };
            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);
            mesh.AddEdge(a, b);
            mesh.AddEdge(b, c);
            mesh.AddEdge(c, d);
            mesh.AddEdge(d, e);
            mesh.AddEdge(e, f);
            mesh.AddEdge(f, g);
            mesh.AddEdge(g, h);
            mesh.AddEdge(h, a);
            // TODO: Could add face here
            var myp = new MonotoneYPartitioner<Vertex, EdgeBase, FaceBase>(mesh);
            Mesh<Vertex, EdgeBase, FaceBase> result = myp.GetResult();
            Assert.AreEqual(8, result.VertexCount);
            Assert.AreEqual(9, result.EdgeCount);
        }

        [Test]
        public void BoundingBoxTest()
        {
            Vertex a = new Vertex(0, 0);
            Vertex b = new Vertex(10, 0);
            Vertex c = new Vertex(10, 10);
            Vertex d = new Vertex(0, 10);
            var vertices = new[] { a, b, c, d };
            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);
            mesh.AddEdge(a, b);
            mesh.AddEdge(b, c);
            mesh.AddEdge(c, d);
            mesh.AddEdge(d, a);
            // TODO: Could add face here
            var myp = new MonotoneYPartitioner<Vertex, EdgeBase, FaceBase>(mesh);
            Mesh<Vertex, EdgeBase, FaceBase> result = myp.GetResult();
            Assert.AreEqual(4, result.VertexCount);
            Assert.AreEqual(4, result.EdgeCount);
        }

        [Test]
        public void BoundingBoxWithSegmentTest()
        {
            Vertex a = new Vertex(0, 0);
            Vertex b = new Vertex(10, 0);
            Vertex c = new Vertex(10, 10);
            Vertex d = new Vertex(0, 10);
            Vertex e = new Vertex(5, 2);
            Vertex f = new Vertex(5, 8);
            var vertices = new[] { a, b, c, d, e, f };
            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);
            mesh.AddEdge(a, b);
            mesh.AddEdge(b, c);
            mesh.AddEdge(c, d);
            mesh.AddEdge(d, a);
            mesh.AddEdge(e, f);
            // TODO: Could add face here
            var myp = new MonotoneYPartitioner<Vertex, EdgeBase, FaceBase>(mesh);
            Mesh<Vertex, EdgeBase, FaceBase> result = myp.GetResult();
            Assert.AreEqual(6, result.VertexCount);
            Assert.AreEqual(7, result.EdgeCount);
        }

        [Test]
        public void BoundingBoxWithPolygonTest()
        {
            Vertex a = new Vertex(8, 14);
            Vertex b = new Vertex(3, 10);
            Vertex c = new Vertex(3, 3);
            Vertex d = new Vertex(8, 1);
            Vertex e = new Vertex(13, 4);
            Vertex f = new Vertex(13, 11);
            Vertex bb1 = new Vertex(0, 0);
            Vertex bb2 = new Vertex(15, 0);
            Vertex bb3 = new Vertex(15, 15);
            Vertex bb4 = new Vertex(0, 15);
            var vertices = new[] { f, e, d, c, b, a, bb1, bb2, bb3, bb4 };
            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);
            mesh.AddEdge(f, e);
            mesh.AddEdge(e, d);
            mesh.AddEdge(d, c);
            mesh.AddEdge(c, b);
            mesh.AddEdge(b, a);
            mesh.AddEdge(a, f);

            mesh.AddEdge(bb1, bb2);
            mesh.AddEdge(bb2, bb3);
            mesh.AddEdge(bb3, bb4);
            mesh.AddEdge(bb4, bb1);

            var myp = new MonotoneYPartitioner<Vertex, EdgeBase, FaceBase>(mesh);
            Mesh<Vertex, EdgeBase, FaceBase> result = myp.GetResult();
            Assert.AreEqual(10, result.VertexCount);
            Assert.AreEqual(12, result.EdgeCount);
        }

        [Test]
        public void BoundingBoxWithPlanarSubdivision1Test()
        {
            Vertex a = new Vertex(8, 14);
            Vertex b = new Vertex(3, 10);
            Vertex c = new Vertex(3, 3);
            Vertex d = new Vertex(8, 1);
            Vertex e = new Vertex(13, 4);
            Vertex f = new Vertex(13, 11);
            Vertex a1 = new Vertex(8, 9);
            Vertex bb1 = new Vertex(0, 0);
            Vertex bb2 = new Vertex(15, 0);
            Vertex bb3 = new Vertex(15, 15);
            Vertex bb4 = new Vertex(0, 15);
            var vertices = new[] { f, e, d, c, b, a, a1, bb1, bb2, bb3, bb4 };
            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);

            // Hexagon
            mesh.AddEdge(f, e);
            mesh.AddEdge(e, d);
            mesh.AddEdge(d, c);
            mesh.AddEdge(c, b);
            mesh.AddEdge(b, a);
            mesh.AddEdge(a, f);

            // Spur into hexagon
            mesh.AddEdge(a, a1);

            // Bounding box
            mesh.AddEdge(bb1, bb2);
            mesh.AddEdge(bb2, bb3);
            mesh.AddEdge(bb3, bb4);
            mesh.AddEdge(bb4, bb1);

            var myp = new MonotoneYPartitioner<Vertex, EdgeBase, FaceBase>(mesh);
            Mesh<Vertex, EdgeBase, FaceBase> result = myp.GetResult();
            Assert.AreEqual(11, result.VertexCount);
            Assert.AreEqual(14, result.EdgeCount);
        }

        [Test]
        public void BoundingBoxWithPlanarSubdivision2Test()
        {
            Vertex a = new Vertex(8, 14);
            Vertex b = new Vertex(3, 10);
            Vertex c = new Vertex(3, 3);
            Vertex d = new Vertex(8, 1);
            Vertex e = new Vertex(13, 4);
            Vertex f = new Vertex(13, 11);
            Vertex a1 = new Vertex(8, 9);
            Vertex bb1 = new Vertex(0, 0);
            Vertex bb2 = new Vertex(15, 0);
            Vertex bb3 = new Vertex(15, 15);
            Vertex bb4 = new Vertex(0, 15);
            var vertices = new[] { f, e, d, c, b, a, a1, bb1, bb2, bb3, bb4 };
            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);

            // Hexagon
            mesh.AddEdge(f, e);
            mesh.AddEdge(e, d);
            mesh.AddEdge(d, c);
            mesh.AddEdge(c, b);
            mesh.AddEdge(b, a);
            mesh.AddEdge(a, f);

            // Spur into hexagon
            mesh.AddEdge(f, a1);

            // Bounding box
            mesh.AddEdge(bb1, bb2);
            mesh.AddEdge(bb2, bb3);
            mesh.AddEdge(bb3, bb4);
            mesh.AddEdge(bb4, bb1);

            var myp = new MonotoneYPartitioner<Vertex, EdgeBase, FaceBase>(mesh);
            Mesh<Vertex, EdgeBase, FaceBase> result = myp.GetResult();
            Assert.AreEqual(11, result.VertexCount);
            Assert.AreEqual(14, result.EdgeCount);
        }

        [Test]
        public void BoundingBoxWithBoxTest()
        {
            Vertex a = new Vertex(4, 4);
            Vertex b = new Vertex(11, 4);
            Vertex c = new Vertex(11, 11);
            Vertex d = new Vertex(4, 11);
            Vertex bb1 = new Vertex(0, 0);
            Vertex bb2 = new Vertex(15, 0);
            Vertex bb3 = new Vertex(15, 15);
            Vertex bb4 = new Vertex(0, 15);
            var vertices = new[] { a, b, c, d, bb1, bb2, bb3, bb4 };
            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);

            // Box
            mesh.AddEdge(a, b);
            mesh.AddEdge(b, c);
            mesh.AddEdge(c, d);
            mesh.AddEdge(d, a);

            // Bounding box
            mesh.AddEdge(bb1, bb2);
            mesh.AddEdge(bb2, bb3);
            mesh.AddEdge(bb3, bb4);
            mesh.AddEdge(bb4, bb1);

            var myp = new MonotoneYPartitioner<Vertex, EdgeBase, FaceBase>(mesh);
            Mesh<Vertex, EdgeBase, FaceBase> result = myp.GetResult();
            Assert.AreEqual(8, result.VertexCount);
            Assert.AreEqual(10, result.EdgeCount);
        }

        [Test]
        public void BoundingBoxWithPlanarSubdivision3Test()
        {
            Vertex a = new Vertex(8, 14);
            Vertex b = new Vertex(3, 10);
            Vertex c = new Vertex(3, 6);
            Vertex d = new Vertex(6, 8);
            Vertex e = new Vertex(3, 3);
            Vertex f = new Vertex(8, 1);
            Vertex g = new Vertex(13, 4);
            Vertex h = new Vertex(13, 11);

            Vertex b1 = new Vertex(2, 10);
            Vertex f1 = new Vertex(8, 9);

            Vertex bb1 = new Vertex(0, 0);
            Vertex bb2 = new Vertex(15, 0);
            Vertex bb3 = new Vertex(15, 15);
            Vertex bb4 = new Vertex(0, 15);

            var vertices = new[] {a, b, c, d, e, f, g, h, b1, f1, bb1, bb2, bb3, bb4};
            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);
            EdgeBase ab = mesh.AddEdge(a, b);
            EdgeBase bc = mesh.AddEdge(b, c);
            EdgeBase cd = mesh.AddEdge(c, d);
            EdgeBase de = mesh.AddEdge(d, e);
            EdgeBase ef = mesh.AddEdge(e, f);
            EdgeBase fg = mesh.AddEdge(f, g);
            EdgeBase gh = mesh.AddEdge(g, h);
            EdgeBase ha = mesh.AddEdge(h, a);

            EdgeBase b_b1 = mesh.AddEdge(b, b1);
            EdgeBase ff1 = mesh.AddEdge(f, f1);

            EdgeBase bb1bb2 = mesh.AddEdge(bb1, bb2);
            EdgeBase bb2bb3 = mesh.AddEdge(bb2, bb3);
            EdgeBase bb3bb4 = mesh.AddEdge(bb3, bb4);
            EdgeBase bb4bb1 = mesh.AddEdge(bb4, bb1);

            var myp = new MonotoneYPartitioner<Vertex, EdgeBase, FaceBase>(mesh);
            Mesh<Vertex, EdgeBase, FaceBase> result = myp.GetResult();
            Assert.AreEqual(14, result.VertexCount);
            Assert.AreEqual(20, result.EdgeCount);

            // Check for input edges
            Assert.AreEqual(ab, mesh.Find(a, b));
            Assert.AreEqual(bc, mesh.Find(b, c));
            Assert.AreEqual(cd, mesh.Find(c, d));
            Assert.AreEqual(de, mesh.Find(d, e));
            Assert.AreEqual(ef, mesh.Find(e, f));
            Assert.AreEqual(fg, mesh.Find(f, g));
            Assert.AreEqual(gh, mesh.Find(g, h));
            Assert.AreEqual(ha, mesh.Find(h, a));
            Assert.AreEqual(b_b1, mesh.Find(b, b1));
            Assert.AreEqual(ff1, mesh.Find(f, f1));
            Assert.AreEqual(bb1bb2, mesh.Find(bb1, bb2));
            Assert.AreEqual(bb2bb3, mesh.Find(bb2, bb3));
            Assert.AreEqual(bb3bb4, mesh.Find(bb3, bb4));
            Assert.AreEqual(bb4bb1, mesh.Find(bb4, bb1));

            // Check for additional edges
            Assert.IsNotNull(mesh.Find(a, bb3));
            Assert.IsNotNull(mesh.Find(b1, a));
            Assert.IsNotNull(mesh.Find(f1, b));
            Assert.IsNotNull(mesh.Find(d, f1));
            Assert.IsNotNull(mesh.Find(c, e));
            Assert.IsNotNull(mesh.Find(f, bb1));
        }

        [Test]
        public void CollinearTest()
        {
            Vertex a = new Vertex(0, 0);
            Vertex b = new Vertex(5, 0);
            Vertex c = new Vertex(10, 0);
            Vertex d = new Vertex(10, 5);
            Vertex e = new Vertex(10, 10);
            Vertex f = new Vertex(5, 10);
            Vertex g = new Vertex(0, 10);
            Vertex h = new Vertex(0, 5);

            var vertices = new[] { a, b, c, d, e, f, g, h, };
            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);
            EdgeBase ab = mesh.AddEdge(a, b);
            EdgeBase bc = mesh.AddEdge(b, c);
            EdgeBase cd = mesh.AddEdge(c, d);
            EdgeBase de = mesh.AddEdge(d, e);
            EdgeBase ef = mesh.AddEdge(e, f);
            EdgeBase fg = mesh.AddEdge(f, g);
            EdgeBase gh = mesh.AddEdge(g, h);
            EdgeBase ha = mesh.AddEdge(h, a);

            var myp = new MonotoneYPartitioner<Vertex, EdgeBase, FaceBase>(mesh);
            Mesh<Vertex, EdgeBase, FaceBase> result = myp.GetResult();
            Assert.AreEqual(8, result.VertexCount);
            Assert.AreEqual(8, result.EdgeCount);

            // Check for input edges
            Assert.AreEqual(ab, mesh.Find(a, b));
            Assert.AreEqual(bc, mesh.Find(b, c));
            Assert.AreEqual(cd, mesh.Find(c, d));
            Assert.AreEqual(de, mesh.Find(d, e));
            Assert.AreEqual(ef, mesh.Find(e, f));
            Assert.AreEqual(fg, mesh.Find(f, g));
            Assert.AreEqual(gh, mesh.Find(g, h));
            Assert.AreEqual(ha, mesh.Find(h, a));
        }
    }
}
