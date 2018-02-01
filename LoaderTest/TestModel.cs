using System;
using System.Collections.Generic;
using System.Text;

using Utility;
using Builders;
using Geodyssey;

using Model;

namespace LoaderTest
{
    public class TestModel : IGeoModel, IFactoryProduct
    {
        #region Fields
        private List<IRegularGrid2D> grids = new List<IRegularGrid2D>();
        #endregion

        #region IGeoModel Members
        public IGrid2DBuilder CreateGridBuilder()
        {
            return new TestGrid2DBuilder(this);
        }

        public IRegularGrid2D this[int index]
        {
            get { return grids[index]; }
        }

        public void Add(IRegularGrid2D grid)
        {
            grids.Add(grid);
            Console.WriteLine(grid.ToString());
        }
        #endregion


        #region IFactoryProduct Members
        public object GetFactoryKey()
        {
            return "Test";
        }
        #endregion
    }
}
