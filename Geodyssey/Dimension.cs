/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-21
 * Time: 10:44
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace Geodyssey
{
	/// <summary>
	/// Description of Dimension.
	/// </summary>
	public class Dimension : ICloneable
	{
	    private double value;
	    private AbstractMutationStrategy mutationStrategy;
	    private AbstractRangeStrategy rangeStrategy;
	    	    
		public Dimension(double value, AbstractMutationStrategy mutationStrategy, AbstractRangeStrategy rangeStrategy)
		{
		    this.value = value;
		    this.mutationStrategy = mutationStrategy;
		    this.rangeStrategy = rangeStrategy;
		}
		
		public Dimension(Dimension other) :
		    this(other.value, other.mutationStrategy, other.rangeStrategy)
		{
		}
		
		public double Value
		{
		    get { return value; }
		}
		
		public void Mutate()
		{
		    value = this.rangeStrategy.Check( this.mutationStrategy.Mutate(value) );
		}
		
		public object Clone()
		{
		    return new Dimension(this);
		}
		
        public override string ToString()
        {
            return value.ToString();
        }
	}
}
