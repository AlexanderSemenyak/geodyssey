using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geometry.RedBlue
{
    internal class SelfIntersectingLineSegmentsException : Exception
    {
        public SelfIntersectingLineSegmentsException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}
