using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using NeuronDotNet.Core;
using NeuronDotNet.Core.Layers;
using NeuronDotNet.Core.Connections;

using Numeric;
using Model;

namespace Analysis
{
    public class FaultInHorizonClassifer
    {
        #region Fields
        private int matrixWidth;
        private int numHiddenNodes;
        private int numOutputNodes = 1;
        private Network network;
        #endregion

        #region Construction
        public FaultInHorizonClassifer(string filename, int matrixWidth)
        {
            Debug.Assert(matrixWidth >= 3);
            Debug.Assert(matrixWidth % 2 == 1);
            this.matrixWidth = matrixWidth;
            network = new Network(filename);
            network.Initialize();
            network.TrainingCycles = 10;
        }

        public FaultInHorizonClassifer(int matrixWidth)
        {
            Debug.Assert(matrixWidth >= 3);
            Debug.Assert(matrixWidth % 2 == 1);
            this.matrixWidth = matrixWidth;
            int numInputNodes = matrixWidth * matrixWidth;
            numHiddenNodes = numInputNodes + (numInputNodes / 3);
            Layer inputLayer = new SigmoidLayer(numInputNodes);
            Layer hiddenLayer = new SigmoidLayer(numHiddenNodes);
            Layer outputLayer = new SigmoidLayer(numOutputNodes);

            ConnectionFactory.EstablishCompleteConnection(inputLayer, hiddenLayer);
            ConnectionFactory.EstablishCompleteConnection(hiddenLayer, outputLayer);

            network = new Network(inputLayer);
            network.Initialize();
            network.TrainingCycles = 10;
        }

        #endregion

        #region Methods
        public void Learn(IRegularGrid2D input, IRegularGrid2D expected)
        {
            // Build the training samples
            for (int j = 0; j < input.SizeJ; ++j)
            {
                for (int i = 0; i < input.SizeI; ++i)
                {
                    double[] inValues = MatrixInput(input, i, j, true);
                    double[] expectedValues = TrainingExpected(expected, i, j);
                    network.TrainingSamples.Add(new TrainingSample(inValues, expectedValues));
                }
            }
            network.Learn();
            network.Save("athena.net");

            //double[] validate1 = network.Run(new double[] {  0.013, -0.026, -0.060,  0.043, 0.0, -0.037,  0.013, -0.026, -0.060 }); // 0.0
            //double[] validate2 = network.Run(new double[] {  0.134,  0.050, -0.004, 0.078,  0.0, -0.034,  0.031, -0.044, -0.062 }); // 0.49 
            //double[] validate3 = network.Run(new double[] { -0.060,  0.071,  0.246, -0.170, 0.0,  0.296, -0.277,  0.327,  0.943 }); // 0.94
        }

        public IRegularGrid2D CreateFaultProbability(IRegularGrid2D horizon)
        {
            // TODO: This is a little hacky and wasteful - replace this
            // with a Transform method on the Grid class
            IRegularGrid2D probabilityGrid = (IRegularGrid2D) horizon.Clone();
            for (int j = 0; j < horizon.SizeJ; ++j)
            {
                for (int i = 0; i < horizon.SizeI; ++i)
                {
                    double[] matrix = MatrixInput(horizon, i, j, false);
                    double prediction = network.Run(matrix)[0]; // TODO : Assumes only one result
                    probabilityGrid[i, j] = prediction; 
                }
            }
            return probabilityGrid;
        }

        /// <summary>
        /// Returns the array of normalised training values for the
        /// specified grid node, or null if the specified grid node
        /// has no value.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns>Either an array of numInputNodes doubles, or null.</returns>
        private double[] MatrixInput(IRegularGrid2D grid, int i, int j, bool deBias)
        {
            Debug.Assert(grid != null);
            if (!grid[i, j].HasValue)
            {
                return null;
            }
            double zCentre = grid[i, j].Value;
            int centre = (matrixWidth - 1) / 2;
            // Must return a 1D array, since that's what training sample requires
            double[] matrix = new double[matrixWidth * matrixWidth];
            for (int q = 0; q < matrixWidth; ++q)
            {
                for (int p = 0; p < matrixWidth; ++p)
                {
                    int relativeI = p - centre;
                    int relativeJ = q - centre;
                    int absoluteI = i + relativeI;
                    int absoluteJ = j + relativeJ;
                    int index = q * matrixWidth + p;
                    
                    double? z = PaddedGridValue(grid, absoluteI, absoluteJ);
                    Debug.Assert(z.HasValue);
                    // TODO: Assumes square cells
                    double zDiff = z.Value - zCentre;
                    double zNorm = zDiff / grid.Spacing.DeltaX;
                    matrix[index] = zNorm;
                }
            }
            if (deBias)
            {
                DeBiasMatrix(matrix, centre, matrixWidth);
            }

            return matrix;
        }

        /// <summary>
        /// Randomly rotate or mirror the training sample matrix in-place
        /// to remove orientation bias.
        /// </summary>
        /// <param name="matrix">The matrix to be processed</param>
        /// <returns>The randomly modified matrix</returns>
        private void DeBiasMatrix(double[] matrix, int centre, int matrixWidth)
        {
            // Flip X
            if (Rng.DiscreteUniformZeroOrOne() == 1)
            {
                FlipP(matrix, centre, matrixWidth);
            }


            // Flip Y
            if (Rng.DiscreteUniformZeroOrOne() == 1)
            {
                FlipQ(matrix, centre, matrixWidth);
            }

            // TODO: Could add rotation here or instead
        }

        private static void FlipP(double[] matrix, int centre, int matrixWidth)
        {
            for (int p = 0; p < centre; ++p)
            {
                int pTarget = 2 * centre - p;
                for (int q = 0; q < matrixWidth; ++q)
                {
                    int sourceIndex = q * matrixWidth + p;
                    int targetIndex = q * matrixWidth + pTarget;
                    SwapArrayElements(matrix, sourceIndex, targetIndex);
                }
            }
        }

        private static void FlipQ(double[] matrix, int centre, int matrixWidth)
        {
            for (int q = 0; q < centre; ++q)
            {
                int qTarget = 2 * centre - q;
                for (int p = 0; p < matrixWidth; ++p)
                {
                    int sourceIndex = q * matrixWidth + p;
                    int targetIndex = qTarget * matrixWidth + p;
                    SwapArrayElements(matrix, sourceIndex, targetIndex);
                }
            }
        }

        private static void SwapArrayElements(double[] matrix, int sourceIndex, int targetIndex)
        {
            double tmp = matrix[targetIndex];
            matrix[targetIndex] = matrix[sourceIndex];
            matrix[sourceIndex] = tmp;
        }

        /// <summary>
        /// Returns the value for the specified grid cell, padding data off the image
        /// boundaries to allow matrix sampling to work right up to the image edges.
        /// This version reflects the data at the image boundaries.
        /// </summary>
        /// <param name="grid">The grid</param>
        /// <param name="absoluteI">The column coordinate</param>
        /// <param name="absoluteJ">The row coodinate</param>
        /// <returns>Either a real grid value, for in-range i and j, or an interpolated value</returns>
        private double? PaddedGridValue(IRegularGrid2D grid, int absoluteI, int absoluteJ)
        {
            // Modify i co-ordinate if necessary
            if (absoluteI < 0)
            {
                absoluteI = 0 - absoluteI;
            }
            else if (absoluteI > grid.SizeI - 1)
            {
                absoluteI = 2 * (grid.SizeI - 1) - absoluteI;
            }

            // Modify j co-ordinate if necessary
            if (absoluteJ < 0)
            {
                absoluteJ = 0 - absoluteJ;
            }
            else if (absoluteJ > grid.SizeJ - 1)
            {
                absoluteJ = 2 * (grid.SizeJ - 1) - absoluteJ;
            }

            return grid[absoluteI, absoluteJ];
        }

        private double[] TrainingExpected(IRegularGrid2D grid, int i, int j)
        {
            Debug.Assert(grid[i, j].HasValue);
            return new double[] { grid[i, j].Value };
        }

        #endregion
    }
}
