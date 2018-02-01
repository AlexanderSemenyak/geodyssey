/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-22
 * Time: 18:20
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Diagnostics;

namespace Geodyssey
{
	/// <summary>
	/// Description of Phenotype.
	/// </summary>
	public abstract class Phenotype
	{
	    Domain domain;
	    
		public Phenotype(Domain domain)
		{
		    this.domain = domain;
		}
		
		/// <summary>
		/// Compute the multiobjective fitness vector
		/// </summary>
		/// <param name="other">Another Phenotype object against which this
        /// Phenotype is to be compared; usually the target phenotype.</param>
		/// <returns>A multiobjective fitness vector</returns>
		public abstract double[] Compare(Phenotype other);

        public Domain Domain
        {
            get { return domain;  }
            set { domain = value; }
        }
	}
}
