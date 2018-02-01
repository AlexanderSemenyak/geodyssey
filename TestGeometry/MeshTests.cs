using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Geometry.HalfEdge;

namespace Geometry
{
    using NUnit.Framework;

    [TestFixture]
    public class MeshTests
    {
        private Mesh<VertexBase, EdgeBase, FaceBase> mesh;

        [SetUp]
        public void Setup()
        {
            mesh = new Mesh<VertexBase, EdgeBase, FaceBase>();
        }

        [Test]
        public void DefaultConstruction()
        {
            Assert.AreEqual(0, mesh.VertexCount);
        }

        [Test]
        public void SingleIsolatedVertex()
        {
            VertexBase vA = new VertexBase();
            mesh.Add(vA);
            Assert.AreEqual(1, mesh.VertexCount);
            Assert.IsTrue(vA.IsIsolated);
        }

        [Test]
        public void TwoIsolatedVertices()
        {
            VertexBase vA = new VertexBase();
            VertexBase vB = new VertexBase();
            mesh.Add(vA);
            mesh.Add(vB);
            Assert.AreEqual(2, mesh.VertexCount);
            Assert.IsTrue(vA.IsIsolated);
            Assert.IsTrue(vB.IsIsolated);
        }

        [Test]
        public void SingleEdge()
        {
            VertexBase vA = new VertexBase();
            VertexBase vB = new VertexBase();
            mesh.Add(vA);
            mesh.Add(vB);
            EdgeBase edge = new EdgeBase();
            EdgeBase actualEdge = mesh.AddEdge(vA, vB, edge);
            Assert.AreEqual(2, mesh.VertexCount);
            Assert.AreEqual(1, mesh.EdgeCount);
            Assert.IsFalse(vA.IsIsolated);
            Assert.IsFalse(vB.IsIsolated);
            Assert.AreSame(edge, actualEdge);
            Assert.AreSame(vA, edge.Source);
            Assert.AreSame(vB, edge.Target);
        }

        [Test]
        public void DuplicateEdge()
        {
            VertexBase vA = new VertexBase();
            VertexBase vB = new VertexBase();
            mesh.Add(vA);
            mesh.Add(vB);
            EdgeBase edge1 = new EdgeBase();
            EdgeBase actualEdge1 = mesh.AddEdge(vA, vB, edge1);
            EdgeBase edge2 = new EdgeBase();
            EdgeBase actualEdge2 = mesh.AddEdge(vA, vB, edge2);
            Assert.AreSame(edge1, actualEdge1);
            Assert.AreSame(actualEdge1, actualEdge2);
            Assert.AreNotSame(edge2, actualEdge2);
        }

        [Test]
        public void Triangle()
        {
            VertexBase vA = new VertexBase();
            VertexBase vB = new VertexBase();
            VertexBase vC = new VertexBase();
            mesh.Add(vA);
            mesh.Add(vB);
            mesh.Add(vC);

            EdgeBase eA = mesh.AddEdge(vB, vC);
            EdgeBase eB = mesh.AddEdge(vC, vA);
            EdgeBase eC = mesh.AddEdge(vA, vB);

            Assert.AreNotSame(eA, eB);
            Assert.AreNotSame(eB, eC);
            Assert.AreNotSame(eC, eA);
            Assert.AreEqual(2, vA.Degree);
            Assert.AreEqual(2, vB.Degree);
            Assert.AreEqual(2, vC.Degree);
        }

        [Test]
        public void Quadilateral()
        {
            VertexBase vA = new VertexBase();
            VertexBase vB = new VertexBase();
            VertexBase vC = new VertexBase();
            VertexBase vD = new VertexBase();

            mesh.Add(vA);
            mesh.Add(vB);
            mesh.Add(vC);
            mesh.Add(vD);

            EdgeBase eA = mesh.AddEdge(vA, vB);
            EdgeBase eB = mesh.AddEdge(vB, vC);
            EdgeBase eC = mesh.AddEdge(vC, vD);
            EdgeBase eD = mesh.AddEdge(vD, vA);

            Assert.AreNotSame(eA, eB);
            Assert.AreNotSame(eB, eC);
            Assert.AreNotSame(eC, eD);
            Assert.AreNotSame(eD, eA);
            Assert.AreEqual(4, mesh.VertexCount);
            Assert.AreEqual(4, mesh.EdgeCount);
            Assert.AreEqual(0, mesh.FaceCount);
            Assert.AreEqual(2, vA.Degree);
            Assert.AreEqual(2, vB.Degree);
            Assert.AreEqual(2, vC.Degree);
            Assert.AreEqual(2, vD.Degree);
        }

        [Test]
        public void BracedQuadilateral()
        {
            VertexBase vA = new VertexBase();
            VertexBase vB = new VertexBase();
            VertexBase vC = new VertexBase();
            VertexBase vD = new VertexBase();

            mesh.Add(vA);
            mesh.Add(vB);
            mesh.Add(vC);
            mesh.Add(vD);

            EdgeBase eA = mesh.AddEdge(vA, vB);
            EdgeBase eB = mesh.AddEdge(vB, vC);
            EdgeBase eC = mesh.AddEdge(vC, vD);
            EdgeBase eD = mesh.AddEdge(vD, vA);
            EdgeBase brace = mesh.AddEdge(vA, vC);

            Assert.AreNotSame(eA, eB);
            Assert.AreNotSame(eB, eC);
            Assert.AreNotSame(eC, eD);
            Assert.AreNotSame(eD, eA);
            Assert.AreNotEqual(eA, brace);
            Assert.AreEqual(4, mesh.VertexCount);
            Assert.AreEqual(5, mesh.EdgeCount);
            Assert.AreEqual(0, mesh.FaceCount);
            Assert.AreEqual(3, vA.Degree);
            Assert.AreEqual(2, vB.Degree);
            Assert.AreEqual(3, vC.Degree);
            Assert.AreEqual(2, vD.Degree);
        }

        [Test]
        public void Tetrahedron()
        {
            VertexBase vA = new VertexBase();
            VertexBase vB = new VertexBase();
            VertexBase vC = new VertexBase();
            VertexBase vD = new VertexBase();

            mesh.Add(vA);
            mesh.Add(vB);
            mesh.Add(vC);
            mesh.Add(vD);

            EdgeBase e1 = mesh.AddEdge(vA, vB);
            EdgeBase e2 = mesh.AddEdge(vB, vC);
            EdgeBase e3 = mesh.AddEdge(vC, vA);
            EdgeBase e4 = mesh.AddEdge(vA, vD);
            EdgeBase e5 = mesh.AddEdge(vB, vD);
            EdgeBase e6 = mesh.AddEdge(vC, vD);

            FaceBase fa = mesh.AddFace(e1, e2, e3);
            FaceBase fb = mesh.AddFace(e1, e5, e4);
            FaceBase fc = mesh.AddFace(e2, e6, e5);
            FaceBase fd = mesh.AddFace(e3, e4, e6);

            Assert.AreNotEqual(e1, e2);
            Assert.AreNotEqual(e2, e3);
            Assert.AreNotEqual(e3, e4);
            Assert.AreNotEqual(e4, e5);
            Assert.AreNotEqual(e5, e6);

            Assert.AreEqual(3, vA.Degree);
            Assert.AreEqual(3, vB.Degree);
            Assert.AreEqual(3, vC.Degree);
            Assert.AreEqual(3, vD.Degree);

            Assert.AreEqual(4, mesh.VertexCount);
            Assert.AreEqual(6, mesh.EdgeCount);

            Assert.AreEqual(3, fa.EdgeCount);
            Assert.AreEqual(3, fb.EdgeCount);
            Assert.AreEqual(3, fc.EdgeCount);
            Assert.AreEqual(3, fd.EdgeCount);
        }


    }
}
