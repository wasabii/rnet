using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Provides a durable wrapper around a <see cref="RnetConnection"/>.
    /// </summary>
    public class RnetClient : IDisposable
    {

        /// <summary>
        /// Currently active connection.
        /// </summary>
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
        /// Queued messages to send.
        /// </summary>
        BlockingCollection<RnetMessage> sendQueue = new BlockingCollection<RnetMessage>();

        /// <summary>
        /// Queued messages to receive.
        /// </summary>
        BlockingCollection<object> receiveQueue = new BlockingCollection<object>();

        /// <summary>
        /// Queued errors.
        /// </summary>
        BlockingCollection<object> errorQueue = new BlockingCollection<object>();

        /// <summary>
        /// Sends messages to the connection.
        /// </summary>
        Thread sendThread;

        /// <summary>
        /// Receives messages from the connection.
        /// </summary>
        Thread receiveThread;

        /// <summary>
        /// Dispatches received events to the application.
        /// </summary>
        Thread dispatchThread;

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
        /// Starts the <see cref="RnetClient"/>.
        /// </summary>
        public void Start()
        {
            // already started
            if (cts != null)
                return;

            cts = new CancellationTokenSource();
            sendThread = new Thread(SendThreadStart);
            sendThread.Start();
            receiveThread = new Thread(ReceiveThreadStart);
            receiveThread.Start();
            dispatchThread = new Thread(DispatchThreadStart);
            dispatchThread.Start();

            OnStateChanged(new RnetClientStateEventArgs(State));
        }

        /// <summary>
        /// Stops the <see cref="RnetClient"/>.
        /// </summary>
        public void Stop()
        {
            if (cts == null)
                return;

            // signal cancel and wait for shutdown
            cts.Cancel();
            sendThread.Join();
            receiveThread.Join();
            dispatchThread.Join();

            // clean up instances
            cts = null;
            sendThread = null;
            receiveThread = null;
            dispatchThread = null;

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
        void EnsureConnection(CancellationToken ct)
        {
            lock (connection)
            {
                // dispose of existing connection if it has been shut down
                if (connection.State != RnetConnectionState.Open)
                {
                    TryRaiseConnectionStateChanged();

                    // wait for connection back off
                    while (TimeSpan.FromSeconds(30) > (DateTime.UtcNow - lastConnectionAttempt))
                        Monitor.Wait(connection, 5000);
                    lastConnectionAttempt = DateTime.UtcNow;

                    // double check
                    if (connection.State == RnetConnectionState.Open)
                        return;

                    // attempt to open connection
                    connection.Open();
                    TryRaiseConnectionStateChanged();

                    // reset to lowest value so we'll reconnect immediately next time around
                    lastConnectionAttempt = DateTime.MinValue;
                }
            }
        }

        /// <summary>
        /// Sends messages to the connection.
        /// </summary>
        void SendThreadStart()
        {
            // cache token
            var ct = cts.Token;

            while (!ct.IsCancellationRequested)
            {
                RnetMessage message = null;

                try
                {
                    EnsureConnection(ct);

                    if (sendQueue.TryTake(out message, 1000))
                        connection.Send(message);
                }
                catch (Exception e)
                {
                    try
                    {
                        // attempt to close the existing connection
                        lock (connection)
                            connection.Close();
                    }
                    catch (Exception e2)
                    {
                        errorQueue.Add(e2);
                    }

                    // add message back into queue
                    if (message != null)
                        sendQueue.Add(message);

                    errorQueue.Add(e);
                }
            }
        }

        /// <summary>
        /// Receives messages from the connection.
        /// </summary>
        void ReceiveThreadStart()
        {
            // cache token
            var ct = cts.Token;

            while (!ct.IsCancellationRequested)
            {
                RnetConnection cnn = null;

                try
                {
                    EnsureConnection(ct);

                    var message = connection.Receive();
                    if (message != null)
                        receiveQueue.Add(message);
                }
                catch (RnetConnectionException e)
                {
                    if (ConnectionState == RnetConnectionState.Open)
                        errorQueue.Add(e);
                }
                catch (Exception e)
                {
                    try
                    {
                        // attempt to close the existing connection
                        lock (connection)
                            cnn.Close();
                    }
                    catch (Exception e2)
                    {
                        errorQueue.Add(e2);
                    }

                    errorQueue.Add(e);
                }
            }
        }

        void DispatchThreadStart()
        {
            // cache token
            var ct = cts.Token;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    object o = null;

                    // wait for any new event
                    BlockingCollection<object>.TakeFromAny(
                        new BlockingCollection<object>[] { receiveQueue, errorQueue },
                        out o, ct);

                    var m = o as RnetMessage;
                    if (m != null)
                        OnMessageReceived(new RnetMessageEventArgs(m));

                    var e = o as Exception;
                    if (e != null)
                        OnError(new RnetClientErrorEventArgs(e));
                }
                catch (Exception e)
                {
                    try
                    {
                        OnError(new RnetClientErrorEventArgs(e));
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }
            }
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
            Stop();
        }

    }

}
