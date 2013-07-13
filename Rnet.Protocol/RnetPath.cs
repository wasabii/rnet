﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rnet.Protocol
{

    /// <summary>
    /// Represents an RNet path.
    /// </summary>
    [DebuggerDisplay("{DebugView}")]
    public class RnetPath : IEnumerable<byte>
    {

        byte[] items;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetPath(params byte[] items)
        {
            this.items = items;
        }

        /// <summary>
        /// Gets the items contained in the path.
        /// </summary>
        public byte this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }

        /// <summary>
        /// Gets the number of items in the path.
        /// </summary>
        public int Length
        {
            get { return items.Length; }
        }

        /// <summary>
        /// Writes the path to the writer.
        /// </summary>
        /// <param name="writer"></param>
        internal void Write(RnetStreamWriter writer)
        {
            writer.WriteByte((byte)items.Length);
            foreach (var item in items)
                writer.WriteByte(item);
        }

        /// <summary>
        /// Reads a path from the reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal static RnetPath Read(RnetMessageBodyReader reader)
        {
            var len = reader.ReadByte();
            var buf = new byte[len];
            for (int i = 0; i < len; i++)
                buf[i] = reader.ReadByte();

            return new RnetPath(buf);
        }

        public IEnumerator<byte> GetEnumerator()
        {
            return ((IEnumerable<byte>)items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string DebugView
        {
            get { return string.Join(".", this); }
        }

        public override string ToString()
        {
            return DebugView;
        }

    }

}