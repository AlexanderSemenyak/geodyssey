using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility.Collections
{
    public class TransformEqualityComparer<TSource, TResult> : IEqualityComparer<TSource>
    {
        private readonly IEqualityComparer<TResult> resultComparer;
        private readonly Func<TSource, TResult> transformation;

        public TransformEqualityComparer(IEqualityComparer<TResult> resultComparer, Func<TSource, TResult> transformation)
        {
            this.resultComparer = resultComparer;
            this.transformation = transformation;
        }

        public bool Equals(TSource x, TSource y)
        {
            return resultComparer.Equals(transformation(x), transformation(y));
        }

        public int GetHashCode(TSource obj)
        {
            return resultComparer.GetHashCode(transformation(obj));
        }
    }
}
