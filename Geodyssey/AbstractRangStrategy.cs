/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-21
 * Time: 15:13
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace Geodyssey
{
	/// <summary>
	/// Description of AbstractRangeCheckStrategy.
	/// </summary>
	public abstract class AbstractRangeStrategy
	{
		public AbstractRangeStrategy()
		{
		}
		
		public abstract double Check(double value);
	}
}
