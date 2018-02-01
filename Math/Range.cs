using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Numeric
{
    public static class Range
    {
        public static double Clip(double value, double limit1, double limit2)
        {
            if (limit1 == limit2)
            {
                return limit1;
            }
            else if (limit1 < limit2)
            {
                return Math.Min(Math.Max(limit1, value), limit2);
            }
            return Math.Min(Math.Max(limit2, value), limit1); 
        }

        public static int Clip(int value, int limit1, int limit2)
        {
            if (limit1 == limit2)
            {
                return limit1;
            }
            else if (limit1 < limit2)
            {
                return Math.Min(Math.Max(limit1, value), limit2);
            }
            return Math.Min(Math.Max(limit2, value), limit1);
        }
    }
}
