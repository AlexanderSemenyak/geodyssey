using System;
using System.Collections.Generic;
using System.Text;

using Numeric;
using Geometry;
using Geometry.HalfEdge;

using Utility.Extensions.System.Collections.Generic;

namespace Geometry.PolygonPartitioning
{
    using NUnit.Framework;

    [TestFixture]
    public class MonotonePolygonTriangulatorTests
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
        public void Test1()
        {
            Vertex a = new Vertex(384450, 7171260);
            Vertex b = new Vertex(384450, 7171380);
            Vertex c = new Vertex(384570, 7171500);
            Vertex d = new Vertex(384690, 7171500);
            Vertex e = new Vertex(384810, 7171500);
            Vertex f = new Vertex(384930, 7171500);
            Vertex g = new Vertex(385050, 7171380);
            Vertex h = new Vertex(385170, 7171260);

            var vertices = new[] {a, b, c, d, e, f, g, h};
            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);
            mesh.AddEdge(a, b);
            mesh.AddEdge(b, c);
            mesh.AddEdge(c, d);
            mesh.AddEdge(d, e);
            mesh.AddEdge(e, f);
            mesh.AddEdge(f, g);
            mesh.AddEdge(g, h);
            mesh.AddEdge(h, a);
            FaceBase face = mesh.AddFace(vertices);
            var myt = new MonotonePolygonTriangulator<Vertex, EdgeBase, FaceBase>(mesh, face);
            Mesh<Vertex, EdgeBase, FaceBase> result = myt.GetResult();

            Assert.AreEqual(1, result.Euler);
            Assert.AreEqual(vertices.Length - 2, result.FaceCount);
            foreach (FaceBase fc in result.Faces)
            {
                Assert.AreEqual(3, fc.EdgeCount);
            }
        }

        [Test]
        public void Test2()
        {
            Vertex a = new Vertex(384690, 7171500);
            Vertex b = new Vertex(384570, 7171500);
            Vertex c = new Vertex(383610, 7171500);
            Vertex d = new Vertex(383730, 7171620);
            Vertex e = new Vertex(383850, 7171740);
            Vertex f = new Vertex(383970, 7171860);
            Vertex g = new Vertex(384090, 7171860);
            Vertex h = new Vertex(384690, 7171860);
            Vertex i = new Vertex(384690, 7171740);
            Vertex j = new Vertex(384690, 7171620);

            var vertices = new[] { a, b, c, d, e, f, g, h, i, j };
            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);
            mesh.AddEdge(a, b);
            mesh.AddEdge(b, c);
            mesh.AddEdge(c, d);
            mesh.AddEdge(d, e);
            mesh.AddEdge(e, f);
            mesh.AddEdge(f, g);
            mesh.AddEdge(g, h);
            mesh.AddEdge(h, i);
            mesh.AddEdge(i, j);
            mesh.AddEdge(j, a);
            FaceBase face = mesh.AddFace(vertices);
            var myt = new MonotonePolygonTriangulator<Vertex, EdgeBase, FaceBase>(mesh, face);
            Mesh<Vertex, EdgeBase, FaceBase> result = myt.GetResult();

            Assert.AreEqual(1, result.Euler);
            Assert.AreEqual(vertices.Length - 2, result.FaceCount);
            foreach (FaceBase fc in result.Faces)
            {
                Assert.AreEqual(3, fc.EdgeCount);
            }
        }

        [Test]
        public void Test3()
        {
            var vertices = new[]
                               {
                                   new Vertex(384210, 7175340),
                                   new Vertex(384330, 7175220),
                                   new Vertex(384330, 7175100),
                                   new Vertex(384210, 7174980),
                                   new Vertex(384210, 7174860),
                                   new Vertex(384210, 7174740),
                                   new Vertex(384210, 7174620),
                                   new Vertex(384210, 7174500),
                                   new Vertex(384210, 7174380),
                                   new Vertex(384330, 7174260),
                                   new Vertex(384330, 7174140),
                                   new Vertex(384330, 7174020),
                                   new Vertex(384330, 7173900),
                                   new Vertex(384330, 7173780),
                                   new Vertex(384330, 7173660),
                                   new Vertex(384330, 7173540),
                                   new Vertex(384330, 7173420),
                                   new Vertex(384210, 7173300),
                                   new Vertex(384330, 7173300),
                                   new Vertex(384450, 7173300),
                                   new Vertex(384570, 7173180),
                                   new Vertex(384570, 7173060),
                                   new Vertex(384570, 7172940),
                                   new Vertex(384570, 7172820),
                                   new Vertex(384570, 7172700),
                                   new Vertex(384570, 7172580),
                                   new Vertex(384570, 7172460),
                                   new Vertex(384570, 7172340),
                                   new Vertex(384570, 7172220),
                                   new Vertex(384570, 7172100),
                                   new Vertex(384210, 7171980),
                                   new Vertex(383970, 7171860),
                                   new Vertex(383850, 7171740),
                                   new Vertex(383730, 7171620),
                                   new Vertex(383250, 7171500),
                                   new Vertex(383370, 7171500),
                                   new Vertex(383490, 7171500),
                                   new Vertex(383490, 7171380),
                                   new Vertex(383610, 7171260),
                                   new Vertex(384450, 7171260),
                                   new Vertex(385170, 7171260),
                                   new Vertex(391410, 7171260),
                                   new Vertex(391530, 7171260),
                                   new Vertex(391650, 7171140),
                                   new Vertex(391770, 7171140),
                                   new Vertex(393930, 7171140),
                                   new Vertex(388890, 7171020),
                                   new Vertex(389010, 7170900),
                                   new Vertex(389010, 7170780),
                                   new Vertex(389010, 7170660),
                                   new Vertex(389010, 7170540),
                                   new Vertex(386010, 7170420),
                                   new Vertex(386130, 7170300),
                                   new Vertex(386130, 7170180),
                                   new Vertex(386250, 7170060),
                                   new Vertex(386370, 7169940),
                                   new Vertex(386490, 7169820),
                                   new Vertex(386610, 7169700),
                                   new Vertex(386730, 7169580),
                                   new Vertex(386850, 7169460),
                                   new Vertex(386970, 7169460),
                                   new Vertex(387090, 7169340),
                                   new Vertex(387090, 7169220),
                                   new Vertex(387210, 7169100),
                                   new Vertex(387330, 7168980),
                                   new Vertex(387450, 7168860),
                                   new Vertex(387450, 7168740),
                                   new Vertex(387450, 7168620),
                                   new Vertex(387450, 7168500),
                                   new Vertex(387450, 7168380),
                                   new Vertex(387450, 7168260),
                                   new Vertex(388170, 7168260),
                                   new Vertex(388170, 7168140),
                                   new Vertex(388050, 7168020),
                                   new Vertex(387930, 7167900),
                                   new Vertex(387810, 7167780),
                                   new Vertex(387690, 7167660),
                                   new Vertex(387090, 7167540),
                                   new Vertex(386970, 7167420),
                                   new Vertex(387690, 7167420),
                                   new Vertex(387810, 7167300),
                                   new Vertex(387930, 7167180),
                                   new Vertex(388050, 7167060),
                                   new Vertex(388170, 7167060),
                                   new Vertex(388290, 7166940),
                                   new Vertex(388410, 7166820),
                                   new Vertex(388530, 7166820),
                                   new Vertex(388650, 7166700),
                                   new Vertex(388770, 7166580),
                                   new Vertex(388890, 7166460),
                                   new Vertex(389010, 7166460),
                                   new Vertex(389130, 7166340),
                                   new Vertex(389130, 7166220),
                                   new Vertex(389130, 7166100),
                                   new Vertex(389130, 7165980),
                                   new Vertex(389130, 7165860),
                                   new Vertex(389130, 7165740),
                                   new Vertex(389130, 7165620),
                                   new Vertex(389250, 7165500),
                                   new Vertex(389730, 7165500),
                                   new Vertex(390690, 7165500),
                                   new Vertex(399450, 7165500),
                                   new Vertex(403890, 7165500),
                                   new Vertex(405210, 7165500),
                                   new Vertex(383249, 7165499),
                                   new Vertex(383249, 7203301),
                                   new Vertex(425011, 7203301),
                                   new Vertex(384690, 7203300),
                                   new Vertex(384690, 7203180),
                                   new Vertex(387090, 7203180),
                                   new Vertex(387090, 7203060),
                                   new Vertex(387210, 7202940),
                                   new Vertex(384570, 7202820),
                                   new Vertex(384570, 7202700),
                                   new Vertex(384090, 7202580),
                                   new Vertex(384210, 7202580),
                                   new Vertex(384210, 7202460),
                                   new Vertex(384210, 7202340),
                                   new Vertex(384210, 7202220),
                                   new Vertex(384210, 7202100),
                                   new Vertex(383850, 7201980),
                                   new Vertex(383970, 7201980),
                                   new Vertex(384090, 7201980),
                                   new Vertex(384570, 7201980),
                                   new Vertex(384690, 7201980),
                                   new Vertex(384810, 7201980),
                                   new Vertex(384810, 7201860),
                                   new Vertex(384810, 7201740),
                                   new Vertex(384930, 7201620),
                                   new Vertex(386970, 7201620),
                                   new Vertex(386850, 7201500),
                                   new Vertex(383850, 7201380),
                                   new Vertex(383730, 7201260),
                                   new Vertex(386850, 7201260),
                                   new Vertex(386730, 7201140),
                                   new Vertex(386610, 7201020),
                                   new Vertex(383490, 7200900),
                                   new Vertex(383250, 7200780),
                                   new Vertex(383370, 7200780),
                                   new Vertex(386610, 7200780),
                                   new Vertex(386610, 7200660),
                                   new Vertex(386610, 7200540),
                                   new Vertex(386490, 7200420),
                                   new Vertex(386490, 7200300),
                                   new Vertex(384570, 7200180),
                                   new Vertex(384570, 7200060),
                                   new Vertex(384450, 7199940),
                                   new Vertex(384330, 7199820),
                                   new Vertex(384210, 7199700),
                                   new Vertex(384210, 7199580),
                                   new Vertex(384210, 7199460),
                                   new Vertex(384090, 7199340),
                                   new Vertex(384090, 7199220),
                                   new Vertex(383970, 7199100),
                                   new Vertex(383610, 7198980),
                                   new Vertex(383730, 7198980),
                                   new Vertex(383850, 7198980),
                                   new Vertex(383850, 7198860),
                                   new Vertex(386130, 7198860),
                                   new Vertex(385170, 7198740),
                                   new Vertex(385050, 7198620),
                                   new Vertex(384930, 7198500),
                                   new Vertex(384930, 7198380),
                                   new Vertex(384810, 7198260),
                                   new Vertex(384810, 7198140),
                                   new Vertex(384690, 7198020),
                                   new Vertex(385770, 7198020),
                                   new Vertex(385770, 7197900),
                                   new Vertex(385770, 7197780),
                                   new Vertex(385770, 7197660),
                                   new Vertex(385770, 7197540),
                                   new Vertex(385770, 7197420),
                                   new Vertex(385650, 7197300),
                                   new Vertex(385650, 7197180),
                                   new Vertex(385530, 7197060),
                                   new Vertex(385410, 7196940),
                                   new Vertex(385410, 7196820),
                                   new Vertex(385290, 7196700),
                                   new Vertex(385290, 7196580),
                                   new Vertex(384930, 7196460),
                                   new Vertex(385050, 7196460),
                                   new Vertex(385170, 7196460),
                                   new Vertex(385170, 7196340),
                                   new Vertex(385170, 7196220),
                                   new Vertex(385170, 7196100),
                                   new Vertex(385170, 7195980),
                                   new Vertex(385170, 7195860),
                                   new Vertex(385050, 7195740),
                                   new Vertex(385050, 7195620),
                                   new Vertex(384930, 7195500),
                                   new Vertex(384810, 7195380),
                                   new Vertex(383250, 7195260),
                                   new Vertex(383370, 7195260),
                                   new Vertex(383490, 7195140),
                                   new Vertex(383610, 7195020),
                                   new Vertex(383490, 7194900),
                                   new Vertex(383490, 7194780),
                                   new Vertex(383370, 7194660),
                                   new Vertex(383250, 7194540),
                                   new Vertex(384090, 7194540),
                                   new Vertex(383970, 7194420),
                                   new Vertex(383850, 7194300),
                                   new Vertex(383850, 7194180),
                                   new Vertex(383730, 7194060),
                                   new Vertex(383730, 7193940),
                                   new Vertex(383610, 7193820),
                                   new Vertex(383610, 7193700),
                                   new Vertex(383250, 7193580),
                                   new Vertex(383370, 7193580),
                                   new Vertex(383490, 7193580),
                                   new Vertex(383490, 7193460),
                                   new Vertex(383490, 7193340),
                                   new Vertex(383490, 7193220),
                                   new Vertex(383490, 7193100),
                                   new Vertex(383490, 7192980),
                                   new Vertex(383610, 7192860),
                                   new Vertex(383730, 7192740),
                                   new Vertex(383730, 7192620),
                                   new Vertex(383730, 7192500),
                                   new Vertex(383730, 7192380),
                                   new Vertex(383730, 7192260),
                                   new Vertex(388050, 7192260),
                                   new Vertex(387930, 7192140),
                                   new Vertex(387930, 7192020),
                                   new Vertex(387930, 7191900),
                                   new Vertex(387930, 7191780),
                                   new Vertex(385050, 7191660),
                                   new Vertex(385050, 7191540),
                                   new Vertex(385050, 7191420),
                                   new Vertex(388170, 7191420),
                                   new Vertex(383850, 7191300),
                                   new Vertex(383730, 7191180),
                                   new Vertex(383610, 7191060),
                                   new Vertex(383490, 7190940),
                                   new Vertex(383250, 7190820),
                                   new Vertex(383370, 7190820),
                                   new Vertex(383490, 7190820),
                                   new Vertex(383490, 7190700),
                                   new Vertex(385290, 7190700),
                                   new Vertex(385170, 7190580),
                                   new Vertex(385170, 7190460),
                                   new Vertex(385170, 7190340),
                                   new Vertex(385050, 7190220),
                                   new Vertex(385050, 7190100),
                                   new Vertex(385050, 7189980),
                                   new Vertex(384930, 7189860),
                                   new Vertex(383490, 7189740),
                                   new Vertex(383610, 7189620),
                                   new Vertex(383610, 7189500),
                                   new Vertex(383610, 7189380),
                                   new Vertex(383610, 7189260),
                                   new Vertex(383730, 7189140),
                                   new Vertex(383730, 7189020),
                                   new Vertex(383730, 7188900),
                                   new Vertex(383250, 7188780),
                                   new Vertex(383370, 7188780),
                                   new Vertex(383490, 7188780),
                                   new Vertex(383610, 7188780),
                                   new Vertex(384570, 7188780),
                                   new Vertex(384570, 7188660),
                                   new Vertex(384570, 7188540),
                                   new Vertex(384450, 7188420),
                                   new Vertex(384330, 7188300),
                                   new Vertex(384210, 7188180),
                                   new Vertex(384210, 7188060),
                                   new Vertex(384210, 7187940),
                                   new Vertex(384210, 7187820),
                                   new Vertex(384210, 7187700),
                                   new Vertex(383970, 7187580),
                                   new Vertex(383970, 7187460),
                                   new Vertex(383970, 7187340),
                                   new Vertex(383970, 7187220),
                                   new Vertex(384810, 7187220),
                                   new Vertex(384810, 7187100),
                                   new Vertex(384810, 7186980),
                                   new Vertex(384810, 7186860),
                                   new Vertex(384810, 7186740),
                                   new Vertex(384690, 7186620),
                                   new Vertex(384570, 7186500),
                                   new Vertex(384570, 7186380),
                                   new Vertex(384570, 7186260),
                                   new Vertex(384570, 7186140),
                                   new Vertex(384450, 7186020),
                                   new Vertex(384330, 7185900),
                                   new Vertex(384330, 7185780),
                                   new Vertex(384330, 7185660),
                                   new Vertex(384210, 7185540),
                                   new Vertex(384090, 7185420),
                                   new Vertex(384090, 7185300),
                                   new Vertex(383970, 7185180),
                                   new Vertex(383850, 7185060),
                                   new Vertex(383730, 7184940),
                                   new Vertex(383610, 7184820),
                                   new Vertex(383610, 7184700),
                                   new Vertex(383610, 7184580),
                                   new Vertex(383610, 7184460),
                                   new Vertex(383610, 7184340),
                                   new Vertex(383610, 7184220),
                                   new Vertex(383610, 7184100),
                                   new Vertex(383610, 7183980),
                                   new Vertex(383490, 7183860),
                                   new Vertex(383250, 7183740),
                                   new Vertex(383370, 7183740),
                                   new Vertex(383490, 7183740),
                                   new Vertex(383610, 7183620),
                                   new Vertex(386490, 7183620),
                                   new Vertex(386370, 7183500),
                                   new Vertex(386250, 7183380),
                                   new Vertex(386250, 7183260),
                                   new Vertex(386130, 7183140),
                                   new Vertex(386010, 7183020),
                                   new Vertex(386010, 7182900),
                                   new Vertex(385890, 7182780),
                                   new Vertex(385890, 7182660),
                                   new Vertex(383490, 7182540),
                                   new Vertex(383490, 7182420),
                                   new Vertex(383490, 7182300),
                                   new Vertex(383490, 7182180),
                                   new Vertex(383490, 7182060),
                                   new Vertex(385410, 7182060),
                                   new Vertex(385410, 7181940),
                                   new Vertex(385290, 7181820),
                                   new Vertex(385170, 7181700),
                                   new Vertex(385170, 7181580),
                                   new Vertex(384690, 7181460),
                                   new Vertex(384570, 7181340),
                                   new Vertex(384570, 7181220),
                                   new Vertex(384570, 7181100),
                                   new Vertex(384450, 7180980),
                                   new Vertex(384450, 7180860),
                                   new Vertex(383730, 7180740),
                                   new Vertex(383850, 7180620),
                                   new Vertex(383850, 7180500),
                                   new Vertex(383850, 7180380),
                                   new Vertex(383850, 7180260),
                                   new Vertex(383730, 7180140),
                                   new Vertex(383730, 7180020),
                                   new Vertex(383730, 7179900),
                                   new Vertex(383610, 7179780),
                                   new Vertex(383610, 7179660),
                                   new Vertex(383610, 7179540),
                                   new Vertex(383610, 7179420),
                                   new Vertex(383610, 7179300),
                                   new Vertex(383250, 7179180),
                                   new Vertex(383370, 7179180),
                                   new Vertex(383490, 7179180),
                                   new Vertex(383490, 7179060),
                                   new Vertex(383610, 7178940),
                                   new Vertex(383730, 7178820),
                                   new Vertex(383850, 7178700),
                                   new Vertex(383850, 7178580),
                                   new Vertex(383850, 7178460),
                                   new Vertex(383730, 7178340),
                                   new Vertex(383250, 7178220),
                                   new Vertex(383370, 7178220),
                                   new Vertex(383490, 7178220),
                                   new Vertex(383610, 7178220),
                                   new Vertex(383610, 7178100),
                                   new Vertex(383610, 7177980),
                                   new Vertex(383610, 7177860),
                                   new Vertex(383730, 7177740),
                                   new Vertex(383730, 7177620),
                                   new Vertex(383850, 7177500),
                                   new Vertex(383850, 7177380),
                                   new Vertex(383850, 7177260),
                                   new Vertex(383850, 7177140),
                                   new Vertex(383850, 7177020),
                                   new Vertex(383850, 7176900),
                                   new Vertex(383850, 7176780),
                                   new Vertex(383850, 7176660),
                                   new Vertex(383730, 7176540),
                                   new Vertex(383610, 7176420),
                                   new Vertex(383490, 7176300),
                                   new Vertex(385770, 7176300),
                                   new Vertex(385770, 7176180),
                                   new Vertex(385770, 7176060),
                                   new Vertex(385770, 7175940),
                                   new Vertex(385770, 7175820),
                                   new Vertex(384570, 7175700),
                                   new Vertex(384570, 7175580),
                                   new Vertex(384570, 7175460)
                               };

            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);

            vertices.Wrap().ForEachPair((p, q) => mesh.AddEdge(p, q));

            FaceBase face = mesh.AddFace(vertices);
            var myt = new MonotonePolygonTriangulator<Vertex, EdgeBase, FaceBase>(mesh, face);
            Mesh<Vertex, EdgeBase, FaceBase> result = myt.GetResult();

            Assert.AreEqual(1, result.Euler);
            Assert.AreEqual(vertices.Length - 2, result.FaceCount);
            foreach (FaceBase fc in result.Faces)
            {
                Assert.AreEqual(3, fc.EdgeCount);
            }
        }

        [Test]
        public void TestFinalVertices()
        {
            // This test exercises the third diagonal inserting loop in the
            // algorithm, which inserts the final diagonals.
            var vertices = new[]
                               {
                                   new Vertex( 33, 70),
                                   new Vertex( 45, 70),
                                   new Vertex(105, 70),
                                   new Vertex(  9, 58),
                                   new Vertex( 21, 70),
                               };

            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);

            vertices.Wrap().ForEachPair((p, q) => mesh.AddEdge(p, q));

            FaceBase face = mesh.AddFace(vertices);
            var myt = new MonotonePolygonTriangulator<Vertex, EdgeBase, FaceBase>(mesh, face);
            Mesh<Vertex, EdgeBase, FaceBase> result = myt.GetResult();

            Assert.AreEqual(1, result.Euler);
            Assert.AreEqual(vertices.Length - 2, result.FaceCount);
            foreach (FaceBase fc in result.Faces)
            {
                Assert.AreEqual(3, fc.EdgeCount);
            }
        }

        [Test]
        public void Test4()
        {
            // This test is a reduced version of Test3
            var vertices = new[]
                               {
                                    new Vertex(383249,	7203301),
                                    new Vertex(384690,	7203300),
                                    new Vertex(384690,	7203180),
                                    new Vertex(387090,	7203180),
                                    new Vertex(387090,	7203060),
                                    new Vertex(387210,	7202940),
                                    new Vertex(383249,  7202900)
                               };

            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);

            vertices.Wrap().ForEachPair((p, q) => mesh.AddEdge(p, q));

            FaceBase face = mesh.AddFace(vertices);
            var myt = new MonotonePolygonTriangulator<Vertex, EdgeBase, FaceBase>(mesh, face);
            Mesh<Vertex, EdgeBase, FaceBase> result = myt.GetResult();

            Assert.AreEqual(1, result.Euler);
            Assert.AreEqual(vertices.Length - 2, result.FaceCount);
            foreach (FaceBase fc in result.Faces)
            {
                Assert.AreEqual(3, fc.EdgeCount);
            }
        }

        [Test]
        public void Test5()
        {
                    var vertices = new[]
                                       {
                                   new Vertex(405810, 7165740),
                                   new Vertex(405690, 7165860),
                                   new Vertex(420090, 7165980),
                                   new Vertex(419970, 7166100),
                                   new Vertex(419850, 7166100),
                                   new Vertex(419850, 7166220),
                                   new Vertex(419850, 7166340),
                                   new Vertex(419850, 7166460),
                                   new Vertex(420090, 7166580),
                                   new Vertex(421050, 7166700),
                                   new Vertex(421170, 7166820),
                                   new Vertex(421170, 7166940),
                                   new Vertex(421770, 7167060),
                                   new Vertex(421770, 7167180),
                                   new Vertex(421770, 7167300),
                                   new Vertex(421770, 7167420),
                                   new Vertex(422130, 7167540),
                                   new Vertex(422010, 7167540),
                                   new Vertex(421890, 7167540),
                                   new Vertex(407610, 7167540),
                                   new Vertex(407610, 7167660),
                                   new Vertex(408090, 7167780),
                                   new Vertex(419730, 7167900),
                                   new Vertex(419730, 7168020),
                                   new Vertex(407850, 7168020),
                                   new Vertex(407970, 7168140),
                                   new Vertex(408090, 7168260),
                                   new Vertex(423930, 7168380),
                                   new Vertex(424050, 7168500),
                                   new Vertex(424170, 7168620),
                                   new Vertex(424170, 7168740),
                                   new Vertex(424170, 7168860),
                                   new Vertex(424170, 7168980),
                                   new Vertex(424290, 7169100),
                                   new Vertex(424530, 7169220),
                                   new Vertex(424770, 7169340),
                                   new Vertex(425010, 7169460),
                                   new Vertex(424890, 7169460),
                                   new Vertex(409170, 7169460),
                                   new Vertex(409170, 7169580),
                                   new Vertex(409290, 7169700),
                                   new Vertex(409410, 7169820),
                                   new Vertex(409410, 7169940),
                                   new Vertex(409530, 7170060),
                                   new Vertex(409530, 7170180),
                                   new Vertex(409650, 7170300),
                                   new Vertex(409650, 7170420),
                                   new Vertex(409770, 7170540),
                                   new Vertex(409770, 7170660),
                                   new Vertex(409770, 7170780),
                                   new Vertex(409890, 7170900),
                                   new Vertex(409890, 7171020),
                                   new Vertex(410010, 7171140),
                                   new Vertex(410130, 7171260),
                                   new Vertex(410250, 7171380),
                                   new Vertex(410370, 7171500),
                                   new Vertex(410490, 7171620),
                                   new Vertex(410490, 7171740),
                                   new Vertex(410610, 7171860),
                                   new Vertex(410610, 7171980),
                                   new Vertex(410730, 7172100),
                                   new Vertex(410730, 7172220),
                                   new Vertex(410730, 7172340),
                                   new Vertex(410850, 7172460),
                                   new Vertex(410850, 7172580),
                                   new Vertex(410970, 7172700),
                                   new Vertex(411090, 7172820),
                                   new Vertex(411090, 7172940),
                                   new Vertex(411210, 7173060),
                                   new Vertex(411330, 7173180),
                                   new Vertex(411450, 7173300),
                                   new Vertex(411570, 7173420),
                                   new Vertex(411570, 7173540),
                                   new Vertex(411690, 7173660),
                                   new Vertex(411690, 7173780),
                                   new Vertex(411690, 7173900),
                                   new Vertex(411690, 7174020),
                                   new Vertex(411690, 7174140),
                                   new Vertex(411690, 7174260),
                                   new Vertex(411690, 7174380),
                                   new Vertex(411570, 7174500),
                                   new Vertex(411570, 7174620),
                                   new Vertex(411450, 7174740),
                                   new Vertex(411450, 7174860),
                                   new Vertex(411330, 7174980),
                                   new Vertex(411330, 7175100),
                                   new Vertex(411330, 7175220),
                                   new Vertex(411330, 7175340),
                                   new Vertex(411330, 7175460),
                                   new Vertex(411330, 7175580),
                                   new Vertex(411330, 7175700),
                                   new Vertex(411450, 7175820),
                                   new Vertex(411570, 7175940),
                                   new Vertex(411690, 7176060),
                                   new Vertex(412290, 7176180),
                                   new Vertex(412290, 7176300),
                                   new Vertex(412290, 7176420),
                                   new Vertex(412650, 7176540),
                                   new Vertex(412770, 7176660),
                                   new Vertex(412890, 7176780),
                                   new Vertex(413010, 7176900),
                                   new Vertex(413130, 7177020),
                                   new Vertex(413250, 7177140),
                                   new Vertex(413370, 7177260),
                                   new Vertex(413610, 7177380),
                                   new Vertex(413850, 7177500),
                                   new Vertex(413970, 7177620),
                                   new Vertex(414090, 7177740),
                                   new Vertex(414090, 7177860),
                                   new Vertex(414090, 7177980),
                                   new Vertex(414090, 7178100),
                                   new Vertex(414210, 7178220),
                                   new Vertex(414330, 7178340),
                                   new Vertex(414330, 7178460),
                                   new Vertex(414330, 7178580),
                                   new Vertex(414330, 7178700),
                                   new Vertex(414330, 7178820),
                                   new Vertex(414330, 7178940),
                                   new Vertex(414330, 7179060),
                                   new Vertex(414330, 7179180),
                                   new Vertex(414330, 7179300),
                                   new Vertex(414450, 7179420),
                                   new Vertex(414570, 7179540),
                                   new Vertex(414690, 7179660),
                                   new Vertex(414690, 7179780),
                                   new Vertex(414690, 7179900),
                                   new Vertex(414690, 7180020),
                                   new Vertex(414690, 7180140),
                                   new Vertex(414690, 7180260),
                                   new Vertex(414930, 7180380),
                                   new Vertex(415050, 7180500),
                                   new Vertex(415050, 7180620),
                                   new Vertex(415170, 7180740),
                                   new Vertex(415290, 7180860),
                                   new Vertex(415290, 7180980),
                                   new Vertex(415410, 7181100),
                                   new Vertex(415410, 7181220),
                                   new Vertex(415410, 7181340),
                                   new Vertex(415410, 7181460),
                                   new Vertex(415290, 7181580),
                                   new Vertex(415170, 7181700),
                                   new Vertex(415170, 7181820),
                                   new Vertex(415410, 7181940),
                                   new Vertex(415530, 7182060),
                                   new Vertex(415170, 7182060),
                                   new Vertex(415050, 7182180),
                                   new Vertex(415050, 7182300),
                                   new Vertex(415170, 7182420),
                                   new Vertex(415290, 7182540),
                                   new Vertex(416010, 7182660),
                                   new Vertex(416010, 7182780),
                                   new Vertex(416610, 7182900),
                                   new Vertex(416490, 7183020),
                                   new Vertex(416490, 7183140),
                                   new Vertex(416610, 7183260),
                                   new Vertex(416730, 7183380),
                                   new Vertex(416850, 7183500),
                                   new Vertex(416970, 7183620),
                                   new Vertex(416970, 7183740),
                                   new Vertex(416970, 7183860),
                                   new Vertex(416970, 7183980),
                                   new Vertex(417090, 7184100),
                                   new Vertex(417090, 7184220),
                                   new Vertex(417090, 7184340),
                                   new Vertex(417090, 7184460),
                                   new Vertex(417090, 7184580),
                                   new Vertex(417090, 7184700),
                                   new Vertex(417450, 7184820),
                                   new Vertex(417330, 7184820),
                                   new Vertex(417210, 7184820),
                                   new Vertex(417210, 7184940),
                                   new Vertex(417090, 7185060),
                                   new Vertex(416970, 7185180),
                                   new Vertex(416850, 7185300),
                                   new Vertex(416850, 7185420),
                                   new Vertex(416970, 7185540),
                                   new Vertex(416970, 7185660),
                                   new Vertex(416970, 7185780),
                                   new Vertex(416370, 7185780),
                                   new Vertex(416370, 7185900),
                                   new Vertex(416250, 7186020),
                                   new Vertex(416130, 7186140),
                                   new Vertex(416010, 7186260),
                                   new Vertex(416010, 7186380),
                                   new Vertex(415890, 7186500),
                                   new Vertex(415890, 7186620),
                                   new Vertex(415890, 7186740),
                                   new Vertex(415770, 7186860),
                                   new Vertex(415770, 7186980),
                                   new Vertex(415770, 7187100),
                                   new Vertex(415770, 7187220),
                                   new Vertex(415770, 7187340),
                                   new Vertex(415650, 7187460),
                                   new Vertex(415650, 7187580),
                                   new Vertex(415650, 7187700),
                                   new Vertex(415650, 7187820),
                                   new Vertex(415530, 7187940),
                                   new Vertex(415530, 7188060),
                                   new Vertex(415530, 7188180),
                                   new Vertex(415530, 7188300),
                                   new Vertex(415410, 7188420),
                                   new Vertex(415410, 7188540),
                                   new Vertex(415290, 7188660),
                                   new Vertex(415290, 7188780),
                                   new Vertex(415170, 7188900),
                                   new Vertex(415050, 7189020),
                                   new Vertex(414930, 7189140),
                                   new Vertex(414930, 7189260),
                                   new Vertex(414810, 7189380),
                                   new Vertex(414810, 7189500),
                                   new Vertex(414690, 7189620),
                                   new Vertex(414570, 7189740),
                                   new Vertex(414450, 7189860),
                                   new Vertex(414330, 7189980),
                                   new Vertex(414330, 7190100),
                                   new Vertex(414330, 7190220),
                                   new Vertex(414330, 7190340),
                                   new Vertex(414330, 7190460),
                                   new Vertex(414330, 7190580),
                                   new Vertex(414330, 7190700),
                                   new Vertex(414330, 7190820),
                                   new Vertex(414330, 7190940),
                                   new Vertex(414330, 7191060),
                                   new Vertex(414330, 7191180),
                                   new Vertex(414330, 7191300),
                                   new Vertex(414330, 7191420),
                                   new Vertex(414330, 7191540),
                                   new Vertex(414330, 7191660),
                                   new Vertex(414330, 7191780),
                                   new Vertex(414330, 7191900),
                                   new Vertex(414330, 7192020),
                                   new Vertex(414450, 7192140),
                                   new Vertex(414450, 7192260),
                                   new Vertex(414570, 7192380),
                                   new Vertex(414570, 7192500),
                                   new Vertex(414570, 7192620),
                                   new Vertex(414570, 7192740),
                                   new Vertex(414570, 7192860),
                                   new Vertex(414690, 7192980),
                                   new Vertex(414690, 7193100),
                                   new Vertex(414690, 7193220),
                                   new Vertex(414810, 7193340),
                                   new Vertex(414810, 7193460),
                                   new Vertex(414930, 7193580),
                                   new Vertex(414930, 7193700),
                                   new Vertex(414930, 7193820),
                                   new Vertex(414930, 7193940),
                                   new Vertex(415050, 7194060),
                                   new Vertex(415050, 7194180),
                                   new Vertex(415050, 7194300),
                                   new Vertex(415170, 7194420),
                                   new Vertex(415290, 7194540),
                                   new Vertex(415290, 7194660),
                                   new Vertex(415290, 7194780),
                                   new Vertex(415290, 7194900),
                                   new Vertex(415290, 7195020),
                                   new Vertex(415290, 7195140),
                                   new Vertex(415290, 7195260),
                                   new Vertex(415650, 7195380),
                                   new Vertex(415530, 7195380),
                                   new Vertex(415530, 7195500),
                                   new Vertex(415530, 7195620),
                                   new Vertex(415530, 7195740),
                                   new Vertex(415650, 7195860),
                                   new Vertex(415650, 7195980),
                                   new Vertex(415650, 7196100),
                                   new Vertex(415770, 7196220),
                                   new Vertex(415770, 7196340),
                                   new Vertex(415890, 7196460),
                                   new Vertex(415890, 7196580),
                                   new Vertex(416010, 7196700),
                                   new Vertex(416010, 7196820),
                                   new Vertex(416010, 7196940),
                                   new Vertex(416010, 7197060),
                                   new Vertex(416130, 7197180),
                                   new Vertex(416130, 7197300),
                                   new Vertex(416490, 7197420),
                                   new Vertex(416370, 7197420),
                                   new Vertex(416250, 7197420),
                                   new Vertex(416250, 7197540),
                                   new Vertex(416250, 7197660),
                                   new Vertex(416250, 7197780),
                                   new Vertex(416130, 7197900),
                                   new Vertex(416130, 7198020),
                                   new Vertex(416130, 7198140),
                                   new Vertex(416250, 7198260),
                                   new Vertex(416250, 7198380),
                                   new Vertex(416370, 7198500),
                                   new Vertex(416490, 7198620),
                                   new Vertex(416490, 7198740),
                                   new Vertex(416490, 7198860),
                                   new Vertex(416490, 7198980),
                                   new Vertex(416490, 7199100),
                                   new Vertex(416610, 7199220),
                                   new Vertex(416730, 7199340),
                                   new Vertex(416730, 7199460),
                                   new Vertex(416850, 7199580),
                                   new Vertex(416850, 7199700),
                                   new Vertex(416850, 7199820),
                                   new Vertex(416970, 7199940),
                                   new Vertex(416970, 7200060),
                                   new Vertex(417090, 7200180),
                                   new Vertex(417090, 7200300),
                                   new Vertex(417090, 7200420),
                                   new Vertex(417210, 7200540),
                                   new Vertex(417450, 7200660),
                                   new Vertex(417330, 7200660),
                                   new Vertex(417330, 7200780),
                                   new Vertex(417330, 7200900),
                                   new Vertex(417330, 7201020),
                                   new Vertex(417330, 7201140),
                                   new Vertex(417330, 7201260),
                                   new Vertex(417330, 7201380),
                                   new Vertex(417330, 7201500),
                                   new Vertex(417330, 7201620),
                                   new Vertex(417450, 7201740),
                                   new Vertex(417450, 7201860),
                                   new Vertex(417570, 7201980),
                                   new Vertex(417930, 7202100),
                                   new Vertex(419850, 7202220),
                                   new Vertex(420210, 7202340),
                                   new Vertex(420450, 7202460),
                                   new Vertex(420810, 7202580),
                                   new Vertex(420930, 7202700),
                                   new Vertex(421050, 7202820),
                                   new Vertex(422130, 7202940),
                                   new Vertex(422010, 7203060),
                                   new Vertex(421530, 7203060),
                                   new Vertex(421410, 7203060),
                                   new Vertex(421290, 7203060),
                                   new Vertex(421170, 7203060),
                                   new Vertex(421170, 7203180),
                                   new Vertex(421170, 7203300),
                                   new Vertex(414450, 7203300),
                                   new Vertex(412050, 7203300),
                                   new Vertex(410370, 7203300),
                                   new Vertex(409530, 7203300),
                                   new Vertex(408090, 7203300),
                                   new Vertex(404610, 7203300),
                                   new Vertex(401010, 7203300),
                                   new Vertex(396210, 7203300),
                                   new Vertex(393450, 7203300),
                                   new Vertex(393090, 7203300),
                                   new Vertex(387090, 7203300),
                                   new Vertex(384690, 7203300),
                                   new Vertex(425011, 7203301),
                                   new Vertex(425011, 7165499),
                                   new Vertex(383249, 7165499),
                                   new Vertex(405210, 7165500),
                                   new Vertex(405210, 7165620),
                                   new Vertex(412170, 7165740),
                                   new Vertex(412050, 7165740),
                                   new Vertex(411930, 7165740),
                                   new Vertex(408690, 7165740),
                                   new Vertex(408570, 7165740),
                                   new Vertex(408450, 7165740)
                               };

            var mesh = new Mesh<Vertex, EdgeBase, FaceBase>(vertices);

            vertices.Wrap().ForEachPair((p, q) => mesh.AddEdge(p, q));

            FaceBase face = mesh.AddFace(vertices);
            var myt = new MonotonePolygonTriangulator<Vertex, EdgeBase, FaceBase>(mesh, face);
            Mesh<Vertex, EdgeBase, FaceBase> result = myt.GetResult();

            Assert.AreEqual(1, result.Euler);
            Assert.AreEqual(vertices.Length - 2, result.FaceCount);
            foreach (FaceBase fc in result.Faces)
            {
                Assert.AreEqual(3, fc.EdgeCount);
            }
        }
    }
}
