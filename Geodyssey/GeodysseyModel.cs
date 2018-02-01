using System;
using System.Collections.Generic;
using System.Text;

using Utility;
using Model;
using Builders;

namespace Geodyssey
{
    public class GeodysseyModel : IGeoModel, IFactoryProduct
    {
        #region Fields
        private readonly List<IRegularGrid2D> grids = new List<IRegularGrid2D>();
        #endregion

        #region Construction
        public GeodysseyModel()
        {
        }
        #endregion

        #region IGeoModel Members
        public IGrid2DBuilder CreateGridBuilder()
        {
            return new GeodysseyGrid2DBuilder(this);
        }

        public IRegularGrid2D this[int index]
        {
            get { return grids[index]; }
        }

        public void Add(IRegularGrid2D grid)
        {
            grids.Add(grid);
            //Console.WriteLine(grid.ToString());
        }
        #endregion


        #region IFactoryProduct Members
        public object GetFactoryKey()
        {
            return "Geodyssey";
        }
        #endregion
    }
}
