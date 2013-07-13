using System;
using System.IO;

namespace Rnet.Protocol
{

    /// <summary>
    /// Provides methods by which to write RNet messages to a <see cref="Stream"/>.
    /// </summary>
    public class RnetStreamWriter
    {

        int len = -1;
        int sum = -1;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="rnet"></param>
        internal RnetStreamWriter(Stream stream)
        {
            Stream = stream;
        }

        /// <summary>
        /// Gets a reference to the target <see cref="Stream"/>
        /// </summary>
        public Stream Stream { get; private set; }

        /// <summary>
        /// Begins a new message.
        /// </summary>
        internal void BeginMessage(RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId, RnetMessageType messageType)
        {
            if (len != -1 || sum != -1)
                throw new InvalidOperationException("A message is already in progress.");

            len = 0;
            sum = 0;

            WriteStart();
            targetDeviceId.Write(this);
            sourceDeviceId.Write(this);
            WriteByte((byte)messageType);
        }

        /// <summary>
        /// Ends the message in progress.
        /// </summary>
        internal void EndMessage()
        {
            if (len == -1 || sum == -1)
                throw new InvalidOperationException("A message has not been started.");

            WriteChecksum();
            WriteEnd();

            len = -1;
            sum = -1;
        }

        /// <summary>
        /// Writes a single byte to the RNet stream.
        /// </summary>
        /// <param name="b"></param>
        internal void WriteRaw(byte b)
        {
            Stream.WriteByte(b);
        }

        /// <summary>
        /// Writes a single byte to the RNet stream, without checking for overflow.
        /// </summary>
        /// <param name="b"></param>
        internal void WriteMessageByte(byte b)
        {
            len++;
            sum += b;
            WriteRaw(b);
        }

        /// <summary>
        /// Writes a message start to the RNet stream.
        /// </summary>
        internal void WriteStart()
        {
            WriteMessageByte((byte)RnetSpecialMessageChars.MessageStart);
        }

        /// <summary>
        /// Writes the checksum byte to the RNet stream.
        /// </summary>
        internal void WriteChecksum()
        {
            // checksum is sum + len, 7 bits
            WriteRaw((byte)((sum + len) & 0x7f));
        }

        /// <summary>
        /// Writes a message end to the RNet stream.
        /// </summary>
        internal void WriteEnd()
        {
            WriteRaw((byte)RnetSpecialMessageChars.MessageEnd);
        }

        /// <summary>
        /// Writes a single message body byte to the RNet stream.
        /// </summary>
        /// <param name="b"></param>
        internal void WriteByte(byte b)
        {
            if ((b & 0x80) != 0x00)
            {
                // byte has high bit set, invert
                WriteMessageByte((byte)RnetSpecialMessageChars.Invert);
                WriteMessageByte((byte)(b ^ 0xff));
            }
            else
            {
                WriteMessageByte(b);
            }
        }

        /// <summary>
        /// Writes a portion of a message.
        /// </summary>
        /// <param name="buffer"></param>
        internal void WriteBody(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
                WriteMessageByte(buffer[i]);
        }
        
        /// <summary>
        /// Writes <see cref="UInt16"/> value to the RNet device. RNet requires the low and high bytes to be swapped.
        /// </summary>
        /// <param name="value"></param>
        internal void WriteUInt16(ushort value)
        {
            var lo = (byte)((value & 0x00FF));
            var hi = (byte)((value & 0xFF00) >> 8);
            WriteByte(lo);
            WriteByte(hi);
        }

    }

}
