/*
 *
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-22
 * Time: 17:50
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Text;
using System.Diagnostics;
using Wintellect.PowerCollections;

using Numeric;

namespace Geodyssey
{
	/// <summary>
	/// Description of Individual.
	/// </summary>
	public class Individual : ICloneable
	{
	    private static int sid = -1;
	    private int id = ++sid;
	    private Domain domain;
	    private double[] fitness = null;
        private int paretoRank = 0; // Zero marks as unset since valid paretoRank >= 1
	    private Genotype genotype;
        private WeakReference<Phenotype> phenotype = new WeakReference<Phenotype>(null); 
	    
	    #region Construction
	        
	    public Individual(Domain domain) :
	        this(domain, 0)
		{
		}
		
		public Individual(Domain domain, int initialGenomeSize)
		{
	        this.domain = domain;
		    this.genotype = new Genotype( domain, initialGenomeSize );
            
		}
		
		public Individual(Individual other)
		{
		    this.domain = other.domain;
		    this.fitness = other.fitness;
		    this.genotype = (Genotype) other.genotype.Clone();
            // Should probably have clonable Phenotypes
		}
		
		private Individual(Domain domain, Genotype genotype)
		{
		    this.domain = domain;
		    this.genotype = genotype;
		}
		
		#endregion
		
		#region Properties
		
		public int Id
		{
		    get { return id; }
		}
		
		public double[] Fitness
		{
		    get
		    {
		        if (fitness == null)
		        {
		            MeasureFitness();
		            ClearPhenotype();
		        }
		        return fitness;
		    }
		}

        public int ParetoRank
        {
            get { return paretoRank; }
            set
            {
                Debug.Assert(value >= 1);
                paretoRank = value;
            }
        }

		public Genotype Genome
		{
		    get { return genotype; }
		}
		
		public Phenotype Phenome
		{
		    get
		    {
                // phenotype is a weak reference we must be
                // sure here to return a strong reference so it
                // is not deleteded whilst it is in use.
                if (phenotype.IsAlive)
                {
                    return phenotype.Target;
                }
                Phenotype strongPhenotype = genotype.Express(domain);
                phenotype.Target = strongPhenotype;
		        return strongPhenotype;
		    }
		}
		
		#endregion
		
		#region Methods
		
		public void MeasureFitness()
		{
		    fitness = domain.Fitness(Phenome);
		}
		
		public void Mutate()
		{
		    Dirty();
		    genotype.Mutate();
		}
		
		public void InvertMutate()
		{
		    Dirty();
		    genotype.InvertMutate();
		}

        public void DuplicateMutate()
        {
            Dirty();
            genotype.DuplicateMutate();
        }

        public void DeleteMutate()
        {
            Dirty();
            genotype.DeleteMutate();
        }
		
		public static Pair<Individual, Individual> Crossover(Individual mother, Individual father)
		{
            Debug.Assert(mother.Genome.Count >= 2);
            Debug.Assert(father.Genome.Count >= 2);
		    int splicePointMother = 1 + Rng.DiscreteUniformZeroToN(mother.genotype.Count - 2);
		    int splicePointFather = 1 + Rng.DiscreteUniformZeroToN(father.genotype.Count - 2);
            Debug.Assert(splicePointMother > 0 && splicePointMother < mother.genotype.Count);
            Debug.Assert(splicePointFather > 0 && splicePointFather < father.genotype.Count);
		    Pair<Genotype, Genotype> childGenes = Genotype.Splice(mother.genotype, father.genotype, splicePointMother, splicePointFather);
		    Individual childOne = new Individual(mother.domain, childGenes.First);
		    Individual childTwo = new Individual(mother.domain, childGenes.Second);
		    return new Pair<Individual, Individual>(childOne, childTwo);
		}
				
		private void Dirty()
		{
		    fitness = null;
		    phenotype.Target = null;
		}
		
		public void ClearPhenotype()
		{
		    phenotype.Target = null;
		}
		
		public object Clone()
		{
		    return new Individual(this);
		}
		
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("INDIVIDUAL\n");
            sb.AppendFormat("  Fitness = {0}\n", Fitness);
            sb.Append(Genome.ToString());
            sb.Append("END INDIVIDUAL\n");
            return sb.ToString();
        }
		
		#endregion
	

	}
}
