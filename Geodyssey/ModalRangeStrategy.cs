/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-21
 * Time: 15:14
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;

namespace Geodyssey
{
	/// <summary>
	/// Description of ModalRangeCheckStrategy.
	/// </summary>
	public class ModalRangeStrategy : AbstractRangeStrategy
	{
	    private double min;
	    private double max;
	    
	    private static Dictionary<Pair<double, double>, ModalRangeStrategy> instances = new Dictionary<Pair<double, double>, ModalRangeStrategy>();
	    
	    // Memoizing constructor
	    public static ModalRangeStrategy Create(double min, double max)
	    {
	        // TODO: This method may require synchronisation for the static instances
	        Pair<double, double> key = new Pair<double, double>(min, max);
	        if (instances.ContainsKey(key))
	        {
	            return instances[key];
	        }
	        ModalRangeStrategy instance = new ModalRangeStrategy(min, max);
	        instances.Add(key, instance);
	        return instance;
	    }
	    
		private ModalRangeStrategy(double min, double max)
		{
		    this.min = min;
		    this.max = max;
		}
		
        public override double Check(double value)
        {
            // TODO: Verify this formula
            return min + (value - min) % (max - min);
        }
	}
}
