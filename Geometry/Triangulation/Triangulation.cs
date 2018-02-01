using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Numeric;

namespace Geometry.Triangulation
{

    /// <summary>
    /// A simple triangulation class consisting of a list of numbered vertices (v-list)
    /// and a list of triangles (t-list).
    /// </summary>
    public class Triangulation
    {
        private readonly List<Point2D> vertices;
        private readonly List<IndexedTriangle> triangles;

        public Triangulation(IEnumerable<Point2D> vertices)
        {
            IEnumerable<Point2D> orderedVertices = vertices.OrderBy(p => p.X);
            this.vertices = new List<Point2D>(orderedVertices);
            this.triangles = new List<IndexedTriangle>();
        }

        public int VertexCount
        {
            get { return vertices.Count; }
        }

        public int TriangleCount
        {
            get { return triangles.Count; }
        }

        public IEnumerable<Point2D> Vertices
        {
            get
            {
                foreach (Point2D vertex in vertices)
                {
                    yield return vertex;
                }
            }
        }

        public void AddVertex(Point2D vertex)
        {
            vertices.Add(vertex);
        }

        public void AddTriangle(int v1, int v2, int v3)
        {
            triangles.Add(new IndexedTriangle(this, v1, v2, v3));
        }

        public Point2D VertexAt(int index)
        {
            return vertices[index];
        }

        public IndexedTriangle TriangleAt(int index)
        {
            return triangles[index];
        }

        public void RemoveTriangle(int index)
        {
            triangles.RemoveAt(index);
        }

        public void RemoveVertex(int index)
        {
            vertices.RemoveAt(index);
        }


    }
}
