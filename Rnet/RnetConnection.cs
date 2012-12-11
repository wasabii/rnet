using System;
using System.IO;

namespace Rnet
{

    /// <summary>
    /// Provides the core RNET implementation.
    /// </summary>
    public abstract class RnetConnection : IDisposable
    {

        /// <summary>
        /// Initializes a new connection that communicates with the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream"></param>
        protected RnetConnection()
        {

        }

        /// <summary>
        /// Stream providing access to RNET.
        /// </summary>
        protected abstract Stream Stream { get; }

        /// <summary>
        /// Opens the connection to RNET.
        /// </summary>
        public abstract void Open();

        /// <summary>
        /// Gets whether or not the RNET connection is open.
        /// </summary>
        public abstract bool IsOpen { get; }

        /// <summary>
        /// Closes the current connection.
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// Disposes of the current connection.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Checks whether the connection has been opened.
        /// </summary>
        void CheckOpen()
        {
            if (!IsOpen)
                throw new InvalidOperationException("RnetConnection has not been opened.");
        }



    }

}
