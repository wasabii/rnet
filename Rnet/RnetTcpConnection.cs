using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Rnet
{

    public sealed class RnetTcpConnection : RnetConnection
    {

        IPEndPoint ep;
        TcpClient tcp;
        Thread receiverThread;
        CancellationTokenSource cts;
        CancellationToken ct;

        /// <summary>
        /// Initializes a new connection to an RNET device at the given endpoint.
        /// </summary>
        /// <param name="ep"></param>
        public RnetTcpConnection(IPEndPoint ep)
        {
            this.ep = ep;
        }

        /// <summary>
        /// Initializes a new connection to an RNET device at the given IP address and port.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public RnetTcpConnection(IPAddress ip, int port)
            : this(new IPEndPoint(ip, port))
        {

        }

        /// <summary>
        /// Initializes a new connection to the RNET device at the given host name and port.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        public RnetTcpConnection(string hostname, int port)
        {
            var ip = Dns.GetHostAddresses(hostname).FirstOrDefault();
            if (ip == null)
                throw new Exception("Could not resolve hostname.");

            ep = new IPEndPoint(ip, port);
        }

        internal override Stream Stream
        {
            get { return tcp.GetStream(); }
        }

        public override bool IsOpen
        {
            get { return tcp.Connected; }
        }

        public override void Open()
        {
            // initialize new TCP client and connect
            tcp = new TcpClient();
            tcp.Connect(ep);

            // to signal receiver thread to shutdown
            cts = new CancellationTokenSource();
            ct = cts.Token;

            // start receiving messages
            receiverThread = new Thread(ReceiverThreadMain);
            receiverThread.Start();
        }

        /// <summary>
        /// Entry point for the receiver thread.
        /// </summary>
        void ReceiverThreadMain()
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var message = Reader.TryReadMessage();
                    if (message != null)
                        OnMessageReceived(new RnetMessageReceivedEventArgs(message));
                }
                catch (Exception)
                {
                    // cancel if connection was dropped
                    if (!tcp.Connected)
                        cts.Cancel();
                }
            }
        }

        public override void Close()
        {
            Dispose();
        }

        public override void Dispose()
        {
            if (cts != null)
            {
                // signal the thread to stop, and wait for it
                cts.Cancel();
                receiverThread.Abort();
                receiverThread.Join();
                cts = null;
                receiverThread = null;
            }

            if (tcp != null)
            {
                tcp.Close();
                tcp = null;
            }

            base.Dispose();
        }
    }

}
