using System.Collections.Generic;

namespace Rnet
{

    static class DictionaryExtensions
    {

        public static TValue ValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
        {
            TValue value;
            return self.TryGetValue(key, out value) ? value : default(TValue);
        }

    }

}
