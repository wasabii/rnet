using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Rnet
{

    static class DictionaryExtensions
    {

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
        {
            Contract.Requires<ArgumentNullException>(self != null);

            TValue value;
            return self.TryGetValue(key, out value) ? value : default(TValue);
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, Func<TKey, TValue> create)
        {
            Contract.Requires<ArgumentNullException>(self != null);

            TValue value;
            return self.TryGetValue(key, out value) ? value : self[key] = create(key);
        }

        /// <summary>
        /// Gets or creates a value on a dictionary of weak references.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, WeakReference<TValue>> self, TKey key, Func<TKey, TValue> create)
            where TValue : class
        {
            Contract.Requires<ArgumentNullException>(self != null);

            // get existing item
            WeakReference<TValue> reference;
            TValue value;
            if (self.TryGetValue(key, out reference) && (value = reference.GetTargetOrDefault()) != null)
                return value;

            // insert new item
            self[key] = reference = new WeakReference<TValue>(value = create(key));
            return value;
        }

        public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> self, TKey key)
        {
            Contract.Requires<ArgumentNullException>(self != null);

            TValue value;
            return self.TryRemove(key, out value);
        }

    }

}
