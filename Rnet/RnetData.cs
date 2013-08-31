using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace Rnet
{

    public struct RnetData : IEnumerable<byte>
    {

        byte[] data;

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(data != null);
        }


        /// <summary>
        /// Reads a <see cref="RnetData"/> structure from the given message body.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal static RnetData Read(RnetMessageBodyReader reader)
        {
            Contract.Requires(reader != null);

            var l = reader.ReadUInt16();
            var d = new byte[l];
            for (int i = 0; i < l; i++)
                d[i] = reader.ReadByte();

            return new RnetData(d);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="data"></param>
        internal RnetData(byte[] data)
            : this()
        {
            Contract.Requires(data != null);

            this.data = data;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="length"></param>
        internal RnetData(ushort length)
            : this()
        {
            Contract.Requires(length >= 0);

            this.data = new byte[length];
        }

        /// <summary>
        /// Gets or sets the byte of data at the given index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte this[int index]
        {
            get { Contract.Requires(index >= 0); return data[index]; }
            set { Contract.Requires(index >= 0); data[index] = value; }
        }

        /// <summary>
        /// Gets the length of the data.
        /// </summary>
        public ushort Length
        {
            get { return (ushort)data.Length; }
        }

        public IEnumerator<byte> GetEnumerator()
        {
            return (IEnumerator<byte>)data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void WriteDebugView(TextWriter writer)
        {
            Contract.Requires(writer != null);
            Contract.Assert(data != null);

            writer.Write("0x");
            for (int i = 0; i < data.Length; i++)
                writer.Write("{0:x2}", data[i]);

            if (data.Length == 1)
                writer.Write(" /* {0} */", data[0]);
            else if (data.Length == 2)
                writer.Write(" /* {0} */", data[0] << 8 + data[1]);
            else if (GetDebugText() != null)
                writer.Write(" /* \"{0}\" */", GetDebugText());
        }

        /// <summary>
        /// Attempts to extract an RNET text stream from the data.
        /// </summary>
        /// <returns></returns>
        string GetText()
        {
            Contract.Requires(data != null);

            try
            {
                if (data.Length < 4)
                    return null;

                // alignment
                var a = (RnetTextAlignment)data[0];
                if (a != RnetTextAlignment.Left && a != RnetTextAlignment.Centered)
                    return null;

                // flash time
                var f = data[1] << 8 + data[2];

                // attempt to decode rest of string
                var c = new char[128];
                var d = Encoding.ASCII.GetDecoder();
                d.Fallback = new DecoderExceptionFallback();
                var l = d.GetChars(data, 3, data.Length - 3, c, 0);

                // validate for allowed characters
                for (int i = 0; i < l; i++)
                    if (c[i] < 32 || c[i] > 126)
                        return null;

                return new string(c, 0, l);
            }
            catch (DecoderFallbackException)
            {
                // ignore
            }

            return null;
        }

        /// <summary>
        /// Attempts to extract an RNET text stream from the data.
        /// </summary>
        /// <returns></returns>
        string GetDebugText()
        {
            try
            {
                if (data.Length < 4)
                    return null;

                return Encoding.ASCII.GetString(data);
            }
            catch (DecoderFallbackException)
            {
                // ignore
            }

            return null;
        }

        public string DebugView
        {
            get
            {
                var b = new StringWriter();
                WriteDebugView(b);
                return b.ToString();
            }
        }

        public override string ToString()
        {
            return DebugView;
        }

        /// <summary>
        /// Gets the data as a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return data;
        }

    }

}
