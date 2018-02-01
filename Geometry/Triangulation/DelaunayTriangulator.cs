using System;
using System.Collections.Generic;
using System.Linq;
using Geometry.HalfEdge;
using Numeric;

namespace Geometry.Triangulation
{
    /// <summary>
    /// Performs the Delauney triangulation on a set of vertices.
    /// </summary>
    /// <remarks>
    /// Based on Paul Bourke's "An Algorithm for Interpolating Irregularly-Spaced Data
    /// with Applications in Terrain Modelling"
    /// http://astronomy.swin.edu.au/~pbourke/modelling/triangulate/
    /// </remarks>
    public static class DelaunayTriangulator
    {
       
        public static Triangulation Triangulate(IEnumerable<Point2D> points)
        {
            Triangulation triangulation = new Triangulation(points);
            // Find the maximum and minimum vertex bounds, and the number of points
            // This is to allow calculation of the bounding supertriangle
            int numPoints = triangulation.VertexCount;

            if (numPoints < 3)
            {
                throw new ArgumentException("Need at least three vertices for triangulation");
            }

            int numTriMax = 4 * numPoints;

            double xMin = triangulation.Vertices.First().X;
            double yMin = triangulation.Vertices.First().Y;
            double xMax = triangulation.Vertices.First().X;
            double yMax = triangulation.Vertices.First().Y;
            foreach (Point2D point in points)
            {
                if (point.X < xMin) xMin = point.X;
                if (point.X > xMax) xMax = point.X;
                if (point.Y < yMin) yMin = point.Y;
                if (point.Y > yMax) yMax = point.Y;
            }

            double dx = xMax - xMin;
            double dy = yMax - yMin;
            double dMax = (dx > dy) ? dx : dy;

            double xMid = (xMax + xMin) * 0.5;
            double yMid = (yMax + yMin) * 0.5;

            triangulation.AddVertex(new Point2D(xMid - 2 * dMax, yMid - dMax));
            triangulation.AddVertex(new Point2D(xMid, yMid + 2 * dMax));
            triangulation.AddVertex(new Point2D(xMid + 2 * dMax, yMid - dMax));

            triangulation.AddTriangle(numPoints, numPoints + 1, numPoints + 2);

            // Add points one at a time to the existing mesh
            for (int i = 0; i < numPoints; ++i)
            {
                var edges = new List<IndexedEdge>();
                // Set up the edge buffer.
                // If the vertex lies inside the circumcircle then the
                // three edges of that triangle are added to the edge buffer and the triangle is removed from list.
                // TODO: Can this be done in parallel?
                for (int j = 0; j < triangulation.TriangleCount; ++j)
                {
                    // TODO: Need to cache results of the circumcircle test
                    if (triangulation.TriangleAt(j).InCircumcircle(triangulation.VertexAt(i)))
                    {
                        edges.Add(new IndexedEdge(triangulation.TriangleAt(j).p1, triangulation.TriangleAt(j).p2));
                        edges.Add(new IndexedEdge(triangulation.TriangleAt(j).p2, triangulation.TriangleAt(j).p3));
                        edges.Add(new IndexedEdge(triangulation.TriangleAt(j).p3, triangulation.TriangleAt(j).p1));
                        triangulation.RemoveTriangle(j);
                        --j;
                    }
                }

                // Remove duplicate edges: If all triangles are specified
                // anticlockwise then all interior edges are opposite pointing in direction.
                for (int j = edges.Count - 2; j >= 0; j--)
                {
                    for (int k = edges.Count - 1; k >= j + 1; k--)
                    {
                        if (edges[j].Equals(edges[k]))
                        {
                            edges.RemoveAt(k);
                            edges.RemoveAt(j);
                            --k;
                        }
                    }
                }

                // Form new triangles for the current point
                // Skipping over any tagged edges.
                // All edges are arranged in clockwise order.
                foreach (IndexedEdge edge in edges)
                {
                    triangulation.AddTriangle(edge.p1, edge.p2, i);
                }
            }

            // Remove triangles with supertriangle vertices
            // These are triangles which have a vertex number greater than nv
            for (int i = triangulation.TriangleCount - 1; i >= 0; i--)
            {
                if (triangulation.TriangleAt(i).p1 >= numPoints || triangulation.TriangleAt(i).p2 >= numPoints || triangulation.TriangleAt(i).p3 >= numPoints)
                    triangulation.RemoveTriangle(i);
            }
            //Remove SuperTriangle vertices
            triangulation.RemoveVertex(triangulation.VertexCount - 1);
            triangulation.RemoveVertex(triangulation.VertexCount - 1);
            triangulation.RemoveVertex(triangulation.VertexCount - 1);
            return triangulation;
        }
    }
}