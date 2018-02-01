using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Analysis;
using Geodyssey;
using Image;
using Loaders;
using Model;
using BitImage;
using Utility.Collections;

namespace Artemis
{
    class ArtemisApp
    {
        static void Main(string[] args)
        {
            Uri horizonUri = UriFromArg(args, 0);
            GeodysseyModel model = new GeodysseyModel();
            LoaderController.Instance.Open(horizonUri, model);
            IRegularGrid2D horizon = model[0];

            //string pathOriginal = horizonUri.LocalPath.Replace(".dat", ".grd");
            //horizon.WriteSurfer6BinaryFile(pathOriginal);

            // TODO: Make IRegularGrid IEnumerable

            var extentMap = new FastImage<bool>(horizon.SizeI, horizon.SizeJ, horizon.Select(item => item.HasValue));
            IImage<bool> faultMap = Morphology.Invert(extentMap);

            IImage<double> distanceMap = DistanceMap.EuclideanTransform(extentMap);



            // Remove anything above a threshold distance from data
            const double threshold = 50;
            var clippedFaultMap = extentMap.CloneTransform((i, j) => distanceMap[i, j] < threshold
                                                                       && faultMap[i, j]);

            Trace.WriteLine("Pepper filter");
            IImage<bool> filtered = Morphology.PepperFiltering(5, clippedFaultMap);
            Trace.WriteLine("Closing gaps");
            IImage<bool> closed = Morphology.Closing(filtered);
            Trace.WriteLine("Thinning until convergence");
            IImage<bool> thinned = Morphology.ThinUntilConvergence(closed);
            Trace.WriteLine("Thinning blocks until convergence");
            IImage<bool> blockthinned = Morphology.ThinBlockUntilConvergence(thinned);
            Trace.WriteLine("Filling");
            IImage<bool> filled = Morphology.Fill(blockthinned);

            WriteBinaryImageSurfer6Grid(horizon, filled, horizonUri.LocalPath.Replace(".grd", "_filled.grd"));

            //// Create a double valued 'binary' image showing the extent of the horizon data
            //FastImage<double> extentMap = new FastImage<double>(horizon.SizeI, horizon.SizeJ);
            //for (int i = 0; i < horizon.SizeI; ++i)
            //{
            //    for (int j = 0; j < horizon.SizeJ; ++j)
            //    {
            //        bool hasValue = horizon[i, j].HasValue;
            //        extentMap[i, j] = hasValue ? 1.0 : 0.0;
            //    }
            //}

            //int extent = extentMap.Where(v => v == 1.0).Count();
            //double extentProportion = (double) extent / (extentMap.Width * extentMap.Height);

            //IImage<double> scaledExtentMap = Scaler.Downscale(extentMap, 5);

            //IImage<double> smoothedExtentMap = scaledExtentMap.CloneSize();
            //Convolver.GaussianSmooth(scaledExtentMap, smoothedExtentMap, 3.0);

            //IRegularGrid2D smoothedGrid = horizon.CloneSize(smoothedExtentMap.Width, smoothedExtentMap.Height);

            //for (int i = 0 ; i < smoothedGrid.SizeI; ++i)
            //{
            //    for (int j = 0 ; j < smoothedGrid.SizeJ; ++j)
            //    {
            //        smoothedGrid[i, j] = smoothedExtentMap[i, j];
            //    }
            //}

            //PriorityQueue<double> orderedIntensities = PriorityQueue<double>.CreateHighFirstOut(smoothedExtentMap);
            //int k =  (int) (extentProportion * orderedIntensities.Count);
            //Debug.Assert(k >= 0);
            //for (int i = 0 ; i < k - 1; ++i)
            //{
            //    orderedIntensities.Dequeue();
            //}
            //double threshold = orderedIntensities.Dequeue();


            //string pathSmoothed = horizonUri.LocalPath.Replace(".grd", "_smoothed.grd");
            //smoothedGrid.WriteSurfer6BinaryFile(pathSmoothed);

            //IImage<bool> thresholdMap = BitImage.Analysis.Threshold(smoothedExtentMap, threshold * 2.0);

            //int actual = thresholdMap.Where(v => v).Count();
            //double actualProportion = (double) actual / (thresholdMap.Width * thresholdMap.Height);

            //IRegularGrid2D thresholdGrid = horizon.CloneSize(thresholdMap.Width, thresholdMap.Height);

            //for (int i = 0; i < smoothedGrid.SizeI; ++i)
            //{
            //    for (int j = 0; j < smoothedGrid.SizeJ; ++j)
            //    {
            //        thresholdGrid[i, j] = thresholdMap[i, j] ? 1.0 : 0.0;
            //    }
            //}

            //string pathThresholded = horizonUri.LocalPath.Replace(".grd", "_thresholded.grd");
            //thresholdGrid.WriteSurfer6BinaryFile(pathThresholded);

            //IImage<double> distanceMap = DistanceMap.EuclideanTransform(scaledExtentMap);




            // Convert the image back to a grid for convenient output
            IRegularGrid2D distanceGrid = horizon.CloneSize(distanceMap.Width, distanceMap.Height);
            for (int i = 0; i < distanceMap.Width; ++i)
            {
                for (int j = 0; j < distanceMap.Height; ++j)
                {
                    distanceGrid[i, j] = distanceMap[i, j];
                }
            }

            string pathDistance = horizonUri.LocalPath.Replace(".grd", "_distance.grd");
            distanceGrid.WriteSurfer6BinaryFile(pathDistance);
        }

        private static void WriteBinaryImageSurfer6Grid(IRegularGrid2D prototypeGrid, IImage<bool> image, string uri)
        {
            IRegularGrid2D thresholdGrid = prototypeGrid.CloneSize(image.Width, image.Height);
            for (int i = 0; i < image.Width; ++i)
            {
                for (int j = 0; j < image.Height; ++j)
                {
                    thresholdGrid[i, j] = image[i, j] ? 1.0 : 0.0;
                }
            }
            thresholdGrid.WriteSurfer6BinaryFile(uri);
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
