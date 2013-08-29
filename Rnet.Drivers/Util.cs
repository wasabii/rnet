using System;
using System.Reactive.Linq;

using Rnet.Profiles;

namespace Rnet.Drivers
{

    /// <summary>
    /// Provides various utilities that assist in working with the driver model.
    /// </summary>
    public static class Util
    {

        /// <summary>
        /// Projects each element into an ASCII string.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<string> ToAscii(this IObservable<byte[]> source)
        {
            return source
                .Select(d => Rnet.RnetDataUtil.GetAsciiString(d));
        }

        /// <summary>
        /// Sets the container context values.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="owner"></param>
        /// <param name="container"></param>
        public static void SetContainerContext(this RnetBusObject target, RnetBusObject owner, RnetBusObject container)
        {
            var context = target.Context.Get<IContainerContext>();
            if (context == null ||
                context.Owner != owner ||
                context.Container != container)
                target.Context.Set<IContainerContext>(new ContainerContext(owner, container));
        }

    }

}
