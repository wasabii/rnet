using System;
using System.Threading.Tasks;

namespace Rnet.Profiles
{

    public static class RnetDevicePathNodeExtensions
    {

        /// <summary>
        /// Subscribes to the given device path and invokes the action when data is available.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="path"></param>
        /// <param name="on"></param>
        /// <returns></returns>
        public static async Task<RnetDevicePathNode> Subscribe(this RnetDevice device, RnetPath path, Action<byte[]> on)
        {
            var d = await device.Root.GetAsync(path);
            if (d != null)
            {
                d.BufferChanged += (s, a) => on(a.Value);
                on(await d.GetBufferAsync());
            }

            return d;
        }

    }

}
