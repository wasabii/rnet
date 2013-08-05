using System.IO;

namespace Rnet
{

    /// <summary>
    /// Provides a temporary buffer for incoming set data messages.
    /// </summary>
    class RnetDataHandleWriter
    {

        MemoryStream stream;
        int packetCount;
        int packetNumber = -1;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetDataHandleWriter(int packetCount)
        {
            this.stream = new MemoryStream();
            this.packetCount = packetCount;
        }

        /// <summary>
        /// Receives the data packet.
        /// </summary>
        /// <param name="data"></param>
        public void Write(byte[] data, int packetNumber)
        {
            // skip if out of order packet
            if (this.packetNumber != packetNumber - 1)
                return;

            this.stream.Write(data, 0, data.Length);
            this.packetNumber = packetNumber;
        }

        /// <summary>
        /// Gets whether or not the buffer is complete.
        /// </summary>
        public bool IsComplete
        {
            get { return packetNumber == packetCount - 1; }
        }

        /// <summary>
        /// Gets the byte array that has been buffered so far.
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return stream.ToArray();
        }

    }

}
