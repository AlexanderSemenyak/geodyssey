/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-21
 * Time: 14:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace Geodyssey
{
	/// <summary>
	/// Description of AbstractMutationStrategy.
	/// </summary>
	public abstract class AbstractMutationStrategy
	{
		public AbstractMutationStrategy()
		{
		}
		
		public abstract double Mutate(double value);
	}
}
