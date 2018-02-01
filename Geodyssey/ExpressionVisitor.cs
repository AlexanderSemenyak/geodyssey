/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-29
 * Time: 18:39
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace Geodyssey
{
	/// <summary>
	/// Expresses genes onto a Phenotype.
	/// </summary>
	public abstract class ExpressionVisitor : GeneVisitor
	{
		public ExpressionVisitor()
		{
		}
		
		public abstract Phenotype Pheno
		{
		    get;
		}
	}
}
