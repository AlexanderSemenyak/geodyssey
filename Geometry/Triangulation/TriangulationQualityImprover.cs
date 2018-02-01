using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Numeric;
using Utility.Collections;
using Wintellect.PowerCollections;

using Geometry.HalfEdge;

namespace Geometry.Triangulation
{
    /// <summary>
    /// Class for improving the quality of a triangulation stored in a mesh
    /// </summary>
    public class TriangulationQualityImprover<TVertex, TEdge, TFace>
        where TVertex :VertexBase, IPositionable2D
        where TEdge :EdgeBase, new()
        where TFace :FaceBase, new()
    {
        private readonly Mesh<TVertex, TEdge, TFace> mesh;

        public TriangulationQualityImprover(Mesh<TVertex, TEdge, TFace> mesh)
        {
            this.mesh = mesh;
        }

        public void Improve()
        {
            // Compute a quality measure for each face and place into a priority queue
            IEnumerable<Pair<double, TFace>> pairs = mesh.Faces.Select(f => new Pair<double, TFace>(Quality(f), f));
            var queue = new PriorityQueueDictionary<double, TFace>(pairs);


        }

        /// <summary>
        /// Compute a measure of quality for the given face that ranges
        /// from 0 (degenerate triangle) to 1 (equilateral triangle)
        /// </summary>
        /// <param name="face"></param>
        private static double Quality(TFace face)
        {
            if (face.EdgeCount != 3)
            {
                throw new ArgumentException("Face must be triangular");
            }

            // The quality measure we use is (Sqrt(3) / 36) * (Area / Perimeter^2) which is dimensionless
            Triangle2D triangle = new Triangle2D(face.Vertices.Cast<IPositionable2D>().Select(v => v.Position));
            const double factor = 20.7846097; // 36 / Sqrt(3);
            double quality = factor * triangle.Area / Math.Pow(triangle.Perimeter, 2);
            return quality;
        }
    }
}
