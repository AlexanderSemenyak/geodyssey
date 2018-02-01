/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-22
 * Time: 15:24
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Wintellect.PowerCollections;

using Numeric;

namespace Geodyssey
{
	/// <summary>
	/// Description of Genome.
	/// </summary>
	public class Genotype : ICloneable
	{
        private const int minimumGenomeLength = 2;
	    private BigList<Gene> genes = new BigList<Gene>();
	    
	    #region Construction 
	    
	    public Genotype(Domain domain) :
	        this(domain, 0)
		{
		}
		
		public Genotype(Domain domain, int size)
		{
            Debug.Assert(size == 0 || size >= 2);
		    for(int i = 0; i < size; ++i)
		    {
		        genes.Add(domain.CreateRandomGene());
		    }
		}
		
		public Genotype(Genotype other)
		{
		    foreach (Gene gene in other.genes)
		    {
		        genes.Add((Gene)gene.Clone());
		    }
		}
		
		private Genotype(BigList<Gene> genes)
		{
		    this.genes = genes;    
		}
		
		#endregion
		
		#region Properties
		public int Count
		{
		    get { return genes.Count; }
		}
		#endregion
		
		#region Methods
		public Phenotype Express(Domain domain)
		{
		    ExpressionVisitor ev = domain.CreateExpressionVisitor();
		    foreach (Gene gene in genes)
		    {
		        gene.Accept(ev);
		    }
		    return ev.Pheno;
		}
		
		public static Pair<Genotype, Genotype> Splice(Genotype genome1, Genotype genome2, int splicePoint1, int splicePoint2)
		{
		    // TODO Here we use an inefficient version which creates two containers of genes
		    // to replace the originals.  Ideally, this would be done as it was in the
		    // C++ version with std::list splicing - or by implementing this class in C++/CLR

            Debug.Assert(genome1.Count >= 2);
            Debug.Assert(genome2.Count >= 2);
            Debug.Assert(splicePoint1 < genome1.Count);
            Debug.Assert(splicePoint2 < genome2.Count);

		    BigList<Gene> newGenes1 = new BigList<Gene>();
		    BigList<Gene> newGenes2 = new BigList<Gene>();
		    IEnumerator<Gene> p = genome1.genes.GetEnumerator();
		    for(int i = 0; i < splicePoint1; ++i)
		    {
		        p.MoveNext();
		        newGenes1.Add((Gene) p.Current.Clone());
		    }
		    
		    IEnumerator<Gene> q = genome2.genes.GetEnumerator();
		    for(int j = 0; j < splicePoint2; ++j)
		    {
		        q.MoveNext();
		        newGenes2.Add((Gene) q.Current.Clone());
		    }
		    
		    for(int i = splicePoint1; i < genome1.genes.Count; ++i)
		    {
		        p.MoveNext();
		        newGenes2.Add((Gene) p.Current.Clone());
		    }
		    
		    for(int j = splicePoint2; j < genome2.genes.Count; ++j)
		    {
		        q.MoveNext();
		        newGenes1.Add((Gene) q.Current.Clone());
		    }

            Debug.Assert(newGenes1.Count + newGenes2.Count == genome1.Count + genome2.Count);
		    return new Pair<Genotype, Genotype>(new Genotype(newGenes1), new Genotype(newGenes2));
		}
		 
		public void Mutate()
		{
		    int chosen_gene = Rng.DiscreteUniformZeroToN(genes.Count - 1);
		    genes[chosen_gene].Mutate();
		}
		
		public void InvertMutate()
		{
		    // Select a region of the Genome and invert it
		    // Mutation equation Vm = V * s * r * 2^(-uk)
    		//   where s = sign = {-1, +1) uniform random
    		//   r = range
    		//   u = [0..1] uniform random
    		//   k = precision
    		double resolution = 1; // The smallest number of steps
    		double range = genes.Count - 1;
    		double k = - Math.Log(resolution / range) / Math.Log(2.0);
    		double u = Rng.ContinuousUniformZeroToOne();
    		double s =  range * Math.Pow(2.0, -u * k);
    		int steps = (int)Math.Round(s);
    		int length = 1 + steps;
    		Debug.Assert(length >= 2);
    		Debug.Assert(length <= genes.Count);
    		int start = (length < genes.Count) ? start = Rng.DiscreteUniformZeroToN(genes.Count - length) : 0;
    		Debug.Assert(start < genes.Count - 1);
    		int i = start;
    		int j = start + steps;
    		while(i < j)
    		{
    		    Gene temp = genes[i];
    		    genes[i] = genes[j];
    		    genes[j] = temp;
    		    i++;
    		    j--;
    		}
		}
		
        /// <summary>
        /// Duplicate a single gene
        /// </summary>
        public void DuplicateMutate()
        {
            int offset = Rng.DiscreteUniformZeroToN(genes.Count - 1);
            Gene copy = (Gene) genes[offset].Clone();
            copy.Mutate();
            genes.Insert(offset, copy);
        }

        public void DeleteMutate()
        {
            if (genes.Count > minimumGenomeLength)
            {
                int offset = Rng.DiscreteUniformZeroToN(genes.Count - 1);
                genes.RemoveAt(offset);
            }
        }

		public object Clone()
		{
		    return new Genotype(this);
		}
		
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("START GENOME (");
            sb.Append(genes.Count);
            sb.Append(")\n");
            foreach (Gene gene in genes)
            {
                sb.Append(gene.ToString());
            }
            sb.Append("END GENOME\n");
            return sb.ToString();
        }
		#endregion
	}
}
