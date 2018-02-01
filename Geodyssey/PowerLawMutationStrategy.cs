/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-21
 * Time: 14:55
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;

using Numeric;

namespace Geodyssey
{
	/// <summary>
	/// Description of PowerLawMutationStrategy.
	/// </summary>
	public class PowerLawMutationStrategy : AbstractMutationStrategy
	{
	    private static Dictionary<Pair<double, double>, PowerLawMutationStrategy> instances = new Dictionary<Pair<double, double>, PowerLawMutationStrategy>();
	    
	    private double r;  // Maximum mutation range
	    private double k;  // Mutation precision - related to smallest delta
	    
	    #region Construction
	    
	    // Memoizing constructor
	    public static PowerLawMutationStrategy Create(double range, double resolution)
	    {
	        // TODO: This method may require synchronisation for the static instances
	        Pair<double, double> key = new Pair<double, double>(range, resolution);
	        if (instances.ContainsKey(key))
	        {
	            return instances[key];
	        }
	        PowerLawMutationStrategy instance = new PowerLawMutationStrategy(range, resolution);
	        instances.Add(key, instance);
	        return instance;
	    }
	    	    		
		private PowerLawMutationStrategy(double range, double resolution)
		{
		    this.r = range;
		    // Determine k from the resolution value by rearranging the mutation equation
		    this.k = - Math.Log(resolution / range) / Math.Log(2.0);
		}
		
		#endregion
		
		#region Methods
		
		public override double Mutate(double value)
		{
		    // Mutation equation Vm = V * s * r * 2^(-uk)
    		//   where s = sign = {-1, +1) uniform random
    		//   r = range
    		//   u = [0..1] uniform random
    		//   k = precision
    		int toss = Rng.DiscreteUniformZeroOrOne();
    		int sign = (toss == 0) ? -1 : +1;
    	
    		double u = Rng.ContinuousUniformZeroToOne();
    
    		value += sign * r * Math.Pow(2.0, -u * k);
    		return value;
		}
		
		#endregion
		
	}
}
