using System.Collections.Generic;
using System.IO;

namespace Rnet.Protocol
{

    static class EnumerableExtensions
    {

        /// <summary>
        /// Gets the next byte.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        internal static byte Next(this IEnumerator<byte> e)
        {
            if (!e.MoveNext())
                throw new EndOfStreamException();

            return e.Current;
        }

    }

}
