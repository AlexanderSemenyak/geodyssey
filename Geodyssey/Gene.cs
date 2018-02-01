/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-18
 * Time: 23:34
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Diagnostics;

using Numeric;

namespace Geodyssey
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	public abstract class Gene : ICloneable
	{
	    private Grid2DDomain domain;
	    private Dimension[] dimensions;
	    
	    public class DimensionIndexer
	    {
	        private Gene owner;
	        
	        public DimensionIndexer(Gene owner)
	        {
	            this.owner = owner;
	        }
	        
	        public Dimension this[int index]
	        {
	            get { return owner.dimensions[index];  }
	            set { owner.dimensions[index] = value; }
	        }
	        
	        public int Length
	        {
	            get { return owner.dimensions.Length; }
	        }
	    }
	    
	    private DimensionIndexer dimensionsIndexer;
	    
	    #region Construction
	    
		public Gene(Grid2DDomain domain, int size)
		{
		    Debug.Assert(domain != null);
		    this.domain = domain;
		    dimensions = new Dimension[size];
		    dimensionsIndexer = new DimensionIndexer(this);
		}
		
		public Gene(Gene other) :
		    this(other.domain, other.Count)
		{
		    for (int i = 0; i < other.Count ; ++i)
		    {
		        this.dimensions[i] = (Dimension)other.dimensions[i].Clone();
		    }
		}
		
		#endregion
		
		#region Properties
		
		public int Count
		{
		    get { return dimensions.Length; }
		}
		
		protected DimensionIndexer Dimensions
		{
		    get { return dimensionsIndexer; }
		}
		
		#endregion
		
        #region Methods
		
		public void Mutate()
		{
		    int index = Rng.DiscreteUniformZeroToN(Count - 1);
		    dimensions[index].Mutate();
		}
		
		public abstract bool Accept(GeneVisitor geneVisitor);
		
		public abstract object Clone();
		
		#endregion
	}
}
