using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Wintellect.PowerCollections;

namespace Utility.Extensions.System.Collections.Generic
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Extention method for left Fold.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <param name="function"></param>
        /// <param name="accumulator"></param>
        /// <returns></returns>
        public static T FoldL<T>(this IEnumerable<T> seq, Func<T, T, T> function, T accumulator)
        {
            foreach (T item in seq)
            {
                accumulator = function(item, accumulator);
            }
            return accumulator;
        }

        /// <summary>
        /// Extension method for right fold.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <param name="function"></param>
        /// <param name="accumulator"></param>
        /// <returns></returns>
        public static T FoldR<T>(this IEnumerable<T> seq, Func<T, T, T> function, T accumulator)
        {
            seq = seq.Reverse();
            return FoldL(seq, function, accumulator);
        }

        /// <summary>
        /// Applies the given action to successive pairs of the sequence
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <param name="action"></param>
        public static void ForEachPair<T>(this IEnumerable<T> seq, Action<T, T> action)
        {
            
            bool first = true;
            T previous = default(T);
            foreach (T current in seq)
            {
                if (first)
                {
                    first = false;
                    previous = current;
                    continue;
                }

                action(previous, current);
                previous = current;
            }
        }

        /// <summary>
        /// Returns successive pairs of a sequence
        /// </summary>
        /// <typeparam name="T">The enumerated type</typeparam>
        /// <param name="seq">A sequence from which successive pairs are to be returned</param>
        /// <returns>A sequence of Pairs</returns>
        public static IEnumerable<Pair<T, T>> SuccessivePairs<T>(this IEnumerable<T> seq)
        {
            bool first = true;
            T previous = default(T);
            foreach (T current in seq)
            {
                if (first)
                {
                    first = false;
                    previous = current;
                    continue;
                }

                yield return new Pair<T, T>(previous, current);
                previous = current;
            }            
        }

        /// <summary>
        /// Wraps a sequence round back to the beginning by making enumeration past the
        /// last item of the source sequence return the first item in the source sequence.
        /// The result sequence is not strictly circular - it contains N+1 elements.
        /// </summary>
        /// <typeparam name="T">The element type of the enumerator</typeparam>
        /// <param name="seq">The source sequence</param>
        /// <returns>The result sequence containing one more element that the source sequence,
        /// unless the source sequence is empty.</returns>
        public static IEnumerable<T> Wrap<T>(this IEnumerable<T> seq)
        {
            return seq.Concat(seq.Take(1));
        }

        public static Pair<T, T> MinMax<T>(this IEnumerable<T> seq)
        {
            Comparer<T> cmp = Comparer<T>.Default;
            return MinMax(seq, cmp);
        }

        public static Pair<T, T> MinMax<T>(this IEnumerable<T> seq, IComparer<T> comparer)
        {
            T min = seq.First();
            T max = min;
            foreach (T value in seq)
            {
                if (comparer.Compare(value, min) < 0)
                {
                    min = value;
                }
                if (comparer.Compare(value, max) > 0)
                {
                    max = value;
                }
            }
            return new Pair<T, T>(min, max);
        }

        public static bool IsSorted<T>(this IEnumerable<T> seq)
        {
            Comparer<T> cmp = Comparer<T>.Default;
            return seq.IsSorted(cmp);
        }

        public static bool IsSorted<T>(this IEnumerable<T> seq, IComparer<T> comparer)
        {
            bool first = true;
            T previous = default(T);
            foreach (T current in seq)
            {
                if (first)
                {
                    first = false;
                    previous = current;
                    continue;
                }

                if (comparer.Compare(previous, current) > 0)
                {
                    return false;
                }

                previous = current;
            }
            return true;
        }

        /// <summary>
        /// Merge two sequences that are already sorted into a single sorted
        /// sequence. O(N) complexity.
        /// </summary>
        /// <param name="seq1">A sorted sequence</param>
        /// <param name="seq2">A sorted sequence</param>
        /// <returns></returns>
        public static IEnumerable<T> MergeSorted<T>(this IEnumerable<T> seq1, IEnumerable<T> seq2)
        {
            return seq1.MergeSorted(seq2, Comparer<T>.Default);
        }

        /// <summary>
        /// Merge two sequences that are already sorted into a single sorted sequence.
        /// O(N) complexity.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequences</typeparam>
        /// <param name="seq1">The first source sorted sequence</param>
        /// <param name="seq2">The second source sorted sequence</param>
        /// <param name="cmp">The comparer</param>
        /// <returns>A merged sorted sequence</returns>
        public static IEnumerable<T> MergeSorted<T>(this IEnumerable<T> seq1, IEnumerable<T> seq2, IComparer<T> cmp)
        {
            Debug.Assert(seq1.IsSorted(cmp));
            Debug.Assert(seq2.IsSorted(cmp));
            using (IEnumerator<T> e1 = seq1.GetEnumerator())
            using (IEnumerator<T> e2 = seq2.GetEnumerator())
            {
                bool valid1 = e1.MoveNext();
                bool valid2 = e2.MoveNext();

                while (valid1 && valid2)
                {
                    T value1 = e1.Current;
                    T value2 = e2.Current;
                    if (cmp.Compare(value1, value2) < 0)
                    {
                        yield return value1;
                        valid1 = e1.MoveNext();
                    }
                    else
                    {
                        yield return value2;
                        valid2 = e2.MoveNext();
                    }
                }

                if (valid1)
                {
                    while (valid1)
                    {
                        yield return e1.Current;
                        valid1 = e1.MoveNext();
                    }
                }

                if (valid2)
                {
                    while (valid2)
                    {
                        yield return e2.Current;
                        valid2 = e2.MoveNext();
                    }
                }
            }
        }
    }
}
