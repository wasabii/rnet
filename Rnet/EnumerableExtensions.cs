using System.Collections.Generic;
using System.IO;

namespace Rnet
{

    static class EnumerableExtensions
    {

        /// <summary>
        /// Gets the next byte.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static byte Next(this IEnumerator<byte> e)
        {
            if (!e.MoveNext())
                throw new EndOfStreamException();

            return e.Current;
        }

    }

}
