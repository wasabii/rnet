using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Provides a connection to an RNET device operating over TCP.
    /// </summary>
    public sealed class RnetTcpConnection : RnetConnection
    {

        IPEndPoint ep;
        string host;
        int port;
        TcpClient tcp;

        /// <summary>
        /// Initializes a new connection to RNET.
        /// </summary>
        /// <param name="uri"></param>
        public RnetTcpConnection(Uri uri)
        {
            Contract.Requires<ArgumentNullException>(uri != null);
            Contract.Requires<UriFormatException>(uri.Scheme == "rnet.tcp", "Schema of URI must be 'rnet.tcp'.");

            if (uri.HostNameType == UriHostNameType.Dns ||
                uri.HostNameType == UriHostNameType.Basic ||
                uri.HostNameType == UriHostNameType.Unknown)
            {
                host = uri.DnsSafeHost;
                port = uri.Port;
                if (string.IsNullOrWhiteSpace(host) || port < 0 || port > 65535)
                    throw new RnetConnectionException("Could not discover valid host and/or port.");

                return;
            }
            if (uri.HostNameType == UriHostNameType.IPv4)
            {
                ep = new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port);
                return;
            }
        }

        /// <summary>
        /// Initializes a new connection to RNET.
        /// </summary>
        /// <param name="ep"></param>
        public RnetTcpConnection(IPEndPoint ep)
        {
            Contract.Requires(ep != null);
            Contract.Ensures(this.ep != null);

            this.ep = ep;
        }

        /// <summary>
        /// Initializes a new connection to RNET.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public RnetTcpConnection(IPAddress ip, int port)
            : this(new IPEndPoint(ip, port))
        {
            Contract.Requires(ip != null);
            Contract.Requires(port >= 0);
            Contract.Requires(port <= 65535);
        }

        /// <summary>
        /// Initializes a new connection to RNET.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public RnetTcpConnection(string host, int port)
        {
            Contract.Requires(host != null);
            Contract.Requires(port >= 0);
            Contract.Requires(port <= 65535);

            this.host = host;
            this.port = port;
        }

        protected override async Task Connect(CancellationToken cancellationToken)
        {
            Contract.Ensures(tcp != null);
            Contract.Assert(ep != null || host != null && port > 0 && port < 65535);

            // initialize new TCP client and connect
            tcp = new TcpClient();
            tcp.ReceiveTimeout = 2000;

            if (ep != null)
                // known endpoint
                await tcp.ConnectAsync(ep.Address, ep.Port);
            else
                // known host
                await tcp.ConnectAsync(host, port);
        }

        protected override Task Disconnect(CancellationToken cancellationToken)
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
            Contract.Assert(tcp != null);
            return new RnetStreamReader(tcp.GetStream());
        }

        protected override RnetStreamWriter GetWriter()
        {
            Contract.Assert(tcp != null);
            return new RnetStreamWriter(tcp.GetStream());
        }

        public override RnetConnectionState State
        {
            get { return Connected ? RnetConnectionState.Open : RnetConnectionState.Closed; }
        }

        public async override Task<RnetMessage> Read(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Exception exception = null;

                try
                {
                    return await base.Read(cancellationToken);
                }
                catch (EndOfStreamException e)
                {
                    exception = e;
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
                {
                    if (exception is SocketException && ((SocketException)exception).SocketErrorCode == SocketError.TimedOut)
                        continue;
                    if (exception is EndOfStreamException)
                        continue;
                }

                throw new RnetConnectionException("Unable to receive message.", exception);
            }
        }

        /// <summary>
        /// Determines whether or not the <see cref="TcpClient"/> is currently connected.
        /// </summary>
        bool Connected
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
