using System;
using System.IO;
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
        string host;
        int port;
        TcpClient tcp;

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
            this.host = hostname;
            this.port = port;
        }

        protected override async Task ConnectAsync()
        {
            // initialize new TCP client and connect
            tcp = new TcpClient();
            tcp.ReceiveTimeout = 5000;

            if (ep != null)
                // known endpoint
                await tcp.ConnectAsync(ep.Address, ep.Port);
            else
                // known host
                await tcp.ConnectAsync(host, port);
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
            return new RnetStreamReader(tcp.GetStream());
        }

        protected override RnetStreamWriter GetWriter()
        {
            return new RnetStreamWriter(tcp.GetStream());
        }

        public override RnetConnectionState State
        {
            get { return Connected ? RnetConnectionState.Open : RnetConnectionState.Closed; }
        }

        public async override Task<RnetMessage> ReceiveAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Exception exception = null;

                try
                {
                    return await base.ReceiveAsync(cancellationToken);
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
