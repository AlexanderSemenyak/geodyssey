using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Image;
using Model;

namespace Analysis
{
    /// <summary>
    /// An adapter which presents an IRegularGrid2D as
    /// an image for image processing routines
    /// </summary>
    public class GridImage : ImageBase<double>
    {
        #region Fields
        readonly IRegularGrid2D grid;
        #endregion

        #region Construction
        /// <summary>
        /// Construct from a grid.  The grid is not copied. The image serves as
        /// an adaptor for the grid.
        /// </summary>
        /// <param name="grid"></param>
        public GridImage(IRegularGrid2D grid)
        {
            this.grid = grid;
        }
        #endregion

        #region Properties
        public IRegularGrid2D Grid
        {
            get { return grid; }
        }
        #endregion

        #region IImage Members

        public override int Width
        {
            get { return grid.SizeI; }
        }

        public override int Height
        {
            get { return grid.SizeJ; }
        }

        public override double this[int column, int row]
        {
            get
            {
                Debug.Assert(column >= 0 && column < Width);
                Debug.Assert(row >= 0 && row < Height);
                Debug.Assert(grid[column, row].HasValue);
                return grid[column, row].Value;
            }
            set
            {
                Debug.Assert(column >= 0 && column < Width);
                Debug.Assert(row >= 0 && row < Height);
                grid[column, row] = value;                
            }
        }

        public override void Clear()
        {
            for (int i = 0; i < grid.SizeI; ++i)
            {
                for (int j = 0; j < grid.SizeJ; ++j)
                {
                    grid[i, j] = 0.0;
                }
            }
        }

        // TODO: This code should not be in this class - its too specialized
        public IImage<bool> CreateBinaryImage(double threshhold)
        {
            IImage<bool> binaryImage = new FastImage<bool>(Width, Height);
            for (int i = 0; i < Width; ++i)
            {
                for (int j = 0; j < Height; ++j)
                {
                    binaryImage[i, j] = this[i, j] > threshhold;
                }
            }
            return binaryImage;
        }

        #endregion

        #region ICloneable Members

        public override object Clone()
        {
            IRegularGrid2D gridClone= (IRegularGrid2D) this.grid.Clone();
            GridImage clone = new GridImage(gridClone);
            return clone;
        }

        #endregion


        public override IImage<double> CloneSize()
        {
            IRegularGrid2D gridClone = grid.CloneSize();
            GridImage clone = new GridImage(gridClone);
            return clone;
        }

        public override IImage<double> CloneSize(int newWidth, int newHeight)
        {
            IRegularGrid2D gridClone = grid.CloneSize(newWidth, newHeight);
            GridImage clone = new GridImage(gridClone);
            return clone;
        }
    }
}
