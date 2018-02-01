using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Wintellect.PowerCollections;

namespace Numeric
{
    public class Histogram : ICloneable, IEnumerable<int>
    {
        private readonly List<int> accumulator;
        private double min;
        private double max;

        public Histogram(int numberOfClasses, double min, double max)
        {
            this.min = min;
            this.max = max;

            this.accumulator = new List<int>(numberOfClasses);
            for (int i = 0 ; i < numberOfClasses ; ++i)
            {
                this.accumulator.Add(0);
            }
            Debug.Assert(accumulator.Count == numberOfClasses);

        }

        private Histogram(Histogram other)
        {
            this.accumulator = new List<int>(other.accumulator);
            this.min = other.min;
            this.max = other.max;
        }

        public double Min
        {
            get { return min; }
        }

        public double Max
        {
            get { return max; }
        }

        public int this[int index]
	    {
	        get { return accumulator[index];  }
	    }

        public double ClassInterval
        {
            get { return (max - min) / accumulator.Count; }
        }

        public double LowerBoundary(int index)
        {
            return min + index * ClassInterval;
        }

        public double UpperBoundary(int index)
        {
            return min + (index + 1) * ClassInterval;
        }

        #region ICloneable Members

        public object Clone()
        {
            return new Histogram(this);
        }

        #endregion

        public void Accumulate(double item)
        {
            Debug.Assert(item >= min && item <= max);
            double offset = item - min;
            int index = (int) Math.Floor(offset / ClassInterval);
            // Put the max values into the top class
            if (index == accumulator.Count)
            {
                Debug.Assert(item == max);
                --index;
            }
            Debug.Assert(index >= 0 && index < accumulator.Count);
            ++accumulator[index];
        }

        public void Clear()
        {
            min = 0.0;
            max = 0.0;
            accumulator.Clear();
        }

        public int Count
        {
            get { return accumulator.Count; }
        }

        #region IEnumerable<int> Members

        public IEnumerator<int> GetEnumerator()
        {
            return accumulator.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return accumulator.GetEnumerator();
        }

        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0 ; i < accumulator.Count ; ++i)
            {
                sb.AppendFormat("{0} {1} {2}\n", LowerBoundary(i), UpperBoundary(i), accumulator[i]);
            }
            return sb.ToString();
        }

    }
}
