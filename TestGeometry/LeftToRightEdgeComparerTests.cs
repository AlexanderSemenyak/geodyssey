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
    public class LeftToRightEdgeComparerTests
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

        private Mesh<Vertex, EdgeBase, FaceBase> mesh;
        private Vertex v1;
        private Vertex v2;
        private Vertex v3;
        private Vertex v4;
        private Vertex v6;
        private Vertex v7;

        private EdgeBase edgeTwoToOne;
        private EdgeBase edgeTwoToOne2; // parallel edge
        private EdgeBase edgeOneToTwo; // antiparallel edge
        private EdgeBase edgeThreeToTwo;
        private EdgeBase edgeTwoToThree; // parallel edge
        private EdgeBase edgeFourToThree;
        private EdgeBase edgeSevenToSix;

        private Vertex v100;
        private Vertex v101;
        private Vertex v102;
        private Vertex v103;
        private Vertex v104;
        private Vertex v105;
        private Vertex v106;

        private EdgeBase e100to101;
        private EdgeBase e101to102;
        private EdgeBase e103to104;
        private EdgeBase e105to106;

        private Vertex v200;
        private Vertex v201;
        private Vertex v202;

        private EdgeBase e200to201;
        private EdgeBase e202to201;

        private Vertex v300;
        private Vertex v301;
        private Vertex v302;
        private Vertex v303;

        private EdgeBase e300to301;
        private EdgeBase e302to303;

        private LeftToRightEdgeComparer comparer;

        [SetUp]
        public void Init()
        {
            mesh = new Mesh<Vertex, EdgeBase, FaceBase>(false, true);

            v1 = new Vertex(3, 10);
            v2 = new Vertex(3, 6);
            v3 = new Vertex(6, 8);
            v4 = new Vertex(3, 3);
            v6 = new Vertex(13, 4);
            v7 = new Vertex(13, 11);

            mesh.Add(v1);
            mesh.Add(v2);
            edgeTwoToOne = mesh.AddEdge(v2, v1);
            edgeTwoToOne2 = mesh.AddEdge(v2, v1);
            edgeOneToTwo = mesh.AddEdge(v1, v2);

            mesh.Add(v3);
            edgeThreeToTwo = mesh.AddEdge(v3, v2);
            edgeTwoToThree = mesh.AddEdge(v2, v3);

            mesh.Add(v4);
            edgeFourToThree = mesh.AddEdge(v4, v3);

            mesh.Add(v7);
            mesh.Add(v6);
            edgeSevenToSix = mesh.AddEdge(v7, v6);

            v100 = new Vertex(101, 103);
            v101 = new Vertex(103, 103);
            v102 = new Vertex(108, 103);
            v103 = new Vertex(107, 103);
            v104 = new Vertex(109, 103);
            v105 = new Vertex(104, 101);
            v106 = new Vertex(106, 106);

            mesh.Add(v100);
            mesh.Add(v101);
            e100to101 = mesh.AddEdge(v100, v101);

            mesh.Add(v101);
            mesh.Add(v102);
            e101to102 = mesh.AddEdge(v101, v102);

            mesh.Add(v103);
            mesh.Add(v104);
            e103to104 = mesh.AddEdge(v103, v104);

            mesh.Add(v105);
            mesh.Add(v106);
            e105to106 = mesh.AddEdge(v105, v106);

            v200 = new Vertex(210, 180);
            v201 = new Vertex(90, 180);
            v202 = new Vertex(90, 60);

            mesh.Add(v200);
            mesh.Add(v201);
            mesh.Add(v202);
            e200to201 = mesh.AddEdge(v200, v201);
            e202to201 = mesh.AddEdge(v202, v201);

            v300 = new Vertex(210, 180);
            v301 = new Vertex(90, 180);
            v302 = new Vertex(90, 60);
            v303 = new Vertex(90, 170);

            mesh.Add(v300);
            mesh.Add(v301);
            mesh.Add(v302);
            mesh.Add(v303);
            e300to301 = mesh.AddEdge(v300, v301);
            e302to303 = mesh.AddEdge(v302, v303);
            
            comparer = new LeftToRightEdgeComparer();
        }

        [Test]
        public void CompareEdgeTwoToOneWithEdgeFourToThree()
        {
            Assert.AreEqual(-1, comparer.Compare(edgeTwoToOne, edgeFourToThree));
            Assert.AreEqual( 0, comparer.Compare(edgeTwoToOne, edgeTwoToOne));
            Assert.AreEqual( 0, comparer.Compare(edgeFourToThree, edgeFourToThree));
            Assert.AreEqual(+1, comparer.Compare(edgeFourToThree, edgeTwoToOne));
        }

        [Test]
        public void CompareEdgeTwoToOneWithEdgeSevenToSix()
        {
            Assert.AreEqual(-1, comparer.Compare(edgeTwoToOne, edgeSevenToSix));
            Assert.AreEqual(0, comparer.Compare(edgeTwoToOne, edgeTwoToOne));
            Assert.AreEqual(0, comparer.Compare(edgeSevenToSix, edgeSevenToSix));
            Assert.AreEqual(+1, comparer.Compare(edgeSevenToSix, edgeTwoToOne));
        }

        [Test]
        public void CompareEdgeFourToThreeWithEdgeSevenToSix()
        {
            Assert.AreEqual(-1, comparer.Compare(edgeFourToThree, edgeSevenToSix));
            Assert.AreEqual(0, comparer.Compare(edgeFourToThree, edgeFourToThree));
            Assert.AreEqual(0, comparer.Compare(edgeSevenToSix, edgeSevenToSix));
            Assert.AreEqual(+1, comparer.Compare(edgeSevenToSix, edgeFourToThree));
        }

        [Test]
        public void CompareIdenticalEdges()
        {
            Assert.AreEqual(0, comparer.Compare(edgeTwoToOne, edgeTwoToOne2));
            Assert.AreEqual(0, comparer.Compare(edgeTwoToOne2, edgeTwoToOne));
        }

        [Test]
        public void CompareIdenticalOppositeEdges()
        {
            Assert.AreEqual(0, comparer.Compare(edgeTwoToOne, edgeOneToTwo));
            Assert.AreEqual(0, comparer.Compare(edgeOneToTwo, edgeTwoToOne));
        }

        [Test]
        public void CompareSharedStartEnd()
        {
            Assert.AreEqual(-1, comparer.Compare(edgeThreeToTwo, edgeFourToThree));
            Assert.AreEqual(+1, comparer.Compare(edgeFourToThree, edgeThreeToTwo));
        }

        [Test]
        public void CompareSharedEndStart()
        {
            Assert.AreEqual(+1, comparer.Compare(edgeThreeToTwo, edgeTwoToOne));
            Assert.AreEqual(-1, comparer.Compare(edgeTwoToOne, edgeThreeToTwo));
        }

        [Test]
        public void CompareSharedStartStart()
        {
            Assert.AreEqual(-1, comparer.Compare(edgeTwoToOne, edgeTwoToThree));
            Assert.AreEqual(+1, comparer.Compare(edgeTwoToThree, edgeTwoToOne));
        }

        [Test]
        public void CompareSharedEndEnd()
        {
            Assert.AreEqual(+1, comparer.Compare(edgeFourToThree, edgeTwoToThree));
            Assert.AreEqual(-1, comparer.Compare(edgeTwoToThree, edgeFourToThree));
        }

        [Test]
        public void CompareFirstHorizontal1()
        {
            Assert.AreEqual(-1, comparer.Compare(e100to101, e105to106));
            Assert.AreEqual(0,  comparer.Compare(e100to101, e100to101));
            Assert.AreEqual(0,  comparer.Compare(e105to106, e105to106));
            Assert.AreEqual(+1, comparer.Compare(e105to106, e100to101));
        }

        [Test]
        public void CompareFirstHoriontal2()
        {
            Assert.AreEqual(-1, comparer.Compare(e101to102, e105to106));
            Assert.AreEqual(0,  comparer.Compare(e101to102, e101to102));
            Assert.AreEqual(0,  comparer.Compare(e105to106, e105to106));
            Assert.AreEqual(+1, comparer.Compare(e105to106, e101to102));
        }

        [Test]
        public void CompareFirstHoriontal3()
        {
            Assert.AreEqual(+1, comparer.Compare(e103to104, e105to106));
            Assert.AreEqual(0, comparer.Compare(e103to104, e103to104));
            Assert.AreEqual(0, comparer.Compare(e105to106, e105to106));
            Assert.AreEqual(-1, comparer.Compare(e105to106, e103to104));
        }

        [Test]
        public void CompareBothHorizontal1()
        {
            Assert.AreEqual(-1, comparer.Compare(e100to101, e101to102));
            Assert.AreEqual(0, comparer.Compare(e100to101, e100to101));
            Assert.AreEqual(0, comparer.Compare(e101to102, e101to102));
            Assert.AreEqual(+1, comparer.Compare(e101to102, e100to101));
        }

        [Test]
        public void CompareBothHorizontal2()
        {
            Assert.AreEqual(-1, comparer.Compare(e100to101, e103to104));
            Assert.AreEqual(0, comparer.Compare(e100to101, e100to101));
            Assert.AreEqual(0, comparer.Compare(e103to104, e103to104));
            Assert.AreEqual(+1, comparer.Compare(e103to104, e100to101));
        }

        [Test]
        public void CompareBothHorizontal3()
        {
            Assert.AreEqual(-1, comparer.Compare(e101to102, e103to104));
            Assert.AreEqual(0, comparer.Compare(e101to102, e101to102));
            Assert.AreEqual(0, comparer.Compare(e103to104, e103to104));
            Assert.AreEqual(+1, comparer.Compare(e103to104, e101to102));
        }

        [Test]
        public void ComparePerpendicularConnected()
        {
            Assert.AreEqual(-1, comparer.Compare(e202to201, e200to201));
            Assert.AreEqual(0, comparer.Compare(e202to201, e202to201));
            Assert.AreEqual(0, comparer.Compare(e200to201, e200to201));
            Assert.AreEqual(+1, comparer.Compare(e200to201, e202to201));
        }

        [Test]
        public void ComparePerpendicularDisconnected()
        {
            Assert.AreEqual(-1, comparer.Compare(e302to303, e300to301));
            Assert.AreEqual(0, comparer.Compare(e302to303, e302to303));
            Assert.AreEqual(0, comparer.Compare(e300to301, e300to301));
            Assert.AreEqual(+1, comparer.Compare(e300to301, e302to303));
        }
    }
}
