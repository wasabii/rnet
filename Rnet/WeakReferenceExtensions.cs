using System;
using System.Diagnostics.Contracts;

namespace Rnet
{

    static class WeakReferenceExtensions
    {

        /// <summary>
        /// Gets the target of the given weak reference, of the default value if it no longer exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static T GetTargetOrDefault<T>(this WeakReference<T> self)
            where T : class
        {
            Contract.Requires<ArgumentNullException>(self != null);

            T value;
            return self.TryGetTarget(out value) ? value : default(T);
        }

    }

}
