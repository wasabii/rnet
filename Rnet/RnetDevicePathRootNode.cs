using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Represents the root directory of a device.
    /// </summary>
    public sealed class RnetDevicePathRootNode : RnetDevicePathNode
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="parent"></param>
        internal RnetDevicePathRootNode(RnetDevice device)
            : base(device, null, RnetPath.Empty)
        {

        }

        /// <summary>
        /// Gets the node at the absolute path in the current local directory structure. Returns <c>null</c> if the
        /// directory does not yet exist locally.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<RnetDevicePathNode> FindAsync(RnetPath path)
        {
            return FindAsync(path, Device.RequestDataCancellationToken);
        }

        /// <summary>
        /// Gets the node at the absolute path in the current local directory structure. Returns <c>null</c> if the
        /// directory does not yet exist locally.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<RnetDevicePathNode> FindAsync(RnetPath path, CancellationToken cancellationToken)
        {
            return FindAsync(cancellationToken, path.ToArray());
        }

        /// <summary>
        /// Sets the node data at the absolute path in the current local directory structure.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="buffer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal Task SetAsync(RnetPath path, byte[] buffer)
        {
            return SetBufferAsync(buffer, path.ToArray());
        }

        /// <summary>
        /// Reads the node at the specified absolute path from the device.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<RnetDevicePathNode> RequestAsync(RnetPath path)
        {
            return RequestAsync(path, Device.RequestDataCancellationToken);
        }

        /// <summary>
        /// Reads the node at the specified absolute path from the device.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<RnetDevicePathNode> RequestAsync(RnetPath path, CancellationToken cancellationToken)
        {
            return RequestAsync(cancellationToken, path.ToArray());
        }

        /// <summary>
        /// Waits for the node at the specified absolute path to appear.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal Task<RnetDevicePathNode> WaitAsync(RnetPath path, CancellationToken cancellationToken)
        {
            return WaitAsync(cancellationToken, path.ToArray());
        }

        /// <summary>
        /// Gets the node at the specified absolute path if available or requests it from the remote device.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<RnetDevicePathNode> GetAsync(RnetPath path)
        {
            return GetAsync(path, Device.RequestDataCancellationToken);
        }

        /// <summary>
        /// Gets the node at the specified absolute path if available or requests it from the remote device.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<RnetDevicePathNode> GetAsync(RnetPath path, CancellationToken cancellationToken)
        {
            return GetAsync(cancellationToken, path.ToArray());
        }

        /// <summary>
        /// Gets the node data at the specified absolute path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<byte[]> GetDataAsync(RnetPath path)
        {
            return GetBufferAsync(path.ToArray());
        }

        /// <summary>
        /// Gets the node data at the specified absolute path.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<byte[]> GetDataAsync(RnetPath path, CancellationToken cancellationToken)
        {
            return GetBufferAsync(cancellationToken, path.ToArray());
        }

    }

}
