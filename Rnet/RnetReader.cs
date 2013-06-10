using System;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace Rnet
{

    /// <summary>
    /// Provides methods by which to read messages from an RNet <see cref="Stream"/>.
    /// </summary>
    public class RnetReader
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="stream"></param>
        public RnetReader(Stream stream)
        {
            Stream = stream;
        }

        /// <summary>
        /// Gets a reference to the source <see cref="Stream"/>
        /// </summary>
        public Stream Stream { get; private set; }

        /// <summary>
        /// Attempts to read a message from the RNet connection, or returns <c>null</c> if unable to.
        /// </summary>
        /// <returns></returns>
        internal RnetMessage TryReadMessage()
        {
            try
            {
                // begin new buffer and such
                byte[] buffer = null;
                int length = 0;

                int b;

                // advance until message start character
                while ((b = Stream.ReadByte()) != -1)
                    if ((byte)b == (byte)RnetSpecialMessageChars.MessageStart)
                    {
                        // begin new message
                        buffer = new byte[1024];
                        buffer[length++] = (byte)b;
                        break;
                    }

                // advance until message end character
                while ((b = Stream.ReadByte()) != -1)
                {
                    // add byte to buffer
                    buffer[length++] = (byte)b;

                    if ((byte)b == (byte)RnetSpecialMessageChars.MessageEnd)
                    {
                        // calculate checksum
                        int len = length - 2;
                        int sum = buffer.Take(len).Sum(i => i);
                        int chk = (byte)((sum + len) & 0x7f);

                        // check that checksum is valid, else discard message
                        if (chk != buffer[length - 2])
                            break;

                        // extract message body
                        var body = new byte[length - 3];
                        Array.Copy(buffer, 1, body, 0, length - 3);

                        // body isn't long enough to contain an actual message
                        if (body.Length < 7)
                            break;

                        // attempt to decode, parse and return the message body
                        return ParseMessage(new RnetMessageBodyReader(new MemoryStream(body)));
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // ignore timeouts
            }

            // no message read
            return null;
        }

        /// <summary>
        /// Attempts to parse the given RNet message body.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        RnetMessage ParseMessage(RnetMessageBodyReader reader)
        {
            try
            {
                // parse device ids
                var targetDeviceId = RnetDeviceId.Read(reader);
                var sourceDeviceId = RnetDeviceId.Read(reader);
                var messageType = (RnetMessageType)reader.ReadByte();

                switch (messageType)
                {
                    case RnetMessageType.Event:
                        return RnetEventMessage.Read(reader, targetDeviceId, sourceDeviceId);
                    case RnetMessageType.RequestData:
                        return RnetRequestDataMessage.Read(reader, targetDeviceId, sourceDeviceId);
                    case RnetMessageType.SetData:
                        return RnetSetDataMessage.Read(reader, targetDeviceId, sourceDeviceId);
                    case RnetMessageType.Handshake:
                        return RnetHandshakeMessage.Read(reader, targetDeviceId, sourceDeviceId);
                    default:
                        throw new RnetProtocolException("Unknown RNET message type.");
                }
            }
            catch (RnetException e)
            {
                ExceptionDispatchInfo.Capture(e).Throw();
            }
            catch (Exception e)
            {
                throw new RnetProtocolException("Exception parsing RNET message.", e);
            }

            // unreachable
            return null;
        }

    }

}
