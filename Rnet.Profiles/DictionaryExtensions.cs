using System;
using System.Collections.Generic;

namespace Rnet.Profiles
{

    static class DictionaryExtensions
    {

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
        {
            TValue value;
            return self.TryGetValue(key, out value) ? value : default(TValue);
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, Func<TKey, TValue> create)
        {
            TValue value;
            return self.TryGetValue(key, out value) ? value : self[key] = create(key);
        }

    }

}
