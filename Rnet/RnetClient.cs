using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Rnet
{

    /// <summary>
    /// Provides a durable interface to send and receive RNET messages around an existing <see cref="RnetConnection"/>.
    /// </summary>
    public class RnetClient : IDisposable
    {

        AsyncLock lck = new AsyncLock();
        CancellationTokenSource cts;
        Task receiveTask;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="connection"></param>
        public RnetClient(RnetConnection connection)
        {
            Contract.Requires<ArgumentNullException>(connection != null);

            Connection = connection;
            State = RnetClientState.Stopped;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="uri"></param>
        public RnetClient(Uri uri)
            : this(RnetConnection.Create(uri))
        {
            Contract.Requires<ArgumentNullException>(uri != null);
        }

        /// <summary>
        /// Underlying connection.
        /// </summary>
        public RnetConnection Connection { get; private set; }

        /// <summary>
        /// Gets the current state of the <see cref="RnetClient"/>.
        /// </summary>
        public RnetClientState State { get; private set; }

        /// <summary>
        /// Starts the client.
        /// </summary>
        /// <returns></returns>
        public Task Start()
        {
            return Start(CancellationToken.None);
        }

        /// <summary>
        /// Starts the client.
        /// </summary>
        public async Task Start(CancellationToken cancellationToken)
        {
            using (await lck.LockAsync())
            {
                if (State != RnetClientState.Stopped)
                    throw new RnetException("Client is already started.");

                // start up receiver using a background task
                cts = new CancellationTokenSource();
                receiveTask = Task.Run(async () => await ReceiveLoop(cts.Token));

                // currenty started
                State = RnetClientState.Started;
            }

            OnStateChanged(new RnetClientStateEventArgs(State));
        }

        /// <summary>
        /// Stops the client.
        /// </summary>
        /// <returns></returns>
        public Task Stop()
        {
            return Stop(CancellationToken.None);
        }

        /// <summary>
        /// Stops the client.
        /// </summary>
        public async Task Stop(CancellationToken cancellationToken)
        {
            using (await lck.LockAsync())
            {
                if (State != RnetClientState.Started)
                    return;

                // signal shutdown
                cts.Cancel();
                State = RnetClientState.Stopped;

                // wait for receiving task to terminate
                await receiveTask;
                receiveTask = null;
            }

            OnStateChanged(new RnetClientStateEventArgs(State));
        }

        /// <summary>
        /// Attempts to open the connection if it is not yet open.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task OpenConnection(CancellationToken cancellationToken)
        {
            using (await lck.LockAsync(cancellationToken))
            {
                // connection currently open
                if (Connection.State == RnetConnectionState.Open)
                    return;

                try
                {
                    // attempt to open connection
                    RnetTraceSource.Default.TraceInformation("RnetClient:OpenConnection opening");
                    await Connection.Open(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Receives messages from the connection until cancelled.
        /// </summary>
        async Task ReceiveLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // attempt to restablish a connection if it was lost
                    while (Connection.State != RnetConnectionState.Open && !cancellationToken.IsCancellationRequested)
                        await OpenConnection(cancellationToken);

                    // check for cancelled
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    // read next message
                    var message = await Connection.Read(cancellationToken);
                    if (message != null)
                        OnMessageReceived(new RnetMessageEventArgs(message));
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    OnUnhandledException(new RnetExceptionEventArgs(e));
                }
            }
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task Send(RnetMessage message)
        {
            Contract.Requires<ArgumentNullException>(message != null);

            return Send(message, CancellationToken.None);
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Send(RnetMessage message, CancellationToken cancellationToken)
        {
            Contract.Requires<ArgumentNullException>(message != null);

            // attempt to restablish a connection if it was lost
            while (Connection.State != RnetConnectionState.Open && !cancellationToken.IsCancellationRequested)
                await OpenConnection(cancellationToken);

            // check for cancelled
            if (cancellationToken.IsCancellationRequested)
                return;

            // send message
            await Connection.Send(message, cancellationToken);

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
            Contract.Requires<ArgumentNullException>(args != null);
            Contract.Requires<NullReferenceException>(args.Message != null);
            RnetTraceSource.Default.TraceEvent(TraceEventType.Verbose, -1, "RnetClient:OnMessageSent Message={0}", args.Message);

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
            Contract.Requires<ArgumentNullException>(args != null);
            Contract.Requires<NullReferenceException>(args.Message != null);
            RnetTraceSource.Default.TraceEvent(TraceEventType.Verbose, -1, "RnetClient:OnMessageReceived Message={0}", args.Message);

            if (MessageReceived != null)
                MessageReceived(this, args);
        }

        /// <summary>
        /// Raised when an exception occurs during the operation of the client that the client could not recover from.
        /// </summary>
        public event EventHandler<RnetExceptionEventArgs> UnhandledException;

        /// <summary>
        /// Raises the UnhandledException event.
        /// </summary>
        /// <param name="args"></param>
        void OnUnhandledException(RnetExceptionEventArgs args)
        {
            Contract.Requires<ArgumentNullException>(args != null);
            Contract.Requires<NullReferenceException>(args.Exception != null);
            RnetTraceSource.Default.TraceInformation("RnetClient:OnUnhandledException ExceptionType={0}", args.Exception.GetType());

            if (UnhandledException != null)
                UnhandledException(this, args);
        }

        /// <summary>
        /// Raised when the client state changes.
        /// </summary>
        public event EventHandler<RnetClientStateEventArgs> StateChanged;

        /// <summary>
        /// Raises the StateChanged event.
        /// </summary>
        /// <param name="args"></param>
        void OnStateChanged(RnetClientStateEventArgs args)
        {
            Contract.Requires<ArgumentNullException>(args != null);
            RnetTraceSource.Default.TraceInformation("RnetClient:OnStateChanged State={0}", args.State);

            if (StateChanged != null)
                StateChanged(this, args);
        }

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (State != RnetClientState.Stopped)
                    Stop().Wait(2000);
            }
            catch (Exception e)
            {
                Trace.Fail("Exception in Dispose", e.ToString());
            }

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
