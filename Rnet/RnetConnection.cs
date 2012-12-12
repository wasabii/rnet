using System;
using System.IO;

namespace Rnet
{

    /// <summary>
    /// Provides the core RNET implementation.
    /// </summary>
    public abstract class RnetConnection : IDisposable
    {

        RnetMessageWriter writer;
        RnetMessageReader reader;

        /// <summary>
        /// Initializes a new connection that communicates with the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream"></param>
        protected RnetConnection()
        {

        }

        /// <summary>
        /// Stream providing access to RNet.
        /// </summary>
        internal abstract Stream Stream { get; }

        /// <summary>
        /// Opens the connection to RNet.
        /// </summary>
        public abstract void Open();

        /// <summary>
        /// Gets whether or not the RNet connection is open.
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
                throw new InvalidOperationException("RnetConnection is not open.");
        }

        /// <summary>
        /// Gets the message writer for this connection.
        /// </summary>
        public RnetMessageWriter Writer
        {
            get { return writer ?? (writer = new RnetMessageWriter(Stream)); }
        }

        /// <summary>
        /// Gets the message reader for this connection.
        /// </summary>
        public RnetMessageReader Reader
        {
            get { return reader ?? (reader = new RnetMessageReader(Stream)); }
        }

        /// <summary>
        /// Sends the specified message to the connected RNet device.
        /// </summary>
        /// <param name="message"></param>
        public void Send(RnetMessage message)
        {
            CheckOpen();
            message.Write(Writer);
        }

    }

}
