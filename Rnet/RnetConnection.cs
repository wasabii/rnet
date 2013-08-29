using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

using Nito.AsyncEx;

namespace Rnet
{

    /// <summary>
    /// Provides the core RNET implementation.
    /// </summary>
    public abstract class RnetConnection : IDisposable
    {

        /// <summary>
        /// Obtains a <see cref="RnetConnection"/> from the given URI.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static RnetConnection Create(Uri uri)
        {
            Contract.Requires<ArgumentNullException>(uri != null);

            if (uri.Scheme == "tcp" ||
                uri.Scheme == "rnet.tcp")
                return new RnetTcpConnection(uri);

            throw new RnetException("Unknown connection URI schema " + uri.Scheme + ".");
        }

        AsyncLock write = new AsyncLock();
        RnetStreamWriter writer;
        RnetStreamReader reader;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected RnetConnection()
        {

        }

        /// <summary>
        /// Implementations should return the <see cref="RnetStreamReader"/> for the current connection.
        /// </summary>
        /// <returns></returns>
        protected abstract RnetStreamReader GetReader();

        /// <summary>
        /// Implementations should return the <see cref="RnetStreamWriter"/> for the current connection.
        /// </summary>
        /// <returns></returns>
        protected abstract RnetStreamWriter GetWriter();

        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <returns></returns>
        public Task Open()
        {
            return Open(CancellationToken.None);
        }

        /// <summary>
        /// Opens the connection.
        /// </summary>
        public async Task Open(CancellationToken cancellationToken)
        {
            if (State != RnetConnectionState.Closed)
                throw new RnetConnectionException("Connection is not closed.");

            // establish connection
            await Connect(cancellationToken);

            // obtain reader
            reader = GetReader();
            if (reader == null)
                throw new RnetException("Unable to obtain RnetReader.");

            // obtain writer
            writer = GetWriter();
            if (writer == null)
                throw new RnetException("Unable to obtain RnetWriter.");

            OnStateChanged(new RnetConnectionStateEventArgs(State));
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <returns></returns>
        public Task Close()
        {
            return Close(CancellationToken.None);
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public async Task Close(CancellationToken cancellationToken)
        {
            if (State != RnetConnectionState.Open)
                throw new RnetConnectionException("Connection is not open.");

            // disconnect
            await Disconnect(cancellationToken);
            reader = null;
            writer = null;

            OnStateChanged(new RnetConnectionStateEventArgs(State));
        }

        /// <summary>
        /// Connects to the RNET endpoint.
        /// </summary>
        /// <returns></returns>
        protected abstract Task Connect(CancellationToken cancellationToken);

        /// <summary>
        /// Disconnects from the RNET endpoint.
        /// </summary>
        /// <returns></returns>
        protected abstract Task Disconnect(CancellationToken cancellationToken);

        /// <summary>
        /// Sends a message to the connection.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task Send(RnetMessage message)
        {
            return Send(message, CancellationToken.None);
        }

        /// <summary>
        /// Sends a message to the connection.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task Send(RnetMessage message, CancellationToken cancellationToken)
        {
            Contract.Requires<ArgumentNullException>(message != null);

            if (State != RnetConnectionState.Open)
                throw new RnetConnectionException("Connection is not open.");

            // write in the background to prevent blocking the calling thread on a socket
            return Task.Run(async () =>
                    await WriteMessage(message, cancellationToken),
                cancellationToken);
        }

        /// <summary>
        /// Writes the message with the writer.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task WriteMessage(RnetMessage message, CancellationToken cancellationToken)
        {
            Contract.Requires<ArgumentNullException>(message != null);

            using (await write.LockAsync(cancellationToken))
                message.Write(writer);
        }

        /// <summary>
        /// Reads the next message from the connection.
        /// </summary>
        /// <returns></returns>
        public Task<RnetMessage> Read()
        {
            return Read(CancellationToken.None);
        }

        /// <summary>
        /// Reads the next message from the connection.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task<RnetMessage> Read(CancellationToken cancellationToken)
        {
            if (State != RnetConnectionState.Open)
                throw new RnetConnectionException("Connection is not open.");

            return reader.ReadAsync(cancellationToken);
        }

        /// <summary>
        /// Gets the current connection state.
        /// </summary>
        public abstract RnetConnectionState State { get; }

        /// <summary>
        /// Raised when the connection state changes.
        /// </summary>
        public event EventHandler<RnetConnectionStateEventArgs> StateChanged;

        /// <summary>
        /// Raises the StateChanged event.
        /// </summary>
        /// <param name="args"></param>
        void OnStateChanged(RnetConnectionStateEventArgs args)
        {
            if (StateChanged != null)
                StateChanged(this, args);
        }

        /// <summary>
        /// Disposes of the current connection.
        /// </summary>
        public void Dispose()
        {
            try
            {
                Close().Wait(5000);
                GC.SuppressFinalize(this);
            }
            catch
            { 
                // ignore all
            }
        }

        /// <summary>
        /// Finalizes the instance.
        /// </summary>
        ~RnetConnection()
        {
            Dispose();
        }

    }

}
