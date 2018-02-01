/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-05-02
 * Time: 23:25
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;

namespace Geodyssey
{
	/// <summary>
	/// Description of ReflectRangeStrategy.
	/// </summary>
	public class ReflectRangeStrategy : AbstractRangeStrategy
	{
	    private double min;
	    private double max;
	    
	    private static Dictionary<Pair<double, double>, ReflectRangeStrategy> instances = new Dictionary<Pair<double, double>, ReflectRangeStrategy>();
	    
	    // Memoizing constructor
	    public static ReflectRangeStrategy Create(double min, double max)
	    {
	        // TODO: This method may require synchronisation for the static instances
	        Pair<double, double> key = new Pair<double, double>(min, max);
	        if (instances.ContainsKey(key))
	        {
	            return instances[key];
	        }
	        ReflectRangeStrategy instance = new ReflectRangeStrategy(min, max);
	        instances.Add(key, instance);
	        return instance;
	    }
	    
		private ReflectRangeStrategy(double min, double max)
		{
		    this.min = min;
		    this.max = max;
		}
		
        public override double Check(double value)
        {
            // TODO: Find a not iterative way to do this
            while (value > max || value < min)
            {
                if (value > max)
                {
                    double overshoot = value - max;
                    value = max - overshoot;
                }
                if (value < min)
                {
                    double undershoot = min - value;
                    value = min + undershoot;
                }
            }
            return value;
        }
	}
}
