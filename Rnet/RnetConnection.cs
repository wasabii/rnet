using System;
using System.IO;

namespace Rnet
{

    /// <summary>
    /// Provides the core RNET implementation.
    /// </summary>
    public abstract class RnetConnection : IDisposable
    {

        /// <summary>
        /// Initializes a new connection that communicates with the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream"></param>
        protected RnetConnection()
        {

        }

        /// <summary>
        /// Stream providing access to RNET.
        /// </summary>
        protected abstract Stream Stream { get; }

        /// <summary>
        /// Opens the connection to RNET.
        /// </summary>
        public abstract void Open();

        /// <summary>
        /// Gets whether or not the RNET connection is open.
        /// </summary>
        public abstract bool IsOpen { get; }

        /// <summary>
        /// Closes the current connection.
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// Disposes of the current connection.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Checks whether the connection has been opened.
        /// </summary>
        void CheckOpen()
        {
            if (!IsOpen)
                throw new InvalidOperationException("RnetConnection has not been opened.");
        }

        /// <summary>
        /// Writes a single byte to the RNet stream.
        /// </summary>
        /// <param name="b"></param>
        internal void WriteByte(byte b)
        {
            Stream.WriteByte(b);
        }

        /// <summary>
        /// Writes a single byte to the RNet stream, without checking for overflow.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="len"></param>
        /// <param name="sum"></param>
        internal void WriteMessageByte(byte b, ref int len, ref int sum)
        {
            len++;
            sum += b;
            WriteByte(b);
        }

        /// <summary>
        /// Writes a message start to the RNet stream.
        /// </summary>
        /// <param name="len"></param>
        /// <param name="sum"></param>
        internal void WriteMessageStart(ref int len, ref int sum)
        {
            WriteMessageByte((byte)RnetSpecialChars.MessageStart, ref len, ref sum);
        }

        /// <summary>
        /// Writes the checksum byte to the RNet stream.
        /// </summary>
        /// <param name="len"></param>
        /// <param name="sum"></param>
        internal void WriteMessageChecksum(int len, int sum)
        {
            // checksum is sum + len, 7 bits
            WriteByte((byte)((sum + len) & 0x7f));
        }

        /// <summary>
        /// Writes a message end to the RNet stream.
        /// </summary>
        internal void WriteMessageEnd()
        {
            WriteByte((byte)RnetSpecialChars.MessageEnd);
        }

        /// <summary>
        /// Writes a single message body byte to the RNet stream.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="len"></param>
        /// <param name="sum"></param>
        internal void WriteMessageBodyByte(byte b, ref int len, ref int sum)
        {
            if ((b & 0x80) != 0x00)
            {
                // byte has high bit set, invert
                WriteMessageByte((byte)RnetSpecialChars.Invert, ref len, ref sum);
                WriteMessageByte((byte)(b ^ 0xff), ref len, ref sum);
            }
            else
            {
                WriteMessageByte(b, ref len, ref sum);
            }
        }

        /// <summary>
        /// Writes a portion of a message.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="len"></param>
        /// <param name="sum"></param>
        internal void WriteMessageBodyBytes(byte[] buffer, ref int len, ref int sum)
        {
            for (int i = 0; i < buffer.Length; i++)
                WriteMessageByte(buffer[i], ref len, ref sum);
        }

        /// <summary>
        /// Writes data to the connection.
        /// </summary>
        /// <param name="message"></param>
        internal void WriteMessage(params byte[] message)
        {
            int len = 0;
            int sum = 0;

            WriteMessageStart(ref len, ref sum);
            WriteMessageBodyBytes(message, ref len, ref sum);
            WriteMessageChecksum(len, sum);
            WriteMessageEnd();
        }

    }

}
