/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-19
 * Time: 00:05
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace Geodyssey
{
	/// <summary>
	/// Description of LinearFaultGeneVisitor.
	/// </summary>
	public interface ILinearFaultGeneVisitor
	{
	    void Visit(LinearFaultGene lfg);
	}
}
