using System;
using System.Collections.Generic;

using Model;

namespace Builders
{
    public interface IGeoModel
    {
        IGrid2DBuilder CreateGridBuilder();
        void Add(IRegularGrid2D grid);
        IRegularGrid2D this[int index]
        {
            get;
        }
    }
}
