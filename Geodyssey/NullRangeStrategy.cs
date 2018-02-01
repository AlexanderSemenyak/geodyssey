/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-21
 * Time: 16:34
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;

namespace Geodyssey
{
	/// <summary>
	/// Description of NullRangeCheckStrategy.
	/// </summary>
	public class NullRangeStrategy : AbstractRangeStrategy
	{
	    private static Dictionary<Pair<double, double>, PowerLawMutationStrategy> instances = new Dictionary<Pair<double, double>, PowerLawMutationStrategy>();
	    
	    private static NullRangeStrategy instance = new NullRangeStrategy();
	    
	    #region CONSTRUCTION
	    
	    // Memoizing constructor
	    public static NullRangeStrategy Create()
	    {
	        return instance;
	    }
	    
		private NullRangeStrategy()
		{
		}
		
		#endregion
		
		public override double Check(double value)
        {
            return value;
        }
	}
}
