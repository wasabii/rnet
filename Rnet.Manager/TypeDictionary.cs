using System;
using System.Collections.Generic;
using System.Linq;

using Rnet.Drivers;

namespace Rnet.Manager
{

    public class TypeDictionary<TValue> : Dictionary<string, TValue>
    {

        public TypeDictionary(IDictionary<Type, TValue> dictionary)
            : base(dictionary.ToDictionary(i => i.Key.FullName, i => i.Value))
        {

        }

    }

}
