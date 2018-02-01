using System;
using System.Collections.Generic;
using System.Diagnostics;
using Wintellect.PowerCollections;

namespace Geodyssey
{
	/// <summary>
	/// Description of OutlineMapPhenotype.
	/// </summary>
	public class OutlineMapPhenotype : Phenotype
	{
	    private OutlineMap map;
	    
	    #region Construction
	    
	    public OutlineMapPhenotype(OutlineMapDomain domain) :
	        base(domain)
		{
	        this.map = new OutlineMap(domain.Min, domain.Max);
		}

        public OutlineMapPhenotype(OutlineMap map) :
            base(null)
        {
            this.map = map;
        }

	    #endregion
	    
	    #region Properties
	    
	    public OutlineMap Map
	    {
	        get { return map; }
        }

        #endregion

        #region Methods

        public override double[] Compare(Phenotype other)
		{
            Debug.Assert(other is OutlineMapPhenotype);
            OutlineMapPhenotype otherPhenotype = (OutlineMapPhenotype) other;
            // TODO compute fit.
            return new double[] { 0.0 };
		}

        public override string ToString()
        {
            return Map.ToString();
        }
        
        #endregion
	}
}
