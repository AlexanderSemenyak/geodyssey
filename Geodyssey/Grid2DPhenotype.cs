/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-22
 * Time: 18:23
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Wintellect.PowerCollections;

using Numeric;
using Model;

namespace Geodyssey
{
	/// <summary>
	/// Description of Grid2DPhenotype.
	/// </summary>
	public class Grid2DPhenotype : Phenotype
	{
	    private IRegularGrid2D grid;
	    private IGridCoordinateTransformation hypsometric;
        private IRegularGrid2D partialDerivativeI;
        private IRegularGrid2D partialDerivativeJ;
	    
	    #region Construction
	    
	    public Grid2DPhenotype(Grid2DDomain domain) :
	        base(domain)
		{
	        Point2D origin = domain.Min;
	        Vector2D diagonal = domain.Max - domain.Min;
	        Vector2D spacing = new Vector2D(diagonal.DeltaX / domain.SizeI, diagonal.DeltaY / domain.SizeJ);
	        this.grid = new RegularGrid2D(origin, spacing, domain.SizeI, domain.SizeJ, 0.0);
		}

        public Grid2DPhenotype(IRegularGrid2D grid) :
            base(null)
        {
            this.grid = grid;
        }

	    #endregion
	    
	    #region Properties
	    
	    public IRegularGrid2D Grid
	    {
	        get { return grid; }
	    }
	    
	    private IGridCoordinateTransformation HypsometricGrid
	    {
	        get
	        {
	            if (hypsometric == null)
	            {
	                hypsometric = grid.CreateHypsometricGrid();
	            }
	            return hypsometric;
	        }
	    }

        private IRegularGrid2D PartialDerivativeIGrid
        {
            get
            {
                if (partialDerivativeI == null)
                {
                    partialDerivativeI = grid.CreatePartialDerivativeIGrid();
                }
                return partialDerivativeI;
            }
        }

        private IRegularGrid2D PartialDerivativeJGrid
        {
            get
            {
                if (partialDerivativeJ == null)
                {
                    partialDerivativeJ = grid.CreatePartialDerivativeJGrid();
                }
                return partialDerivativeJ;
            }
        }

	    #endregion
	    
	    #region Methods
	            
        public override double[] Compare(Phenotype other)
		{
            Debug.Assert(other is Grid2DPhenotype);
            Grid2DPhenotype otherPhenotype = (Grid2DPhenotype) other;
            return CompareAdjustedHeight(otherPhenotype);
		}

        private double[] CompareHeight(Grid2DPhenotype other)
        {
            // Partial derivative comparisons
            double? rmsZ = grid.RmsDifference(other.Grid);
            Debug.Assert(rmsZ.HasValue);
            return new double[] { rmsZ.Value };
        }

        private double[] CompareAdjustedHeight(Grid2DPhenotype other)
        {
            double? adjustedRmsZ = AdjustedRmsZ(other);
            Debug.Assert(adjustedRmsZ.HasValue);
            return new double[] { adjustedRmsZ.Value };
        }

        /// <summary>
        /// Divide the Grids into tiles, and compute the adjusted height fitness
        /// for each tile.
        /// </summary>
        /// <param name="other">The grid to be compared against this.</param>
        /// <returns>An array of tile fitnesses</returns>
        private double[] CompareTiledAdjustedHeight(Grid2DPhenotype other)
        {
            List<double> result = new List<double>();
            const int numTilesI = 2;
            const int numTilesJ = 2;
            RegularGrid2D adjusted_grid = AdjustedGrid(other);
            int tileSizeI = this.grid.SizeI / numTilesI;
            int tileSizeJ = this.grid.SizeJ / numTilesJ;
            for (int tileI = 0; tileI < numTilesI; ++tileI)
            {
                int beginI = tileI * tileSizeI;
                int endI = (tileI + 1) * tileSizeI;
                for (int tileJ = 0; tileJ < numTilesJ; ++tileJ)
                {
                    int beginJ = tileJ * tileSizeJ;
                    int endJ = (tileJ + 1) * tileSizeJ;
                    double? adjustedRmsZ = grid.RmsDifference(adjusted_grid, beginI, endI, beginJ, endJ);
                    if (adjustedRmsZ.HasValue)
                    {
                        result.Add(adjustedRmsZ.Value);
                    }
                }
            }
            return result.ToArray();
        }

        private double[] CompareAdjustedHeightAndGradients(Grid2DPhenotype other)
        {
            double? adjustedRmsZ = AdjustedRmsZ(other);
            double? rmsGradientI = RmsGradientI(other);
            double? rmsGradientJ = RmsGradientJ(other);
            Debug.Assert(adjustedRmsZ.HasValue);
            Debug.Assert(rmsGradientI.HasValue);
            Debug.Assert(rmsGradientJ.HasValue);
            return new double[] { adjustedRmsZ.Value, rmsGradientI.Value, rmsGradientJ.Value };
        }

        private double[] CompareHeightAndGradients(Grid2DPhenotype other)
        {
            // Partial derivative comparisons
            double? rmsZ = grid.RmsDifference(other.Grid);
            double? rmsDzDx = RmsGradientI(other);
            double? rmsDzDy = RmsGradientJ(other);
            return new double[] { rmsZ.Value, rmsDzDx.Value, rmsDzDy.Value };
        }

        private double[] CompareWeightedHeightAndGradients(Grid2DPhenotype other)
        {
            // Partial derivative comparisons
            double? rmsZ = grid.RmsDifference(other.Grid);
            double? rmsDzDx = RmsGradientI(other);
            double? rmsDzDy = RmsGradientJ(other);
            double result = rmsZ.Value + 100.0 * rmsDzDx.Value + 100.0 * rmsDzDy.Value;
            return new double[] { result };
        }

        private double? AdjustedRmsZ(Grid2DPhenotype other)
        {
            RegularGrid2D adjusted_grid = AdjustedGrid(other);
            double? adjustedRmsZ = grid.RmsDifference(adjusted_grid);
            return adjustedRmsZ;
        }

        private RegularGrid2D AdjustedGrid(Grid2DPhenotype other)
        {
            double mean = grid.Mean();
            double other_mean = other.Grid.Mean();
            double difference = other_mean - mean;
            RegularGrid2D adjusted_grid = (RegularGrid2D)other.Grid.Clone();
            for (int i = 0; i < adjusted_grid.SizeI; ++i)
            {
                for (int j = 0; j < adjusted_grid.SizeJ; ++j)
                {
                    adjusted_grid[i, j] -= difference;
                }
            }
            return adjusted_grid;
        }

        private double? RmsGradientJ(Grid2DPhenotype other)
        {
            double? rmsDzDy = PartialDerivativeJGrid.RmsDifference(other.PartialDerivativeJGrid);
            return rmsDzDy;
        }

        private double? RmsGradientI(Grid2DPhenotype other)
        {
            double? rmsDzDx = PartialDerivativeIGrid.RmsDifference(other.PartialDerivativeIGrid);
            return rmsDzDx;
        }

        public override string ToString()
        {
            return grid.ToString();
        }
        
        #endregion
	}
}
