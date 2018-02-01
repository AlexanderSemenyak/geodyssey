using System;
using System.Collections.Generic;
using System.Text;

namespace Builders
{
    public interface IGrid2DBuilder : IBuilder
    {   // 
        // ^y     .j
        // |    .
        // |  .
        // |.       x
        // +-------->
        //   . 45°
        //      .
        //         .i
        //
        // Clockwise angle from the x-axis to the i-axis
        double Orientation
        {
            set;
        }

        double XOrigin
        {
            set;
        }

        double YOrigin
        {
            set;
        }

        double IInc
        {
            set;
        }

        double JInc
        {
            set;
        }

        int INum
        {
            set;
        }

        int JNum
        {
            set;
        }

        double this[int i, int j]
        {
            set;
        }
    }
}
