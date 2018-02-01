/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-29
 * Time: 12:34
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;

namespace Geodyssey
{
	/// <summary>
	/// Description of ClipRangeStrategy.
	/// </summary>
	public class ClipRangeStrategy : AbstractRangeStrategy
	{
		private double min;
	    private double max;
	    
	    private static Dictionary<Pair<double, double>, ClipRangeStrategy> instances = new Dictionary<Pair<double, double>, ClipRangeStrategy>();
	    
	    // Memoizing constructor
	    public static ClipRangeStrategy Create(double min, double max)
	    {
	        // TODO: This method may require synchronisation for the static instances
	        Pair<double, double> key = new Pair<double, double>(min, max);
	        if (instances.ContainsKey(key))
	        {
	            return instances[key];
	        }
	        ClipRangeStrategy instance = new ClipRangeStrategy(min, max);
	        instances.Add(key, instance);
	        return instance;
	    }
	    
		private ClipRangeStrategy(double min, double max)
		{
		    this.min = min;
		    this.max = max;
		}
		
        public override double Check(double value)
        {
            return Math.Min(Math.Max(min, value), max);
        }
	}
}
