using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    public static class RnetDeviceDirectoryExtensions
    {

        /// <summary>
        /// Converts the data to an ASCII string.
        /// </summary>
        /// <param name="dat"></param>
        /// <returns></returns>
        static string ToAsciiString(byte[] dat)
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
        /// Gets the directory data as an ASCII string if possible.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static async Task<string> GetAsciiStringAsync(this RnetDevicePathNode self)
        {
            return ToAsciiString(await self.GetBufferAsync());
        }

        /// <summary>
        /// Gets the directory data as an ASCII string if possible.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<string> GetAsciiStringAsync(this RnetDevicePathNode self, CancellationToken cancellationToken)
        {
            return ToAsciiString(await self.GetBufferAsync(cancellationToken));
        }

        /// <summary>
        /// Gets the directory data at the specified relative path as an ASCII string if available or requests it from the remote device.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<string> GetAsciiStringAsync(this RnetDevicePathNode self, params byte[] path)
        {
            return ToAsciiString(await self.GetBufferAsync(path));
        }

        /// <summary>
        /// Gets the directory data at the specified relative path as an ASCII string if available or requests it from the remote device.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<string> GetAsciiStringAsync(this RnetDevicePathNode self, CancellationToken cancellationToken, params byte[] path)
        {
            return ToAsciiString(await self.GetBufferAsync(cancellationToken, path));
        }

        /// <summary>
        /// Gets the directory data at the specified relative path as an ASCII string if available or requests it from the remote device.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<string> GetAsciiStringAsync(this RnetDevicePathRootNode self, RnetPath path)
        {
            return ToAsciiString(await self.GetDataAsync(path));
        }

        /// <summary>
        /// Gets the directory data at the specified relative path as an ASCII string if available or requests it from the remote device.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<string> GetAsciiStringAsync(this RnetDevicePathRootNode self, RnetPath path, CancellationToken cancellationToken)
        {
            return ToAsciiString(await self.GetDataAsync(path, cancellationToken));
        }

    }

}
