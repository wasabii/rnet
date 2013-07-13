using System;
using System.IO;

namespace Rnet.Protocol
{

    /// <summary>
    /// Provides read access to an RNET message body.
    /// </summary>
    public class RnetMessageBodyReader
    {

        /// <summary>
        /// Underlying body stream.
        /// </summary>
        Stream body;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="body"></param>
        public RnetMessageBodyReader(Stream body)
        {
            if (body == null)
                throw new ArgumentNullException("body");

            this.body = body;
        }

        /// <summary>
        /// Reads a single byte from the current message body without first decoding it.
        /// </summary>
        /// <returns></returns>
        byte ReadRaw()
        {
            try
            {
                // read next byte
                var b = body.ReadByte();
                if (b == -1)
                    throw new EndOfStreamException();

                return (byte)b;
            }
            catch (NullReferenceException)
            {
                throw new RnetException();
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new RnetException();
            }
        }

        /// <summary>
        /// Reads a single byte from the RNet message body.
        /// </summary>
        /// <returns></returns>
        public byte ReadByte()
        {
            var b = ReadRaw();
            if (b == (byte)RnetSpecialMessageChars.Invert)
                // invert instruction encountered, flip next byte
                return (byte)(ReadRaw() ^ 0xff);
            else
                return b;
        }

        /// <summary>
        /// Reads a <see cref="UInt16"/> value from the RNet message body.
        /// </summary>
        /// <returns></returns>
        public ushort ReadUInt16()
        {
            var lo = (int)ReadByte();
            var hi = (int)ReadByte();
            return (ushort)((hi << 8) | lo);
        }

    }

}
