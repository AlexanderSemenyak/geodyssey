using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geometry.HalfEdge;

namespace Geometry
{
    public static class Algorithms
    {
        public static Mesh<TVertex, TEdge, TFace> TriangulatePlanarSubdivision<TVertex, TEdge, TFace>(Mesh<TVertex, TEdge, TFace> figure)
        {
            return TriangulatePlanarSubdivision<TVertex, TEdge, TFace>(figure, false);    
        }

        public static Mesh<TVertex, TEdge, TFace> TriangulatePlanarSubdivision<TVertex, TEdge, TFace>(Mesh<TVertex, TEdge, TFace> figure, bool inPlace)
        {
            return TriangulatePlanarSubdivision<TVertex, TEdge, TFace, TEdge>(figure, inPlace);
        }

        public static Mesh<TVertex, TEdge, TFace> TriangulatePlanarSubdivision<TVertex, TEdge, TFace, TNewEdge>(Mesh<TVertex, TEdge, TFace> figure)
        {
            return TriangulatePlanarSubdivision<TVertex, TEdge, TFace, TNewEdge>(figure, false);
        }

        public static Mesh<TVertex, TEdge, TFace> TriangulatePlanarSubdivision<TVertex, TEdge, TFace, TNewEdge>(Mesh<TVertex, TEdge, TFace> figure, bool inPlace)
        {
            var triangulator = PlanarSubdivisionTriangulator<TVertex, TEdge, TFace, TNewEdge>(figure, inPlace);
            return triangulator.Execute();
        }
    }
}
