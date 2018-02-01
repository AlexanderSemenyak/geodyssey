using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;

using FaultMapper.HalfEdge;
using Geometry.HalfEdge;
using Geometry.PolygonPartitioning;
using Geometry.Triangulation;
using Numeric;
using Utility;
using Geodyssey;
using Loaders;
using Model;
using Analysis;
using Image;
using BitImage;
using FaultMapper;

namespace Apollo
{
    class ApolloApp
    {
        static void Main(string[] args)
        {
            Uri faultProbabilityUri = UriFromArg(args, 0);
            Uri horizonUri          = UriFromArg(args, 1);
            GeodysseyModel model = new GeodysseyModel();
            LoaderController.Instance.Open(faultProbabilityUri, model);
            IRegularGrid2D pFaultGrid = model[0]; // The first grid
            LoaderController.Instance.Open(horizonUri, model);
            IRegularGrid2D horizon    = model[1]; // Horizon grid

            GridImage pFaultImage = new GridImage(pFaultGrid);

            GridImage pFaultImageX = (GridImage) pFaultImage.Clone();
            pFaultImageX.Clear();

            GridImage pFaultImageY = (GridImage) pFaultImage.Clone();
            pFaultImageY.Clear();

            Convolver.GaussianGradient(pFaultImage, pFaultImageX, pFaultImageY, 1.0);

            GridImage pFaultImageXX = (GridImage) pFaultImage.Clone();
            pFaultImageXX.Clear();

            GridImage pFaultImageYY = (GridImage) pFaultImage.Clone();
            pFaultImageYY.Clear();

            GridImage pFaultImageXY = (GridImage) pFaultImage.Clone();
            pFaultImageXY.Clear();


            Convolver.HessianMatrixOfGaussian(pFaultImage, pFaultImageXX, pFaultImageYY, pFaultImageXY, 1.0);

            //GridImage pFaultImageBeta = (GridImage) RidgeDetector.PrincipleCurvatureDirection(pFaultImageXX, pFaultImageYY, pFaultImageXY);

            //GridImage pFaultImagePQ = (GridImage) RidgeDetector.LocalLpq(pFaultImageXX, pFaultImageYY, pFaultImageXY, pFaultImageBeta);

            //GridImage pFaultImageP = (GridImage) RidgeDetector.LocalLp(pFaultImageX, pFaultImageY, pFaultImageBeta);
            //GridImage pFaultImageQ = (GridImage) RidgeDetector.LocalLq(pFaultImageX, pFaultImageY, pFaultImageBeta);
            //GridImage pFaultImagePP = (GridImage) RidgeDetector.LocalLpp(pFaultImageXX, pFaultImageYY, pFaultImageXY, pFaultImageBeta);
            //GridImage pFaultImageQQ = (GridImage) RidgeDetector.LocalLqq(pFaultImageXX, pFaultImageYY, pFaultImageXY, pFaultImageBeta);

            Trace.WriteLine("Ridge detector");
            GridImage pFaultImageRidge = (GridImage) RidgeDetector.Detect(pFaultImageX, pFaultImageY, pFaultImageXX, pFaultImageYY, pFaultImageXY);

            IImage<bool> ridge = pFaultImageRidge.CreateBinaryImage(0.0);
            Trace.WriteLine("Pepper filter");
            IImage<bool> filtered = Morphology.PepperFiltering(5, ridge);
            Trace.WriteLine("Closing gaps");
            IImage<bool> closed = Morphology.Closing(filtered);
            Trace.WriteLine("Thinning until convergence");
            IImage<bool> thinned = Morphology.ThinUntilConvergence(closed);
            Trace.WriteLine("Thinning blocks until convergence");
            IImage<bool> blockthinned = Morphology.ThinBlockUntilConvergence(thinned);
            Trace.WriteLine("Filling");
            IImage<bool> filled = Morphology.Fill(blockthinned);
            Trace.WriteLine("Connectivity");
            IImage<int> connectivity = BitImage.Analysis.Connectivity(filled);
            Trace.WriteLine("Connected components");
            IImage<int> components = BitImage.Analysis.ConnectedComponents(filled);
            Trace.WriteLine("Mapping faults");
            FaultNetwork network = FaultNetworkMapper.MapFaultNetwork(filled, horizon);
            Trace.WriteLine("Mapping displacements");
            FaultDisplacementMapper displacementMapper = new FaultDisplacementMapper(network);

            var mesh = displacementMapper.GetResult();

            // Output files of mesh
            Trace.WriteLine("Writing faults");
            string faultSegmentsPath = faultProbabilityUri.LocalPath.Replace(".grd", "_faults.poly");
            string monoSegmentsPath = faultProbabilityUri.LocalPath.Replace(".grd", "_mono.poly");
            using (StreamWriter faultFile = new StreamWriter(faultSegmentsPath))
            using (StreamWriter monoFile = new StreamWriter(monoSegmentsPath))
            {
                foreach (EdgeBase edge in mesh.Edges)
                {
                    StreamWriter file = edge is FaultEdge ? faultFile : monoFile;
                    Point2D source = ((PositionedVertexBase) edge.Source).Position;
                    Point2D target = ((PositionedVertexBase) edge.Target).Position;
                    file.WriteLine("{0}\t{1}", source.X, source.Y);
                    file.WriteLine("{0}\t{1}", target.X, target.Y);
                    file.WriteLine("%");
                }
            }

            // Establish order in the mesh
            Trace.WriteLine("Build planar subdivision - Ordering mesh and inserting faces");
            var orderer = new Orderer2D<PositionedVertexBase, EdgeBase, FaceBase>(mesh);
            var orderedMesh = orderer.GetResult();
            Debug.Assert(orderedMesh.Euler == 2);

            // Triangulate the mesh
            // Copy the list of monotone faces, so we can iterate over it it
            // whilst modifying the faces in the mesh during triangulation.
            Trace.WriteLine("Triangulating");
            List<FaceBase> faces = new List<FaceBase>(orderedMesh.Faces);
            foreach (FaceBase face in faces)
            {
                var triangulator = new MonotonePolygonTriangulator<PositionedVertexBase, EdgeBase, FaceBase>(orderedMesh, face);
                triangulator.GetResult();
            }

            // Improve triangulation quality
            var improver = new TriangulationQualityImprover<PositionedVertexBase, EdgeBase, FaceBase>(mesh); // TODO: Add a flippable critera
            improver.Improve();

            Trace.WriteLine("Writing mesh");
            // Output the mesh
            Random rng = new Random();
            string facesPath = faultProbabilityUri.LocalPath.Replace(".grd", "_faces.poly");
            using (StreamWriter facesFile = new StreamWriter(facesPath))
            {
                // All faces except the last one...
                foreach (FaceBase face in orderedMesh.Faces.Take(orderedMesh.FaceCount - 1))
                {
                    foreach (VertexBase vertex in face.Vertices)
                    {
                        PositionedVertexBase pos = (PositionedVertexBase) vertex;
                        Point2D point = pos.Position;
                        facesFile.WriteLine("{0}\t{1}", point.X, point.Y);
                    }
                    int red = rng.Next(255);
                    int green = rng.Next(255);
                    int blue = rng.Next(255);
                    facesFile.WriteLine("% -W0/{0}/{1}/{2} -G{0}/{1}/{2}", red, green, blue);
                }          
            }

            // Convert images to grids for convenient output

            //GridImage filteredGrid = (GridImage) pFaultImageRidge.Clone();
            //for (int i = 0; i < filteredGrid.Width; ++i)
            //{
            //    for (int j = 0; j < filteredGrid.Height; ++j)
            //    {
            //        filteredGrid[i, j] = filtered[i, j] ? 1.0 : 0.0;
            //    }
            //}

            //GridImage closedGrid = (GridImage) pFaultImageRidge.Clone();
            //for (int i = 0; i < closedGrid.Width; ++i)
            //{
            //    for (int j = 0; j < closedGrid.Height; ++j)
            //    {
            //        closedGrid[i, j] = closed[i, j] ? 1.0 : 0.0;
            //    }
            //}

            //GridImage thinnedGrid = (GridImage) pFaultImageRidge.Clone();
            //for (int i = 0; i < thinnedGrid.Width; ++i)
            //{
            //    for (int j = 0; j < thinnedGrid.Height; ++j)
            //    {
            //        thinnedGrid[i, j] = thinned[i, j] ? 1.0 : 0.0;
            //    }
            //}

            //GridImage blockThinnedGrid = (GridImage) pFaultImageRidge.Clone();
            //for (int i = 0; i < blockThinnedGrid.Width; ++i)
            //{
            //    for (int j = 0; j < blockThinnedGrid.Height; ++j)
            //    {
            //        blockThinnedGrid[i, j] = blockthinned[i, j] ? 1.0 : 0.0;
            //    }
            //}

            GridImage filledGrid = (GridImage) pFaultImageRidge.Clone();
            for (int i = 0; i < filledGrid.Width; ++i)
            {
                for (int j = 0; j < filledGrid.Height; ++j)
                {
                    filledGrid[i, j] = filled[i, j] ? 1.0 : 0.0;
                }
            }

            //GridImage connectivityGrid = (GridImage) pFaultImageRidge.Clone();
            //for (int i = 0; i < filledGrid.Width; ++i)
            //{
            //    for (int j = 0; j < filledGrid.Height; ++j)
            //    {
            //        connectivityGrid[i, j] = connectivity[i, j];
            //    }
            //}

            //GridImage componentsGrid = (GridImage) pFaultImageRidge.Clone();
            //for (int i = 0; i < componentsGrid.Width; ++i)
            //{
            //    for (int j = 0; j < componentsGrid.Height; ++j)
            //    {
            //        componentsGrid[i, j] = components[i, j];
            //    }
            //}

            //string pathX = faultProbabilityUri.LocalPath.Replace(".", "_x.");
            //string pathY = faultProbabilityUri.LocalPath.Replace(".", "_y.");
            //string pathXX = faultProbabilityUri.LocalPath.Replace(".", "_xx.");
            //string pathYX = faultProbabilityUri.LocalPath.Replace(".", "_yy.");
            //string pathXY = faultProbabilityUri.LocalPath.Replace(".", "_xy.");
            //string pathBeta = faultProbabilityUri.LocalPath.Replace(".", "_beta.");
            //string pathPQ = faultProbabilityUri.LocalPath.Replace(".", "_pq.");
            //string pathP = faultProbabilityUri.LocalPath.Replace(".", "_p.");
            //string pathQ = faultProbabilityUri.LocalPath.Replace(".", "_q.");
            //string pathPP = faultProbabilityUri.LocalPath.Replace(".", "_pp.");
            //string pathQQ = faultProbabilityUri.LocalPath.Replace(".", "_qq.");
            //string pathRidge = faultProbabilityUri.LocalPath.Replace(".", "_ridge.");
            //string pathFiltered = faultProbabilityUri.LocalPath.Replace(".", "_filtered.");
            //string pathClosed = faultProbabilityUri.LocalPath.Replace(".", "_closed.");
            //string pathThinned = faultProbabilityUri.LocalPath.Replace(".", "_thinned.");
            //string pathBlockThinned = faultProbabilityUri.LocalPath.Replace(".", "_blockthinned.");
            string pathFilled = faultProbabilityUri.LocalPath.Replace(".", "_filled.");
            //string pathConnectivity = faultProbabilityUri.LocalPath.Replace(".", "_connectivity.");
            //string pathComponents = faultProbabilityUri.LocalPath.Replace(".", "_components.");
            //string pathFaultLines = faultProbabilityUri.LocalPath.Replace(".grd", "_faults.poly");
            string pathBisectors = faultProbabilityUri.LocalPath.Replace(".grd", "_bisectors.poly");
            string pathLabels = faultProbabilityUri.LocalPath.Replace(".grd", "_labels.xy");
            string pathStrands = faultProbabilityUri.LocalPath.Replace(".grd", "_strands.poly");

            //pFaultImageX.Grid.WriteSurfer6BinaryFile(pathX);
            //pFaultImageY.Grid.WriteSurfer6BinaryFile(pathY);
            //pFaultImageXX.Grid.WriteSurfer6BinaryFile(pathXX);
            //pFaultImageYY.Grid.WriteSurfer6BinaryFile(pathXY);
            //pFaultImageXY.Grid.WriteSurfer6BinaryFile(pathXY);
            //pFaultImageBeta.Grid.WriteSurfer6BinaryFile(pathBeta);
            //pFaultImagePQ.Grid.WriteSurfer6BinaryFile(pathPQ);
            //pFaultImageP.Grid.WriteSurfer6BinaryFile(pathP);
            //pFaultImageQ.Grid.WriteSurfer6BinaryFile(pathQ);
            //pFaultImagePP.Grid.WriteSurfer6BinaryFile(pathPP);
            //pFaultImageQQ.Grid.WriteSurfer6BinaryFile(pathQQ);
            //pFaultImageRidge.Grid.WriteSurfer6BinaryFile(pathRidge);
            //filteredGrid.Grid.WriteSurfer6BinaryFile(pathFiltered);
            //closedGrid.Grid.WriteSurfer6BinaryFile(pathClosed);
            //thinnedGrid.Grid.WriteSurfer6BinaryFile(pathThinned);
            //blockThinnedGrid.Grid.WriteSurfer6BinaryFile(pathBlockThinned);
            filledGrid.Grid.WriteSurfer6BinaryFile(pathFilled);
            //connectivityGrid.Grid.WriteSurfer6BinaryFile(pathConnectivity);
            //componentsGrid.Grid.WriteSurfer6BinaryFile(pathComponents);
            //mapper.OutputPolygons(pathFaultLines);
            //displacementMapper.OutputBisectors(pathBisectors);
            //displacementMapper.OutputLabels(pathLabels);
            //displacementMapper.OutputStrands(pathStrands);
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
