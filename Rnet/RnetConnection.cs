using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Provides the core RNET implementation.
    /// </summary>
    public abstract class RnetConnection : IDisposable
    {

        RnetStreamWriter writer;
        RnetStreamReader reader;

        /// <summary>
        /// Initializes a new connection that communicates with the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream"></param>
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
        /// Initializes the connection.
        /// </summary>
        void Init()
        {
            reader = GetReader();
            if (reader == null)
                throw new RnetException("Unable to obtain RnetReader.");

            writer = GetWriter();
            if (writer == null)
                throw new RnetException("Unable to obtain RnetWriter.");
        }

        /// <summary>
        /// Opens the connection to RNet.
        /// </summary>
        public void Open()
        {
            try
            {
                Connect();
                Init();
            }
            catch (RnetException e)
            {
                ExceptionDispatchInfo.Capture(e).Throw();
            }
            catch (Exception e)
            {
                throw new RnetConnectionException("Could not open connection.", e);
            }

            OnStateChanged(new RnetConnectionStateEventArgs(State));
        }

        /// <summary>
        /// Closes the current connection.
        /// </summary>
        public void Close()
        {
            try
            {
                Disconnect();
            }
            catch (RnetException e)
            {
                ExceptionDispatchInfo.Capture(e).Throw();
            }
            catch (Exception e)
            {
                throw new RnetConnectionException("Could not close connection.", e);
            }

            OnStateChanged(new RnetConnectionStateEventArgs(State));
        }

        /// <summary>
        /// Implementations should establish a connection.
        /// </summary>
        protected abstract void Connect();

        /// <summary>
        /// Implementations should establish a connection.
        /// </summary>
        /// <returns></returns>
        protected abstract Task ConnectAsync();

        /// <summary>
        /// Implementations should disconnect.
        /// </summary>
        protected abstract void Disconnect();

        /// <summary>
        /// Implementations should disconnect.
        /// </summary>
        /// <returns></returns>
        protected abstract Task DisconnectAsync();

        /// <summary>
        /// Sends the message to the connection.
        /// </summary>
        /// <param name="message"></param>
        public virtual void Send(RnetMessage message)
        {
            if (State != RnetConnectionState.Open)
                throw new RnetConnectionException("Connection is not open.");

            message.Write(writer);
        }

        /// <summary>
        /// Sends the message to the connection.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual Task SendAsync(RnetMessage message)
        {
            return SendAsync(message, CancellationToken.None);
        }

        /// <summary>
        /// Sends the message to the connection.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task SendAsync(RnetMessage message, CancellationToken cancellationToken)
        {
            if (State != RnetConnectionState.Open)
                throw new RnetConnectionException("Connection is not open.");

            message.Write(writer);

            return Task.FromResult(0);
        }

        /// <summary>
        /// Reads the next message from the connection.
        /// </summary>
        /// <returns></returns>
        public virtual RnetMessage Receive()
        {
            if (State != RnetConnectionState.Open)
                throw new RnetConnectionException("Connection is not open.");

            return reader.Read();
        }

        /// <summary>
        /// Reads the next message from the connection.
        /// </summary>
        /// <returns></returns>
        public virtual Task<RnetMessage> ReceiveAsync()
        {
            return ReceiveAsync(CancellationToken.None);
        }

        /// <summary>
        /// Reads the next message from the connection.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task<RnetMessage> ReceiveAsync(CancellationToken cancellationToken)
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
        public virtual void Dispose()
        {
            Close();
        }

    }

}
