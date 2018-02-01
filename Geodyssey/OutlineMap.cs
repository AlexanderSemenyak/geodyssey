using System;
using System.Collections.Generic;
using System.Text;

using Numeric;

namespace Geodyssey
{
    public class OutlineMap
    {
        private Point2D min;
        private Point2D max;
        private List<ICollection<Point2D>> polygons = new List<ICollection<Point2D>>();

        #region Construction

        public OutlineMap(Point2D min, Point2D max)
        {
            this.min = min;
            this.max = max;
        }

        #endregion

        #region Properties

        public double MinX
        {
            get { return min.X; }
        }

        public double MinY
        {
            get { return min.Y; }
        }

        public double MaxX
        {
            get { return max.X; }
        }

        public double MaxY
        {
            get { return max.Y; }
        }

        #endregion

        #region Methods

        public void AddPolygon(ICollection<Point2D> polygon)
        {
            polygons.Add(polygon);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ICollection<Point2D> polygon in polygons)
            {
                foreach (Point2D point in polygon)
                {
                    sb.AppendFormat("{0}\t{1}", point.X, point.Y);
                    sb.AppendLine();
                }
                sb.AppendLine("%");
            }
            return sb.ToString();
        }

        #endregion
    }
}
