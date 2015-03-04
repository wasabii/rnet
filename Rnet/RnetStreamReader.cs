using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Provides methods by which to read messages from an RNet <see cref="Stream"/>.
    /// </summary>
    public class RnetStreamReader
    {

        Stream source;
        byte[] buffer;
        int l;
        int p;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="source"></param>
        public RnetStreamReader(Stream source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            this.source = source;
            this.buffer = new byte[512];
            this.p = 0;
        }

        /// <summary>
        /// Reads a single byte, or returns the exception that occurred while attempting to.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        async Task<byte> ReadByteAsync(CancellationToken cancellationToken)
        {
            // position is at end of buffer, reset
            if (p >= l)
            {
                p = 0;
                l = 0;
            }

            // position is currently unallocated
            if (p == 0)
            {
                try
                {
                    // read new data into buffer and obtain length
                    l = await source.ReadAsync(buffer, 0, 512, cancellationToken);
                    if (l == -1)
                        throw new EndOfStreamException();
                }
                catch (Exception e)
                {
                    throw new RnetException("Unable to read from underlying stream.", e);
                }
            }

            return buffer[p++];
        }

        /// <summary>
        /// Reads the next message.
        /// </summary>
        /// <returns></returns>
        public virtual RnetMessage Read()
        {
            return ReadAsync(CancellationToken.None).Result;
        }

        /// <summary>
        /// Reads the next message.
        /// </summary>
        /// <returns></returns>
        public virtual Task<RnetMessage> ReadAsync()
        {
            return ReadAsync(CancellationToken.None);
        }

        /// <summary>
        /// Reads the next message.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<RnetMessage> ReadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // wait for message start
            while (await ReadByteAsync(cancellationToken) != (byte)RnetSpecialMessageChars.MessageStart)
                continue;

            // begin new message
            var buffer = new byte[1024];
            var length = 0;
            buffer[length++] = (byte)RnetSpecialMessageChars.MessageStart;

            // advance until message end character
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // obtain next byte
                var b = await ReadByteAsync(cancellationToken);
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
            Contract.Requires<ArgumentNullException>(reader != null);

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
                    case RnetMessageType.Number6:
                        return RnetNumberSixMessage.Read(reader, targetDeviceId, sourceDeviceId);
                    default:
                        return RnetUnknownMessage.Read(reader, targetDeviceId, sourceDeviceId, messageType);
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
