/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-29
 * Time: 15:13
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace Geodyssey
{
	/// <summary>
	/// Description of Domain.
	/// </summary>
	public abstract class Domain
	{
	    private Phenotype target;
	    
	    #region Construction
	    
	    protected Domain()
	    {
	    }
	    
		protected Domain(Phenotype target)
		{
		    this.target = target;
		}
		
		#endregion
		
		#region Properties
		public Phenotype Target
		{
		    get { return target;  }
		    set { target = value; }
		}
		#endregion
		
		#region Methods
		
		public abstract Gene CreateRandomGene();
		public abstract Phenotype CreatePhenotype();
		public abstract ExpressionVisitor CreateExpressionVisitor();
		
        /// <summary>
        /// Compute the multiobjective Fitness vector
        /// </summary>
        /// <param name="trial">The trial Phenotype of which to compute the fitness</param>
        /// <returns>A multiobjective fitness vector.</returns>
		public abstract double[] Fitness(Phenotype trial);
		
		#endregion
	}
}
