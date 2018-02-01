/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-22
 * Time: 23:28
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;

using Model;
using Loaders;

namespace Geodyssey
{
	/// <summary>
	/// Description of GeoGnome.
	/// </summary>
	public static class GeodysseyApp
	{
	    static int Main(string[] args)
	    {
            // Get a URI from the command line argument
            Uri uri;
            try
            {
                uri = new Uri(args[0]);
            }
            catch (UriFormatException)
            {
                string absPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + args[0];
                try
                {
                    uri = new Uri(absPath);
                }
                catch (UriFormatException)
                {
                    return 1;
                }
            }

            ReportLogger.Instance.Create("report.xml");
            ReportLogger.Instance.Writer.WriteStartDocument();
            ReportLogger.Instance.Writer.WriteStartElement("GeodysseyRun");

	        Stopwatch overallWatch = new Stopwatch();
	        overallWatch.Start();
	        //Console.WriteLine("Start!");
	        int initialMinimumGenomeLength = 50;
            int initialMaximumGenomeLength = 100;
	        int initialPopulationSize = 1000;
	        int finalPopulationSize = 100;
	        int numberOfGenerations = 100;
	        
	        double populationPowerBase = Math.Pow((double) finalPopulationSize / (double) initialPopulationSize, 1.0 / numberOfGenerations);
	        
	        int totalNumberOfIndividuals = 0;
	        // Determine how many individuals will be produced in this run
	        for (int i = 0 ; i < numberOfGenerations; ++i)
	        {
	            int inc = (int) Math.Round(initialPopulationSize * Math.Pow(populationPowerBase, i + 1));
	            //Console.WriteLine(inc);
	            totalNumberOfIndividuals += inc;  
	        }
	        Console.WriteLine("Total Number of Individuals = {0}", totalNumberOfIndividuals);
	        Console.WriteLine("Mean Invididuals per Generation = {0}", totalNumberOfIndividuals / numberOfGenerations);
	        
            // Load a target grid
            //RegularGrid2D targetGrid;
            //using (StreamReader reader = File.OpenText("target.grd"))
            //{
            //    targetGrid = RegularGrid2D.Create(reader);
            //}

            GeodysseyModel model = new GeodysseyModel();
            LoaderController.Instance.Open(uri, model);
            IRegularGrid2D targetGrid = model[0]; // The first grid

            Debug.Assert(targetGrid != null);
            Grid2DDomain domain = new Grid2DDomain(targetGrid);
	        
	        Stopwatch stopWatch = new Stopwatch();
	        Population population = new Population(domain, initialPopulationSize, initialMinimumGenomeLength, initialMaximumGenomeLength);
            for (int i = 0 ; i < numberOfGenerations ; ++i)
            {
                stopWatch.Reset();
                stopWatch.Start();
                population.ComputeFitnesses();
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                ReportLogger.Instance.Writer.WriteStartElement("Generation");
                ReportLogger.Instance.Writer.WriteStartAttribute("number");
                ReportLogger.Instance.Writer.WriteValue(i + 1);
                ReportLogger.Instance.Writer.WriteEndAttribute();
                ReportLogger.Instance.Writer.WriteStartAttribute("elapsedTime");
                ReportLogger.Instance.Writer.WriteValue(ts);
                ReportLogger.Instance.Writer.WriteEndAttribute();
                RecordBest(targetGrid, population, i, domain);
                population.LogSummaryStatistics();
                ReportLogger.Instance.Writer.WriteEndElement();
                Console.WriteLine("Generation {0} of {1} with size {2} in {3}) ", i + 1, numberOfGenerations, population.Count, ts);
	            int nextGenerationSize = (int) Math.Round(initialPopulationSize * Math.Pow(populationPowerBase, i + 1));
	            population.Evolve(nextGenerationSize);
	        }
            population.ComputeFitnesses();
            RecordBest(targetGrid, population, numberOfGenerations, domain);
            Console.WriteLine("Generation {0} of {1} with size {2}) ", numberOfGenerations, numberOfGenerations, population.Count);
            overallWatch.Stop();
	        Console.WriteLine("Running time : {0}", overallWatch.Elapsed);
            ReportLogger.Instance.Writer.WriteEndElement();
            ReportLogger.Instance.Writer.WriteEndDocument();
            ReportLogger.Instance.Close();
            Console.ReadKey();
            return 0;
	    }

        private static void RecordBest(IRegularGrid2D targetGrid, Population population, int generation, RectangularDomain domain)
        {
            List<Individual> best = population.Best;
            for (int j = 0 ; j < best.Count ; ++j)
            {
                Console.Write("best[{0}] = [", j);
                foreach (double f in best[j].Fitness)
                {
                    Console.Write(f);
                    Console.Write(" ");
                }
                Console.WriteLine("] with {0} genes", best[j].Genome.Count);

                // Express the genome to create a grid
                Individual bestIndividual = (Individual)best[j].Clone();
                Grid2DPhenotype bestPhenotype = (Grid2DPhenotype) bestIndividual.Phenome;
                bestPhenotype.Grid.MaskFrom(targetGrid);
                StringBuilder gridFileName = new StringBuilder();
                gridFileName.AppendFormat("evolved_{0}_{1}.grd", generation + 1, j);
                bestPhenotype.Grid.WriteSurfer6BinaryFile(gridFileName.ToString());
                ReportLogger.Instance.Writer.WriteStartElement("Grid");
                ReportLogger.Instance.Writer.WriteStartAttribute("file");
                ReportLogger.Instance.Writer.WriteValue(gridFileName.ToString());
                ReportLogger.Instance.Writer.WriteEndAttribute();
                ReportLogger.Instance.Writer.WriteEndElement();

                // Express the genome to create a fault polygon map
                OutlineMapDomain mapDomain = new OutlineMapDomain(domain);
                OutlineMapPhenotype mapPhenotype = (OutlineMapPhenotype) bestIndividual.Genome.Express(mapDomain);
                StringBuilder polygonFileName = new StringBuilder();
                polygonFileName.AppendFormat("evolved_{0}_{1}.poly", generation + 1, j);
                using (StreamWriter writer = File.CreateText(polygonFileName.ToString()))
                {
                    writer.Write(mapPhenotype.ToString());
                }
                ReportLogger.Instance.Writer.WriteStartElement("Polygons");
                ReportLogger.Instance.Writer.WriteStartAttribute("file");
                ReportLogger.Instance.Writer.WriteValue(polygonFileName.ToString());
                ReportLogger.Instance.Writer.WriteEndAttribute();
                ReportLogger.Instance.Writer.WriteEndElement();
            }
        }
	}
}
