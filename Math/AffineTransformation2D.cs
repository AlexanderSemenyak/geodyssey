using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace Numeric
{
    public class AffineTransformation2D
    {
        private Matrix matrix;

        public static AffineTransformation2D Identity()
        {
            throw new NotImplementedException();
        }

        public static AffineTransformation2D Rotation(double anticlockwiseAngle)
        {
            throw new NotImplementedException();
        }

        public static AffineTransformation2D Translation(Vector2D translation)
        {
            throw new NotImplementedException();
        }

        public AffineTransformation2D()
        {
            throw new NotImplementedException();
            // 3 x 3 matrix - we will use homogeneous co-ordinates
            //matrix = new Matrix(3, 3);
        }

        public Point2D Transform(Point2D point)
        {
            throw new NotImplementedException();
        }

        public Vector2D Transform(Vector2D vector)
        {
            throw new NotImplementedException();
        }

        public Direction2D Transform(Direction2D direction)
        {
            throw new NotImplementedException();
        }

        public Line2D Transform(Line2D line)
        {
            throw new NotImplementedException();
        }

        public Ray2D Transform(Ray2D ray)
        {
            throw new NotImplementedException();
        }

        public Triangle2D Transform(Triangle2D triangule)
        {
            throw new NotImplementedException();
        }
    }
}
