using System;
using System.Collections.Generic;
using System.Linq;

namespace Rnet.Profiles
{

    /// <summary>
    /// Base profile class. A <see cref="RnetProfile"/> provides an interface for working with a high level view of
    /// a <see cref="RnetBusObject"/>.
    /// </summary>
    public abstract class RnetProfile
    {

        /// <summary>
        /// Available profile types.
        /// </summary>
        static readonly IEnumerable<Type> ProfileTypes = typeof(RnetProfile).Assembly.GetTypes()
            .Where(i => i.IsSubclassOf(typeof(RnetProfile)))
            .Where(i => !i.IsAbstract)
            .ToList();



    }

}
