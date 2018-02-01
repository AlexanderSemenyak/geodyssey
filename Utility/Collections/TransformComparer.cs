using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility.Collections
{
    /// <summary>
    /// An IComparer&lt;TSource&gt; which adapts an existing comparer by applying a
    /// transformation to types of TSource to TResult before comparing
    /// with IComparer&lt;TResult&gt;
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class TransformComparer<TSource, TResult> : IComparer<TSource>
    {
        private readonly IComparer<TResult> resultComparer;
        private readonly Func<TSource, TResult> transformation;

        public TransformComparer(IComparer<TResult> resultComparer, Func<TSource, TResult> transformation)
        {
            this.resultComparer = resultComparer;
            this.transformation = transformation;
        }

        public int Compare(TSource x, TSource y)
        {
            return resultComparer.Compare(transformation(x), transformation(y));
        }
    }
}
