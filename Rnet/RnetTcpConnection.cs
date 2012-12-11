using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Rnet
{

    public sealed class RnetTcpConnection : RnetConnection
    {

        IPEndPoint ep;
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
            var ip = Dns.GetHostAddresses(hostname).FirstOrDefault();
            if (ip == null)
                throw new Exception("Could not resolve hostname.");

            ep = new IPEndPoint(ip, port);
        }

        protected override Stream Stream
        {
            get { return tcp.GetStream(); }
        }

        public override bool IsOpen
        {
            get { return tcp.Connected; }
        }

        public override void Open()
        {
            tcp = new TcpClient();
            tcp.Connect(ep);
        }

        public override void Close()
        {
            Dispose();
        }

        public override void Dispose()
        {
            if (tcp != null)
            {
                tcp.Close();
                tcp = null;
            }
        }
    }

}
