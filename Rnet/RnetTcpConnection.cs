using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    public sealed class RnetTcpConnection : RnetConnection
    {

        IPEndPoint ep;
        TcpClient tcp;
        RnetStreamReader reader;
        RnetStreamWriter writer;

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

        protected override void Connect()
        {
            ConnectAsync().Wait();
        }

        protected override async Task ConnectAsync()
        {
            // initialize new TCP client and connect
            tcp = new TcpClient();
            tcp.ReceiveTimeout = 5000;
            await tcp.ConnectAsync(ep.Address, ep.Port);

            // initialize reader and writer
            reader = new RnetStreamReader(tcp.GetStream());
            writer = new RnetStreamWriter(tcp.GetStream());
        }

        protected override void Disconnect()
        {
            DisconnectAsync().Wait();
        }

        protected override Task DisconnectAsync()
        {
            if (tcp != null)
            {
                tcp.Close();
                tcp = null;
            }

            return Task.FromResult(0);
        }

        protected override RnetStreamReader GetReader()
        {
            return reader;
        }

        protected override RnetStreamWriter GetWriter()
        {
            return writer;
        }

        public override RnetConnectionState State
        {
            get { return IsConnected ? RnetConnectionState.Open : RnetConnectionState.Closed; }
        }

        public override RnetMessage Receive()
        {
            return ReceiveAsync().Result;
        }

        public override Task<RnetMessage> ReceiveAsync()
        {
            return ReceiveAsync(CancellationToken.None);
        }

        public async override Task<RnetMessage> ReceiveAsync(CancellationToken cancellationToken)
        {
            // ignore timeout exceptions
            while (!cancellationToken.IsCancellationRequested)
            {
                SocketException exception = null;

                try
                {
                    return await base.ReceiveAsync(cancellationToken);
                }
                catch (IOException e)
                {
                    exception = e.InnerException as SocketException;
                }
                catch (SocketException e)
                {
                    exception = e;
                }
                catch (RnetException e)
                {
                    ExceptionDispatchInfo.Capture(e).Throw();
                }

                // timeout exception received, ignore it and continue
                if (exception != null)
                    if (exception.SocketErrorCode == SocketError.TimedOut)
                        continue;

                throw new RnetConnectionException("Unable to receive message.", exception);
            }

            cancellationToken.ThrowIfCancellationRequested();

            // unreachable
            return null;
        }

        /// <summary>
        /// Determines whether or not the <see cref="TcpClient"/> is currently connected.
        /// </summary>
        bool IsConnected
        {
            get
            {
                try
                {
                    if (tcp != null &&
                        tcp.Client != null &&
                        tcp.Client.Connected)
                    {
                        if (tcp.Client.Poll(0, SelectMode.SelectRead))
                        {
                            var buf = new byte[1];
                            return tcp.Client.Receive(buf, SocketFlags.Peek) == 0 ? false : true;
                        }
                        else
                            return true;
                    }
                    else
                        return false;
                }
                catch
                {
                    return false;
                }
            }
        }

    }

}
