using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Builders;
using Geodyssey;

namespace LoaderTest
{
    public class TestGrid2DBuilder : IGrid2DBuilder
    {
        #region Fields
        TestModel model;
        double xOrigin;
        double yOrigin;
        int iNum;
        int jNum;
        double iInc;
        double jInc;
        double orientation;
        RegularGrid2D grid = null;
        #endregion

        #region Construction
        public TestGrid2DBuilder(TestModel model)
        {
            this.model = model;
        }
        #endregion

        #region IGrid2DBuilder Members

        public double Orientation
        {
            set { orientation = value; }
        }

        public double XOrigin
        {
            set { xOrigin = value; }
        }

        public double YOrigin
        {
            set { yOrigin = value; }
        }

        public double IInc
        {
            set { iInc = value; }
        }

        public double JInc
        {
            set { jInc = value; }
        }

        public int INum
        {
            set { iNum = value; }
        }

        public int JNum
        {
            set { jNum = value; }
        }

        public double this[int i, int j]
        {
            set
            {
                if (grid == null)
                {
                    // TODO: Assert on complete set of parameters here
                    grid = new RegularGrid2D(xOrigin, yOrigin, iInc, jInc, iNum, jNum);
                }
                grid[i, j] = value;
            }
        }

        #endregion

        #region IBuilder Members

        public void Build()
        {
            model.Add(grid);
        }

        #endregion
    }
}
