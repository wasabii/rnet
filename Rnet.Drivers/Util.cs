using System;
using System.Reactive.Linq;

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

    }

}
