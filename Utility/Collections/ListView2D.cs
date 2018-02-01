using System;
using System.Collections.Generic;
using System.Text;

namespace Utility.Collections
{
    /// <summary>
    /// A view which presents an IList as a two-dimensional array-like
    /// container in which the dimensions of the view can be modified
    /// with the constraint that the number of elements is equal to that
    /// in the underlying list.
    /// </summary>
    public class MultidimensionalListView<T>
    {
        private IList<T> list;
        private List<int> dimensions;

        public MultidimensionalListView(IList<T> list)
        {
            this.list = list;
            this.numRows = 1;
            this.numColums = list.Count;
        }

        public T this[int row, int column]
        {
            get { return list[index(row, column)]; }
            set { list[indexer(row, column)] = value; }
        }

        public int Rank
        {
            get { return dimensions.Count; }
            set
            {
                dimensions = new List<int>(value);
                for (int i = 0; i < value - 1; ++i)
                {
                    dimensions[i] = 1;
                }
                dimensions[value] = list.Count;
            }
        }

        public int GetLength(int dimension)
        {
            return dimensions[dimension];
        }

        public List<int[]> PossibleDimensions(int rank)
        {
            // Produce a list of factors of sizes
            // TODO: Use a set so we don't get duplicates
            List<int[]> result = new List<int[]>();
            int n = list.Count;
            // How to have n nested loops?
            for (int i = 1; i < Math.Sqrt(rank); ++i)
            {
                for (int j = 1; j < Math.Sqrt(rank); ++j)
                {
                    int ij = i * j;
                    if (n % ij == 0)
                    {
                        int k = n / ij;
                        // TODO: Use a standard algorithm for producing permutations here.
                        result.Add(new int[] { i, j, k });
                        result.Add(new int[] { i, k, j });
                        result.Add(new int[] { j, i, k });
                        result.Add(new int[] { j, k, i });
                        result.Add(new int[] { k, i, j });
                        result.Add(new int[] { k, j, i });
                    }
                }
            }
            result.Sort();
            result.
        }
    }
}
