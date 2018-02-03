using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Utility;
using Geodyssey;
using Loaders;
using Model;
using Analysis;

namespace Athena
{
    class AthenaApp
    {
        // usage : Training: C:\Users\rjs\Documents\dev\p4workspace\sandbox\geodyssey\proto\Athena\testdata\training.irap
        //         Expected: C:\Users\rjs\Documents\dev\p4workspace\sandbox\geodyssey\proto\Athena\testdata\expected.grd
        //         Grid for analysis: F:\sapere_aude\data\IrapClassic\grid_surface\ASCII\RMS\analysis.irap
        static int Main(string[] args)
        {
            // Get a URI from the command line argument
            Uri trainingInputUri = UriFromArg(args, 0);
            Uri trainingExpectedUri = UriFromArg(args, 1);
            Uri analysisUri = UriFromArg(args, 2);

            GeodysseyModel model = new GeodysseyModel();
            LoaderController.Instance.Open(trainingInputUri, model);
            IRegularGrid2D trainingInputGrid = model[0]; // The first grid

            LoaderController.Instance.Open(trainingExpectedUri, model);
            IRegularGrid2D trainingExpectedGrid = model[1]; // The second grid

            LoaderController.Instance.Open(analysisUri, model);
            IRegularGrid2D analysisGrid = model[2]; // The third grid

            // Replaces blanks with 0.0
            for (int j = 0; j < trainingExpectedGrid.SizeJ; ++j)
            {
                for (int i = 0; i < trainingExpectedGrid.SizeI; ++i)
                {
                    if (!trainingExpectedGrid[i, j].HasValue)
                    {
                        trainingExpectedGrid[i, j] = 0.0;
                    }
                    else
                    {
                        if (trainingExpectedGrid[i, j] < 0.0)
                        {
                            trainingExpectedGrid[i, j] = 0.0;
                        }
                        else if (trainingExpectedGrid[i, j] > 1.0)
                        {
                            trainingExpectedGrid[i, j] = 1.0;
                        }
                    }
                }
            }

            int matrixWidth = 5;
            FaultInHorizonClassifer classifier = new FaultInHorizonClassifer(matrixWidth);
            classifier.Learn(trainingInputGrid, trainingExpectedGrid);
            IRegularGrid2D predictedGrid = classifier.CreateFaultProbability(analysisGrid);
            predictedGrid.WriteSurfer6BinaryFile("predicted.grd");
            return 0;
        }

        private static Uri UriFromArg(string[] args, int arg)
        {
            Uri uri;
            try
            {
                uri = new Uri(args[arg]);
            }
            catch (UriFormatException)
            {
                string absPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + args[arg];
                try
                {
                    uri = new Uri(absPath);
                }
                catch (UriFormatException)
                {
                    uri = null;
                }
            }
            return uri;
        }
    }
}
