using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Provides a handle by which to manipulate data at a specific path in a local RNET device.
    /// </summary>
    public sealed class RnetLocalDataHandle : RnetDataHandle
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="path"></param>
        internal RnetLocalDataHandle(RnetLocalDevice device, RnetPath path)
            : base(device, path)
        {

        }

        /// <summary>
        /// Owning local device of the handle.
        /// </summary>
        public new RnetRemoteDevice Device
        {
            get { return (RnetRemoteDevice)base.Device; }
        }

        /// <summary>
        /// Gets the current value without reading it from the device.
        /// </summary>
        public override byte[] Current
        {
            get { return null; }
        }

        /// <summary>
        /// Timestamp of the last data update.
        /// </summary>
        public override DateTime Timestamp
        {
            get { return DateTime.UtcNow; }
        }

        /// <summary>
        /// Reads the data from the device path.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<byte[]> Read(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Refreshes the data from the device.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<byte[]> Refresh(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Writes the data to the device and returns the data after the write was completed.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<byte[]> Write(byte[] data, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Writes the data to the device and returns the data after the write was completed.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<Stream> Write(Stream data, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Sends an event to the path on the device and returns the data after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="data"></param>
        /// <param name="priority"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<byte[]> SendEvent(RnetEvent evt, ushort timestamp, ushort data, RnetPriority priority, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

    }

}
