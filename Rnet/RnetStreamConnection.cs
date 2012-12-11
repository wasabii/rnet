using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rnet
{

    public sealed class RnetStreamConnection : RnetConnection
    {

        Stream stream;
        bool open;

        /// <summary>
        /// Initializes a new connection to RNET using a stream.
        /// </summary>
        /// <param name="stream"></param>
        public RnetStreamConnection(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            this.stream = stream;
        }

        protected override Stream Stream
        {
            get { return stream; }
        }

        public override void Open()
        {
            if (open)
                throw new InvalidOperationException("Connection is already open.");

            if (stream == null)
                throw new ObjectDisposedException("RnetStreamConnection");

            open = true;
        }

        public override bool IsOpen
        {
            get { return open; }
        }

        public override void Close()
        {
            Dispose();
        }

        public override void Dispose()
        {
            if (stream != null)
            {
                stream.Dispose();
                stream = null;
            }

            open = false;
        }

    }

}
