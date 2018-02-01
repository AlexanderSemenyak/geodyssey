using System;
using System.Diagnostics;

namespace Geodyssey
{
    /// <summary>
    /// A Generic wrapper around System.WeakReference
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class WeakReference<T>
    {
        WeakReference weakRef;

        [DebuggerStepThrough]
        public WeakReference(T target)
        {
            weakRef = new WeakReference(target);
        }

        [DebuggerStepThrough]
        public WeakReference(object target, bool trackResurrection)
        {
            weakRef = new WeakReference(target, trackResurrection);
        }

        public bool IsAlive
        {
            [DebuggerStepThrough]
            get { return weakRef.IsAlive; }
        }

        public T Target
        {
            [DebuggerStepThrough]
            get { return (T) weakRef.Target; }
            [DebuggerStepThrough]
            set { weakRef.Target = value; }
        }

        public bool TrackResurrection
        {
            [DebuggerStepThrough]
            get { return weakRef.TrackResurrection; }
        }
    }
}
