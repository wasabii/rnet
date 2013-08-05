using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Various utilities for working with RNET data.
    /// </summary>
    public static class RnetDataUtil
    {

        /// <summary>
        /// Converts the data to an ASCII string.
        /// </summary>
        /// <param name="dat"></param>
        /// <returns></returns>
        public static string GetAsciiString(byte[] dat)
        {
            if (dat == null)
                return null;

            var txt = Encoding.ASCII.GetString(dat);
            if (txt == null)
                return null;

            var idx = txt.IndexOf('\0');
            if (idx != -1)
                txt = txt.Remove(idx);

            return txt;
        }

        /// <summary>
        /// Reads the data as an ASCII string if possible.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static async Task<string> ReadAsciiString(this RnetDataHandle self)
        {
            return GetAsciiString(await self.Read());
        }

        /// <summary>
        /// Reads the data as an ASCII string if possible.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<string> ReadAsciiString(this RnetDataHandle self, CancellationToken cancellationToken)
        {
            return GetAsciiString(await self.Read(cancellationToken));
        }

    }

}
