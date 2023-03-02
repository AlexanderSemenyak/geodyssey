/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-29
 * Time: 11:20
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Diagnostics;
using System.Collections.Generic;
using Wintellect.PowerCollections;
using Troschuetz.Random;
using Troschuetz.Random.Distributions.Continuous;
using Troschuetz.Random.Distributions.Discrete;
using Troschuetz.Random.Generators;

namespace Numeric
{
	/// <summary>
	/// Description of Rng.
	/// </summary>
	public static class Rng
	{
	    // TODO: Thread safety issues MUST be considered for this class. We may need to create per-thread instances
	    //       if performance is an issue.
	    
	    private static MT19937Generator generator = new MT19937Generator();
	    private static Dictionary<Pair<int, int>, DiscreteUniformDistribution> discreteUniformGenerators= new Dictionary<Pair<int, int>, DiscreteUniformDistribution>();
	    private static Dictionary<Pair<double, double>, ContinuousUniformDistribution> continuousUniformGenerators = new Dictionary<Pair<double, double>, ContinuousUniformDistribution>();
	    private static DiscreteUniformDistribution zeroOrOne;
        private static ContinuousUniformDistribution zeroToOne;
	    
	    static Rng()
	    {
	        // Create a commonly-used and quicly locatable zeroOrOne distribution,
	        // and also reference it in the discreteUniformGeneratirs dictionary
	        zeroOrOne = new DiscreteUniformDistribution(generator);
            AlphaAndBeta(zeroOrOne, 0, 1);
	        discreteUniformGenerators.Add(new Pair<int, int>(0, 1), zeroOrOne);
	        
	        // Create a commonly-used and quickly locatable zeroToOne distribution, 
	        // and also reference it in the continuousUniformGenerators dictionary
	        zeroToOne = new ContinuousUniformDistribution(generator);
            AlphaAndBeta(zeroToOne, 0.0, 1.0);
	        continuousUniformGenerators.Add(new Pair<double, double>(0.0, 1.0), zeroToOne);
	    }

	    public static int DiscreteUniform(int min, int max, bool cache)
	    {
            if (min == max)
            {
                return min;
            }
            Debug.Assert(min < max);
	        DiscreteUniformDistribution uniform;
	        Pair<int, int> key = new Pair<int, int>(min, max);
	        if (!discreteUniformGenerators.ContainsKey(key))
	        {
	            uniform = new DiscreteUniformDistribution(generator);
                // Beta must always be greater than Alpha, so we set them this way round
                AlphaAndBeta(uniform, min, max);
	            if (cache)
	            {
	                discreteUniformGenerators.Add(key, uniform);
	            }
	        }
	        else
	        {
	            uniform = discreteUniformGenerators[key];
	        }
            Debug.Assert(uniform != null);
            Debug.Assert(uniform.Alpha == min);
            Debug.Assert(uniform.Beta == max);
	        int value = uniform.Next();
            Debug.Assert(value >= min && value <= max);
            return value;
	    }
	    
	    public static int DiscreteUniform(int min, int max)
	    {
            Debug.Assert(min <= max);
	        int value = DiscreteUniform(min, max, true);
            Debug.Assert(value >= min && value <= max);
            return value;
	    }
	    
	    public static int DiscreteUniformZeroToN(int n, bool cache)
	    {
            Debug.Assert(n >= 0);
	        int value = DiscreteUniform(0, n, cache);
            Debug.Assert(value >= 0 && value <= n);
            return value;
	    }
	    
	    public static int DiscreteUniformZeroToN(int n)
	    {
            Debug.Assert(n >= 0);
	        int value = DiscreteUniformZeroToN(n, true);
            Debug.Assert(value >= 0 && value <= n);
            return value;
	    }
	    
	    public static int DiscreteUniformZeroOrOne()
	    {
	        int value = zeroOrOne.Next();
            Debug.Assert(value == 0 || value == 1);
            return value;
	    }
	    
	    public static double ContinuousUniform(double min, double max, bool cache)
	    {
            if (min == max)
            {
                return min;
            }
            Debug.Assert(min < max);
	        ContinuousUniformDistribution uniform;
	        Pair<double, double> key = new Pair<double, double>(min, max);
	        if(!continuousUniformGenerators.ContainsKey(key))
	        {
	            uniform = new ContinuousUniformDistribution(generator);
                AlphaAndBeta(uniform, min, max);
                // Beta must always be greater than alpha, so we set them this way round
	            if (cache)
	            {
	                continuousUniformGenerators.Add(key, uniform);
	            }
	        }
	        else
	        {
	            uniform = continuousUniformGenerators[key];
	        }
	        Debug.Assert(uniform != null);
            Debug.Assert(uniform.Alpha == min);
            Debug.Assert(uniform.Beta == max);
	        double value = uniform.NextDouble();
            Debug.Assert(value >= min && value <= max);
            return value;
	    }
	    
	    public static double ContinuousUniform(double min, double max)
	    {
            Debug.Assert(min <= max);
	        double value = ContinuousUniform(min, max, true);
            Debug.Assert(value >= min && value <= max);
            return value;
	    }
	    
	    public static double ContinuousUniformZeroToN(double n, bool cache)
	    {
            Debug.Assert(n >= 0.0);
	        double value = ContinuousUniform(0.0, n, cache);
            Debug.Assert(value >= 0.0 && value <= n);
            return value;
	    }
	    
	    public static double ContinuousUniformZeroToN(double n)
	    {
            Debug.Assert(n >= 0.0);
	        double value = ContinuousUniformZeroToN(n, true);
            Debug.Assert(value >= 0.0 && value <= n);
            return value;
        }
	    	    
	    public static double ContinuousUniformZeroToOne()
	    {
	        double value = zeroToOne.NextDouble();
            Debug.Assert(value >= 0.0 && value <= 1.0);
            return value;
	    }

        // A routine to work around the mis-feature in setting alpha and beta
        private static void AlphaAndBeta(ContinuousUniformDistribution distribution, double alpha, double beta)
        {

            distribution.Alpha = Math.Min(alpha, distribution.Alpha);
            distribution.Beta  = Math.Max(beta,  distribution.Beta);

            distribution.Alpha = alpha;
            distribution.Beta  = beta;
            Debug.Assert(distribution.Alpha == alpha);
            Debug.Assert(distribution.Beta == beta);
        }

        // A routine to work around the mis-feature in setting alpha and beta
        private static void AlphaAndBeta(DiscreteUniformDistribution distribution, int alpha, int beta)
        {

            distribution.Alpha = Math.Min(alpha, distribution.Alpha);
            distribution.Beta  = Math.Max(beta, distribution.Beta);

            distribution.Alpha = alpha;
            distribution.Beta = beta;
            Debug.Assert(distribution.Alpha == alpha);
            Debug.Assert(distribution.Beta == beta);
        }
	}
}
