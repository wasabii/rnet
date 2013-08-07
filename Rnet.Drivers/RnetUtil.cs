using System;
using System.Threading.Tasks;

namespace Rnet.Profiles
{

    /// <summary>
    /// Provides various utilities the driver model's interaction with Rnet.
    /// </summary>
    public static class RnetUtil
    {

        /// <summary>
        /// Invokes the action when when data is available.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="on"></param>
        /// <returns></returns>
        public static async Task<RnetDataHandle> Subscribe(this RnetDataHandle handle, Action<byte[]> on)
        {
            // subscribe to data
            var b = false;
            handle.DataAvailable += (s, a) => { b = true; on(a.Data); };

            // issue initial read
            var d = await handle.Read();

            // read did not invoke action, invoke it once ourselves
            if (d != null && !b)
                on(d);

            return handle;
        }

        /// <summary>
        /// Invokes the action when when data is available.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="on"></param>
        /// <returns></returns>
        public static Task<RnetDataHandle> Subscribe(this RnetDataHandle handle, Action<string> on)
        {
            return Subscribe(handle, d => on(RnetDataUtil.GetAsciiString(d)));
        }

    }

}
