using System;
using System.Collections.Generic;
using System.Text;

namespace Numeric
{
    /// <summary>
    /// Implements Liang-Barsky parametric line clipping
    /// Clip 2D line segment defined by a and b against an upright clip
    /// rectangle with corners at p and q. The function returns true if
    /// the visible part of the line seqment is returned in a and b.
    /// This function operates only on the first two elements of a vector
    /// The remainder are untouched
    /// </summary>
    public static class RectangleClip
    {
        /// <summary>
        // Implements Liang-Barsky parametric line clipping
        // This function computes a new value or tE or tL for an interior
        // intersection of a line segment and an edge. If the line segment
        // can be trivially rejected, false is returned; if it cannot be,
        // true is returned and the value of tE or tL is adjusted.
        /// </summary>
        /// <param name="denom"></param>
        /// <param name="sum"></param>
        /// <param name="tE"></param>
        /// <param name="tL"></param>
        /// <returns>false if the line segment cannot be trivially rejected</returns>
        private static bool ClipT(double denom, double num, ref double tE, ref double tL)
        {
            bool accept = true;
            double t = num / denom;
            if (denom > 0.0)
            {
                if (t > tL)
                {
                    accept = false;
                }
                else if (t > tE)
                {
                    tE = t;
                }
            }
            else if (denom < 0.0)
            {
                t = num / denom;
                if (t < tE)
                {
                    accept = false;
                }
                else if (t < tL)
                {
                    tL = t;
                }
            }
            else if (num > 0.0)
            {
                accept = false;
            }
            return accept;
        }

        /// <summary>
        /// The function returns true if the visible part of the line seqment
        /// is returned in a and b.  
        /// </summary>
        /// <param name="p">The lower-left of an upright clip-rectangle</param>
        /// <param name="q">The upper-right of an upright clip-rectangle</param>
        /// <param name="a">On entry, the start of the line segment to be clipped, on exit the start of the clipped segment</param>
        /// <param name="b">On entry, the end of the line segment to be clipped, on exit the end of the clipped segment</param>
        /// <returns>true if any part of the segment is within the clip rectangle</returns>
        public static bool Clip2D(Point2D p, Point2D q, ref Point2D a, ref Point2D b)
        {
            Vector2D d = b - a;
            bool visible = false;
            double tE = 0.0;
            double tL = 1.0;
            if (ClipT(d.DeltaX, p.X - a.X, ref tE, ref tL))
            {
                if (ClipT(-d.DeltaX, a.X - q.X, ref tE, ref tL))
                {
                    if (ClipT(d.DeltaY, p.Y - a.Y, ref tE, ref tL))
                    {
                        if (ClipT(-d.DeltaY, a.Y - q.Y, ref tE, ref tL))
                        {
                            visible = true;
                            if (tL < 1.0)
                            {
                                b = a + d * tL;
                            }
                            if (tE > 0.0)
                            {
                                a = a + d * tE;
                            }
                        }
                    }
                }
            }
            return visible;
        }
    }
}
