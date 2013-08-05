using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Rnet
{

    /// <summary>
    /// Provides a durable wrapper around a <see cref="RnetConnection"/>.
    /// </summary>
    public class RnetClient : RnetModelObject, IDisposable
    {

        /// <summary>
        /// Initializes the static instance.
        /// </summary>
        static RnetClient()
        {
            RnetTcpUriParser.RegisterParser();
        }

        /// <summary>
        /// Obtains a <see cref="RnetConnection"/> from the given URI.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        static RnetConnection CreateConnection(Uri uri)
        {
            if (uri.Scheme == "rnet.tcp")
                return new RnetTcpConnection(uri);

            return null;
        }

        AsyncLock lck = new AsyncLock();
        RnetConnection connection;

        /// <summary>
        /// Moment of last attempted connection.
        /// </summary>
        DateTime lastConnectionAttempt = DateTime.MinValue;

        /// <summary>
        /// Last reported connection state.
        /// </summary>
        RnetConnectionState lastConnectionState = RnetConnectionState.Closed;

        /// <summary>
        /// Receives messages from the connection.
        /// </summary>
        Task receiveTask;

        /// <summary>
        /// Signals the worker thread to cancel.
        /// </summary>
        CancellationTokenSource cts;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="connection"></param>
        public RnetClient(RnetConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            this.connection = connection;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="uri"></param>
        public RnetClient(Uri uri)
            :this(CreateConnection(uri))
        {

        }

        /// <summary>
        /// Starts the <see cref="RnetClient"/>.
        /// </summary>
        public async Task StartAsync()
        {
            using (await lck.LockAsync())
            {
                // already started
                if (cts != null)
                    return;

                // already started
                if (receiveTask != null)
                    return;

                // start up receiver using a background task
                cts = new CancellationTokenSource();
                receiveTask = Task.Run(async () => await ReceiveLoopAsync(cts.Token));
            }

            OnStateChanged(new RnetClientStateEventArgs(State));
        }

        /// <summary>
        /// Stops the <see cref="RnetClient"/>.
        /// </summary>
        public async Task StopAsync()
        {
            using (await lck.LockAsync())
            {
                if (cts == null)
                    return;

                // signal cancel and wait for shutdown
                cts.Cancel();
            }

            // wait for task to end
            await receiveTask;
            receiveTask = null;

            OnStateChanged(new RnetClientStateEventArgs(State));
        }

        /// <summary>
        /// Gets the current state of the <see cref="RnetClient"/>.
        /// </summary>
        public RnetClientState State
        {
            get { return cts == null ? RnetClientState.Stopped : RnetClientState.Started; }
        }

        /// <summary>
        /// Gets the current connection state of the <see cref="RnetClient"/>.
        /// </summary>
        public RnetConnectionState ConnectionState
        {
            get { return connection.State; }
        }

        /// <summary>
        /// Ensures an active connection is available
        /// </summary>
        async Task<bool> EnsureConnection(CancellationToken ct)
        {
            using (await lck.LockAsync(ct))
            {
                // double check
                if (connection.State == RnetConnectionState.Open)
                    return true;

                TryRaiseConnectionStateChanged();

                // attempt to open connection
                try
                {
                    await connection.OpenAsync();
                }
                catch (Exception e)
                {
                    OnError(new RnetClientErrorEventArgs(e));
                }

                TryRaiseConnectionStateChanged();

                return connection.State == RnetConnectionState.Open;
            }
        }

        /// <summary>
        /// Receives messages from the connection.
        /// </summary>
        async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // attempt to restablish a connection if it was lost
                    while (!await EnsureConnection(cancellationToken) && !cancellationToken.IsCancellationRequested)
                        continue;

                    // read next message
                    var message = await connection.ReceiveAsync(cancellationToken);
                    if (message != null)
                        OnMessageReceived(new RnetMessageEventArgs(message));
                }
                catch (RnetConnectionException e)
                {
                    if (ConnectionState == RnetConnectionState.Open)
                        OnError(new RnetClientErrorEventArgs(e));
                }
                catch (Exception e)
                {
                    OnError(new RnetClientErrorEventArgs(e));
                }
            }
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task SendAsync(RnetMessage message)
        {
            return SendAsync(message, CancellationToken.None);
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SendAsync(RnetMessage message, CancellationToken cancellationToken)
        {
            // attempt to restablish a connection if it was lost
            while (!await EnsureConnection(cancellationToken) && !cancellationToken.IsCancellationRequested)
                continue;

            // send message
            await connection.SendAsync(message, cancellationToken);

            // successfully sent
            OnMessageSent(new RnetMessageEventArgs(message));
        }

        /// <summary>
        /// Raised when a message is sent.
        /// </summary>
        public event EventHandler<RnetMessageEventArgs> MessageSent;

        /// <summary>
        /// Raises the MessageSent event.
        /// </summary>
        /// <param name="args"></param>
        void OnMessageSent(RnetMessageEventArgs args)
        {
            if (MessageSent != null)
                MessageSent(this, args);
        }

        /// <summary>
        /// Raised when a new message is received.
        /// </summary>
        public event EventHandler<RnetMessageEventArgs> MessageReceived;

        /// <summary>
        /// Raises the MessageReceived event.
        /// </summary>
        /// <param name="args"></param>
        void OnMessageReceived(RnetMessageEventArgs args)
        {
            if (MessageReceived != null)
                MessageReceived(this, args);
        }

        /// <summary>
        /// Raised when an error occurs.
        /// </summary>
        public event EventHandler<RnetClientErrorEventArgs> Error;

        /// <summary>
        /// Raises the Error event.
        /// </summary>
        /// <param name="args"></param>
        void OnError(RnetClientErrorEventArgs args)
        {
            if (Error != null)
                Error(this, args);
        }

        /// <summary>
        /// Raised when the state changes.
        /// </summary>
        public event EventHandler<RnetClientStateEventArgs> StateChanged;

        /// <summary>
        /// Raises the StateChanged event.
        /// </summary>
        /// <param name="args"></param>
        void OnStateChanged(RnetClientStateEventArgs args)
        {
            if (StateChanged != null)
                StateChanged(this, args);
            RaisePropertyChanged("State");
        }

        /// <summary>
        /// Raised when the connection state changes.
        /// </summary>
        public event EventHandler<RnetConnectionStateEventArgs> ConnectionStateChanged;

        /// <summary>
        /// Raises the ConnectionStateChanged event.
        /// </summary>
        /// <param name="args"></param>
        void OnConnectionStateChanged(RnetConnectionStateEventArgs args)
        {
            if (ConnectionStateChanged != null)
                ConnectionStateChanged(this, args);
            RaisePropertyChanged("ConnectionState");
        }

        /// <summary>
        /// Raises the ConnectionStateChanged event if required.
        /// </summary>
        void TryRaiseConnectionStateChanged()
        {
            if (ConnectionState != lastConnectionState)
                OnConnectionStateChanged(new RnetConnectionStateEventArgs(lastConnectionState = ConnectionState));
        }

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        public void Dispose()
        {
            StopAsync().Wait();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes the instance.
        /// </summary>
        ~RnetClient()
        {
            Dispose();
        }

    }

}
