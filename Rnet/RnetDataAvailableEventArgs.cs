using System;

namespace Rnet
{

    /// <summary>
    /// Provides the data that became available at a device path.
    /// </summary>
    public class RnetDataAvailableEventArgs : EventArgs
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="value"></param>
        internal RnetDataAvailableEventArgs(byte[] data)
        {
            Data = data;
        }

        /// <summary>
        /// Gets the data that has become available.
        /// </summary>
        public byte[] Data { get; private set; }

    }

}
