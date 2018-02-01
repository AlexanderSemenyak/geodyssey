using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Numeric;

namespace Geometry.PolygonPartitioning
{
    public class HighYLowXComparer : IComparer<Point2D>
    {
        #region IComparer<Point2D> Members

        public int Compare(Point2D a, Point2D b)
        {
            double dy = a.Y - b.Y;
            if (dy < 0.0)
            {
                return -1;
            }
            if (dy > 0.0)
            {
                return +1;
            }
            double dx = a.X - b.X;
            if (dx < 0.0)
            {
                return +1;
            }
            if (dx > 0.0)
            {
                return -1;
            }
            return 0;
            
        }

        #endregion
    }
}
