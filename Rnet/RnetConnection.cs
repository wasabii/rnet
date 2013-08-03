using System;
using System.IO;
using System.Runtime.ExceptionServices;
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

        AsyncLock writerLock = new AsyncLock();
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
        /// Opens the connection to RNet.
        /// </summary>
        public async Task OpenAsync()
        {
            try
            {
                try
                {
                    await ConnectAsync();

                    // establish reader
                    reader = GetReader();
                    if (reader == null)
                        throw new RnetException("Unable to obtain RnetReader.");

                    // establish writer
                    writer = GetWriter();
                    if (writer == null)
                        throw new RnetException("Unable to obtain RnetWriter.");
                }
                catch (AggregateException e)
                {
                    e = e.Flatten();
                    if (e.InnerExceptions.Count == 1)
                        ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                    else
                        ExceptionDispatchInfo.Capture(e).Throw();
                }
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
        public async Task CloseAsync()
        {
            try
            {
                try
                {
                    await DisconnectAsync();
                }
                catch (AggregateException e)
                {
                    e = e.Flatten();
                    if (e.InnerExceptions.Count == 1)
                        ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                    else
                        ExceptionDispatchInfo.Capture(e).Throw();
                }
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
        /// <returns></returns>
        protected abstract Task ConnectAsync();

        /// <summary>
        /// Implementations should disconnect.
        /// </summary>
        /// <returns></returns>
        protected abstract Task DisconnectAsync();

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
                throw new RnetConnectionException("RNET connection is not open.");

            // write to writer in the background, but synchronously
            return Task.Run(async () =>
            {
                using (await writerLock.LockAsync(cancellationToken))
                    message.Write(writer);
            }, cancellationToken);
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
            try
            {
                if (State != RnetConnectionState.Open)
                    throw new RnetConnectionException("Connection is not open.");

                return reader.ReadAsync(cancellationToken);
            }
            catch (AggregateException e)
            {
                e = e.Flatten();
                if (e.InnerExceptions.Count == 1)
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                else
                    ExceptionDispatchInfo.Capture(e).Throw();

                // unreachable
                return null;
            }
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
            CloseAsync().Wait();
            GC.SuppressFinalize(this);
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
