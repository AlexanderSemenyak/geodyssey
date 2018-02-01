using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Numeric;

namespace Geometry.Triangulation
{
    public class IndexedTriangle
    {
        private Triangulation triangulation;

        public int p1;
        public int p2;
        public int p3;

        private Triangle2D manifestation;

        public IndexedTriangle(Triangulation triangulation, int point1, int point2, int point3)
	    {
            this.triangulation = triangulation;
		    p1 = point1;
            p2 = point2;
            p3 = point3;
            manifestation = new Triangle2D(triangulation.VertexAt(p1),
                                           triangulation.VertexAt(p2),
                                           triangulation.VertexAt(p3));
	    }

        public bool InCircumcircle(Point2D point)
        {
            return manifestation.InCircumcircle(point);
        }
    }
}
