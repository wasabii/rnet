using System.IO;

namespace Rnet
{

    /// <summary>
    /// Provides methods by which to write an RNet message. Create a new instance for each message to be written.
    /// </summary>
    class RnetMessageWriter
    {

        int len;
        int sum;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="rnet"></param>
        internal RnetMessageWriter(RnetConnection rnet)
            : this(rnet.Stream)
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="rnet"></param>
        internal RnetMessageWriter(Stream stream)
        {
            Stream = stream;
        }

        /// <summary>
        /// Gets a reference to the target <see cref="Stream"/>
        /// </summary>
        public Stream Stream { get; private set; }

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
        /// Writes a Device ID to the RNet device.
        /// </summary>
        /// <param name="deviceId"></param>
        internal void WriteDeviceId(RnetDeviceId deviceId)
        {
            WriteByte(deviceId.ControllerId);
            WriteByte(deviceId.ZoneId);
            WriteByte(deviceId.KeypadId);
        }

        /// <summary>
        /// Writes a Message Type to the RNet device.
        /// </summary>
        /// <param name="messageType"></param>
        internal void WriteMessageType(RnetMessageType messageType)
        {
            WriteByte((byte)messageType);
        }

        /// <summary>
        /// Writes a path to the RNet device.
        /// </summary>
        /// <param name="path"></param>
        internal void WritePath(RnetPath path)
        {
            if (path == null)
            {
                WriteByte(0x00);
            }
            else
            {
                // unpack path in top-first order
                var paths = new RnetPath[path.Level];
                for (int i = path.Level - 1; i >= 0; i--)
                {
                    paths[i] = path;
                    path = path.Previous;
                }

                // number of items in path
                WriteByte(paths[paths.Length - 1].Level);

                // write each item's directory
                foreach (var item in paths)
                    WriteByte(item.Directory);
            }
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

        /// <summary>
        /// Writes data to the connection.
        /// </summary>
        /// <param name="message"></param>
        internal void WriteMessage(params byte[] message)
        {
            WriteStart();
            WriteBody(message);
            WriteChecksum();
            WriteEnd();
        }

    }

}
