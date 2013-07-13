using System;
using System.IO;

namespace Rnet
{

    public class RnetDataItem
    {

        DateTime timestamp;
        byte[] buffer;
        MemoryStream stream;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="path"></param>
        internal RnetDataItem(RnetPath path)
        {
            Path = path;
        }

        /// <summary>
        /// Path to the data item.
        /// </summary>
        public RnetPath Path { get; private set; }

        /// <summary>
        /// Clears any active input buffer.
        /// </summary>
        public void WriteBegin()
        {
            stream = new MemoryStream();
        }

        /// <summary>
        /// Writes the data.
        /// </summary>
        /// <param name="data"></param>
        public void Write(byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Closes the stream and sets the new data as the current data.
        /// </summary>
        public void WriteEnd()
        {
            Timestamp = DateTime.UtcNow;
            Buffer = stream.ToArray();
            stream = null;
        }

        /// <summary>
        /// Gets the active data.
        /// </summary>
        public byte[] Buffer { get; private set; }

        /// <summary>
        /// Gets the timestamp the data was entered.
        /// </summary>
        public DateTime Timestamp { get; private set; }

    }

}
