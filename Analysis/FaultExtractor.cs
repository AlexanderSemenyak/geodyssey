using System;
using System.Collections.Generic;
using System.Text;

using Model;

namespace Analysis
{
    /// <summary>
    /// Extract faults by using ridge extraction techniques from the image processing
    /// world.
    /// </summary>
    public class FaultExtractor
    {
        #region Fields
        private IGridCoordinateTransformation probabilityMap;
        #endregion

        #region Construction
        public FaultExtractor(IGridCoordinateTransformation faultProbabilityMap)
        {
            this.probabilityMap = faultProbabilityMap;
        }
        #endregion

        #region Methods
        public void Execute()
        {
            // Create scale-space representations
            // with Gaussian blur

            // For each scale
            //   Compute Hessian partial Lxx Lyy Lxy  images

            //IRegularGrid2D probabilityMapDx = Convolve(probabilityMap, sobelDx);
            //IRegularGrid2D probabilityMapDy = Convolve(probabilityMap, sobelDy);

        }
        #endregion
    }
}
