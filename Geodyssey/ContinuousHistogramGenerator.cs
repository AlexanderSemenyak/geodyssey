using System;
using System.Collections.Generic;
using System.Text;
using Wintellect.PowerCollections;

using Numeric;

namespace Geodyssey
{
    /// <summary>
    /// Random number generator that uses the supplied histogram to
    /// describe the probability density function. A class-interval
    /// is selected from the PDF, and a uniformally distibuted real
    /// value returned from within the interval.
    /// </summary>
    class ContinuousHistogramGenerator
    {
        private Histogram histogram;
        private List<double> rouletteWheel;

        public ContinuousHistogramGenerator(Histogram histogram)
        {
            this.histogram = (Histogram) histogram.Clone();
            // Build the roulette wheel where widths are proportional
            // to histogram frequency - the cumulative frequency distribution
            this.rouletteWheel = new List<double>(histogram.Count);
            double previous = 0.0;
            foreach (double value in histogram)
            {
                double next = value + previous;
                rouletteWheel.Add(next);
                //Console.WriteLine("next = {0}", next);
                previous = next;
            }

        }

        public double NextDouble()
        {
            // Choose a uniform random offset along the rouletteWheel
            double offset = Rng.ContinuousUniformZeroToN(rouletteWheel[rouletteWheel.Count - 1]);
            // Determine which class-interval we are in
            int index; // Contains the index into the rouletteWheel of the containing class
            Algorithms.BinarySearch<double>(rouletteWheel, offset, out index);
            // Choose a uniformally distributed value from within the class
            double classLower = histogram.LowerBoundary(index);
            double classUpper = histogram.UpperBoundary(index);
            return Rng.ContinuousUniform(classLower, classUpper);
        }
    }
}
