using System;
using System.Collections.Generic;
using System.Text;

using Numeric;

namespace Geometry
{
    public static class VectorGeometry
    {
        /// <summary>
        /// Create a new vector at right angles to the supplied vector, rotated through 90° clockwise.
        /// </summary>
        /// <param name="v"></param>
        /// <returns>A Vector2D a right angles, and to the right of, the supplied vector</returns>
        public static Vector2D PerpendicularRight(Vector2D v)
        {
            // TODO: Can be replaced by -v.Perp
            return new Vector2D(v.DeltaY, -v.DeltaX);
        }

        /// <summary>
        /// Determine the direction bisecting the clockwise angle between vectors
        /// a and b when vectors a and b are placed tip-to-tail.
        /// </summary>
        /// <param name="a">First vector</param>
        /// <param name="b">Second vector</param>
        /// <returns></returns>
        public static Direction2D Bisector(ref Vector2D a, ref Vector2D b)
        {
            Vector2D aUnit = a.Unit;
            Vector2D bUnit = b.Unit;
            int sign = Math.Sign(a.Determinant(b));
            if (sign < 0)
            {
                Direction2D bisector = new Direction2D(bUnit - aUnit);
                return bisector;
            }
            if (sign > 0)
            {
                Direction2D bisector = new Direction2D(aUnit - bUnit);
                return bisector;
            }
            // sign == 0 - vectors are parallel
            if (Math.Sign(a.DeltaX) == Math.Sign(b.DeltaX)
             && Math.Sign(a.DeltaY) == Math.Sign(b.DeltaY))
            {
                return new Direction2D(PerpendicularRight(a));
            }
            return new Direction2D(b);
        }
    }
}
