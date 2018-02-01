/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-22
 * Time: 18:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;
using Wintellect.PowerCollections;
using Amib.Threading;

using Numeric;

namespace Geodyssey
{
	/// <summary>
	/// Description of Population.
	/// </summary>
	public class Population
	{
	    private static double breedingProportion      = 0.6;
	    private static double valueMutationRate       = 0.5;
	    private static double inversionMutationRate   = 0.3;
        private static double duplicationMutationRate = 0.01;
        private static double deletionMutationRate    = 0.01;
	    
	    private Domain domain;
	    private List<Individual> members;
	    
		public Population(Domain domain)
		{
		    this.domain = domain;
		}
		
		public Population(Domain domain, int populationSize, int initialGenomeSize) :
		    this(domain)
		{
		    this.members = new List<Individual>(populationSize);
		    for (int i = 0; i < populationSize; ++i)
		    {
		        members.Add(new Individual(domain, initialGenomeSize));
		    }
		}

        public Population(Domain domain, int populationSize, int minimumGenomeSize, int maximumGenomeSize) :
            this(domain)
        {
            this.members = new List<Individual>(populationSize);
            for (int i = 0 ; i < populationSize ; ++i)
            {
                int initialGenomeSize = Rng.DiscreteUniform(minimumGenomeSize, maximumGenomeSize);
                //Console.WriteLine("initialGenomeSize = {0}", initialGenomeSize);
                members.Add(new Individual(domain, initialGenomeSize));
            }
        }

		#region Properties
		
		public int Count
		{
		    get { return members.Count; }
		}

        public List<Individual> Best
        {
            get
            {
                List<Individual> best = new List<Individual>();
                int i = 0;
                while ((i < members.Count) && (members[i].ParetoRank == members[0].ParetoRank) )
                {
                    Debug.Assert(members[1].ParetoRank >= 1);
                    best.Add(members[i]);
                    ++i;
                }
                return best;
            }
        }

		#endregion
		
		#region Methods
		
		public void Evolve()
		{
		    Evolve(members.Count);
		}
		
		public void Evolve(int newPopulationSize)
		{
		    ParetoStochasticUniversal(newPopulationSize);
		    Mutate();
		}
		
		private void ParetoStochasticUniversal(int newPopulationSize)
		{
		    List<Individual> newMembers = new List<Individual>(newPopulationSize);

		    // breedRanking stores a cumulative list of member.ParetoRank
		    List<int> breedRanking = new List<int>(members.Count);
            Debug.Assert(members[0].ParetoRank != 0);
		    breedRanking.Add(members[0].ParetoRank);
		    for (int i = 1; i < members.Count; ++i)
		    {
                Debug.Assert(members[i].ParetoRank != 0);
		        breedRanking.Add(breedRanking[i - 1] + members[i].ParetoRank);
		    }
		    
		    int breedingSubPopulationSize = (int) Math.Floor(newPopulationSize * breedingProportion);
		    //Console.WriteLine("breedingSubPopulationSize = {0}", breedingSubPopulationSize);
		    if (breedingSubPopulationSize > 0)
		    {
		        // Determine how far we jump down the breedRanking each time we select
		        double selectorIncrement = (double) breedRanking[breedRanking.Count - 1] / (double) breedingSubPopulationSize;
		        // Choose a random phase offset
		        double phase = Rng.ContinuousUniform(double.Epsilon, selectorIncrement, false);
		        double selector = phase;
		        List<Individual> breedingSubPopulation = new List<Individual>();
		        int i = 0;
		        while (i < members.Count)
		        {
		            if (selector < breedRanking[i])
		            {
		                breedingSubPopulation.Add(members[i]);
		                selector += selectorIncrement;
		            }
		            else
		            {
		                ++i;
		            }
		        }
		        Debug.Assert(breedingSubPopulation.Count == breedingSubPopulationSize);
		    
		        Algorithms.RandomShuffleInPlace(breedingSubPopulation);
		        int p_end = (breedingSubPopulation.Count % 2) == 1 ? ((breedingSubPopulation.Count / 2) + 1) : (breedingSubPopulation.Count / 2);  
		        for (int p = 0, q = breedingSubPopulation.Count - 1 ;
		             p < p_end ; ++p, --q)
		        {
		            Pair<Individual, Individual> children = Individual.Crossover(breedingSubPopulation[p], breedingSubPopulation[q]);
		            newMembers.Add(children.First);
		            newMembers.Add(children.Second);
		        }
		    }
		    
		    //Console.WriteLine("newMembers.Count = {0}", newMembers.Count);
		    
		    // Make up numbers in the newMembers generation by taking the best
		    // remaining individuals from the previous generation
		    IEnumerator<Individual> r = members.GetEnumerator();
		    while (newMembers.Count < newPopulationSize)
		    {
		        r.MoveNext();
		        newMembers.Add(r.Current);
		    }
		    
		    members = newMembers;
		    
		    //foreach (Individual individual in members)
		    //{
		    //    Console.Write(individual.Id);
		    //    Console.Write(' ');
		    //}
		    //Console.WriteLine();
		}

        /// <summary>
        /// Computes the multidimensional Pareto Rank for each individual with
        /// respect to the current population, and assignes the result to the
        /// ParetoRank property of each individual.
        /// </summary>
        private void ComputeParetoRanking()
        {
            // TODO: If profiling reveals this to be time-consuming, create a multi-threaded version

            // TODO: N^2 loop here - key point for optimization or approximation if
            //       there is a better algorithm
            foreach (Individual p in members)
            {
                // Initialize the ranking
                p.ParetoRank = 1;
                foreach (Individual q in members)
                {
                    // Don't bother checking for dominance against self
                    if (p == q)
                    {
                        continue;
                    }
                    Debug.Assert(p.Fitness.Length == q.Fitness.Length);
                    // The the hypothesis that all dimensions of p are less-than or equal to
                    // their counterparts in q
                    bool lte = true; // Assume that the hypothesis is true
                    for (int i = 0 ; i < p.Fitness.Length ; ++i)
                    {
                        if (! (p.Fitness[i] <= q.Fitness[i] ))
                        {
                            lte = false;
                            break;
                        }
                    }

                    if (lte)
                    {
                        // Test the hypothesis that at least one of the dimensions of q is
                        // strictly less-than this counterpart in q
                        bool lt = false; // Assume the hypothesis is false
                        for (int i = 0 ; i < p.Fitness.Length ; ++i)
                        {
                            if (p.Fitness[i] < q.Fitness[i])
                            {
                                lt = true;
                                break;
                            }
                        }

                        if (lt)
                        {
                            // Both hypotheses are true, therefore
                            // p has Pareto dominance over q
                            p.ParetoRank += 1;
                        }
                    }
                }
            }
        }

		private void Mutate()
		{
		    // Value mutations
		    foreach (Individual individual in members)
		    {
		        double u = Rng.ContinuousUniformZeroToOne();
		        if (u > (1.0 - valueMutationRate))
		        {
		            individual.Mutate();
		        }
		    }
		    
		    // Structural mutations
		    foreach (Individual individual in members)
		    {
		        double u = Rng.ContinuousUniformZeroToOne();
		        if(u > (1.0 - inversionMutationRate))
		        {
		            individual.InvertMutate();
		        }
		    }

            // Duplication mutations
            foreach (Individual individual in members)
            {
                double u = Rng.ContinuousUniformZeroToOne();
                if (u > (1.0 - duplicationMutationRate))
                {
                    individual.DuplicateMutate();
                }
            }

            // Deletion mutations
            foreach (Individual individual in members)
            {
                double u = Rng.ContinuousUniformZeroToOne();
                if (u > (1.0 - deletionMutationRate))
                {
                    individual.DeleteMutate();
                }
            }
		}

        public void ComputeFitnesses()
        {
            int numberOfThreadsX = System.Environment.ProcessorCount;

            if (numberOfThreadsX == 1)
            {
                // Single-threaded implementation
                foreach (Individual i in members)
                {
                    double[] fitness = i.Fitness;
                }
            }
            else
            {
                // Multithreaded implementation
                using (SmartThreadPool pool = new SmartThreadPool(1000, numberOfThreadsX, numberOfThreadsX))
                {
                    IWorkItemResult[] results = new IWorkItemResult[members.Count];
                    for (int i = 0 ; i < members.Count ; ++i)
                    {
                        WorkItemCallback workItem = new WorkItemCallback(this.ComputeFitness);
                        results[i] = pool.QueueWorkItem(workItem, members[i]);
                    }
                    SmartThreadPool.WaitAll(results);
                }
            }

            ComputeParetoRanking();

            // TODO: Ideally this code would be moved elsewhere
            // Sort in descending Pareto Rank order - we want the best
            // individuals at the front of the container
            members.Sort(delegate(Individual lhs, Individual rhs)
                          {
                              return rhs.ParetoRank.CompareTo(lhs.ParetoRank);
                          });
        }

        private object ComputeFitness(object obj)
        {
            Individual individual = (Individual) obj;
            double[] fitness = individual.Fitness;
            return fitness;
        }

        #endregion

        public void LogSummaryStatistics()
        {
            int minimumIndex = members.Count - 1;
            int maximumIndex = 0;
            int medianIndex = (int) Math.Round(minimumIndex / 2.0);
            int lowerQuartileIndex = (int) Math.Round(minimumIndex * 3.0/ 4.0);
            int upperQuartileIndex = (int) Math.Round(minimumIndex / 4.0);

            double[] minimum = members[minimumIndex].Fitness;
            double[] lowerQuartile = members[lowerQuartileIndex].Fitness;
            double[] median = members[medianIndex].Fitness;
            double[] upperQuartile = members[upperQuartileIndex].Fitness;
            double[] maximum = members[maximumIndex].Fitness;

            XmlWriter writer = ReportLogger.Instance.Writer;
            writer.WriteStartElement("Population");
            writer.WriteStartAttribute("count");
            writer.WriteValue(Count);
            writer.WriteEndAttribute();
            writer.WriteStartElement("SummaryFitnessStatistics");

            writer.WriteStartElement("Minimum");
            foreach (double value in minimum)
            {
                writer.WriteStartElement("Fitness");
                writer.WriteStartAttribute("value");
                writer.WriteValue(value);
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("LowerQuartile");
            foreach (double value in lowerQuartile)
            {
                writer.WriteStartElement("Fitness");
                writer.WriteStartAttribute("value");
                writer.WriteValue(value);
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Median");
            foreach (double value in median)
            {
                writer.WriteStartElement("Fitness");
                writer.WriteStartAttribute("value");
                writer.WriteValue(value);
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("UpperQuartile");
            foreach (double value in upperQuartile)
            {
                writer.WriteStartElement("Fitness");
                writer.WriteStartAttribute("value");
                writer.WriteValue(value);
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Maximum");
            foreach (double value in maximum)
            {
                writer.WriteStartElement("Fitness");
                writer.WriteStartAttribute("value");
                writer.WriteValue(value);
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteEndElement(); // SummaryStatistics
            writer.WriteEndElement(); // Population
        }
    }
}
