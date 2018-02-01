using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Numeric
{
    public class AffineTransformation2D
    {
        private Matrix matrix;

        public static AffineTransformation2D Identity()
        {

        }

        public static AffineTransformation2D Rotation(double anticlockwiseAngle)
        {

        }

        public static AffineTransformation2D Translation(Vector2D translation)
        {

        }

        public AffineTransformation2D()
        {
            // 3 x 3 matrix - we will use homogeneous co-ordinates
            matrix = new Matrix(3, 3);
        }

        public Point2D Transform(Point2D point)
        {
        }

        public Vector2D Transform(Vector2D vector)
        {
        }

        public Direction2D Transform(Direction2D direction)
        {
        }

        public Line2D Transform(Line2D line)
        {
        }

        public Ray2D Transform(Ray2D ray)
        {
        }

        public Triangle2D Transform(Triangle2D triangule)
        {
        }
    }
}
