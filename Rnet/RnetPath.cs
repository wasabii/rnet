using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rnet
{

    /// <summary>
    /// Represents an RNet path.
    /// </summary>
    [DebuggerDisplay("{DebugView}")]
    public class RnetPath : IEnumerable<byte>
    {

        /// <summary>
        /// Returns <c>true</c> if the two paths are equal.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(RnetPath x, RnetPath y)
        {
            return object.Equals(x, y);
        }

        /// <summary>
        /// Returns <c>true</c> if the two paths are not equal.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(RnetPath x, RnetPath y)
        {
            return !object.Equals(x, y);
        }

        /// <summary>
        /// Implicitly converts a <see cref="String"/> to a <see cref="RnetPath"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static implicit operator RnetPath(string path)
        {
            return RnetPath.Parse(path);
        }

        /// <summary>
        /// Implicitly converts a <see cref="String"/> to a <see cref="RnetPath"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static implicit operator string(RnetPath path)
        {
            return path.ToString();
        }

        /// <summary>
        /// Parses the given string into an <see cref="RnetPath"/>.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static RnetPath Parse(string path)
        {
            // split path string and validate for only digits
            var a = path.Split('.');
            for (int i = 0; i < a.Length; i++)
                for (int j = 0; j < a[i].Length; j++)
                    if (!char.IsDigit(a[i][j]))
                        throw new FormatException("RnetPath string is in an invalid format.");

            // convert each element into byte
            var b = new byte[a.Length];
            for (int i = 0; i < a.Length; i++)
                b[i] = byte.Parse(a[i]);

            // generate path
            return new RnetPath(b);
        }

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

        /// <summary>
        /// Navigates to the specified child item.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public RnetPath Navigate(byte index)
        {
            // append index onto the end of the array
            var a = new byte[items.Length + 1];
            Array.Copy(items, a, items.Length);
            a[items.Length] = index;

            // generate new path
            return new RnetPath(a);
        }

        /// <summary>
        /// Navigates to the parent item.
        /// </summary>
        /// <returns></returns>
        public RnetPath GetParent()
        {
            if (items.Length == 1)
                throw new InvalidOperationException("Path is already at the top level.");

            // strip last item off of path
            var a = new byte[items.Length - 1];
            Array.Copy(items, a, a.Length);

            // generate new path
            return new RnetPath(a);
        }

        public IEnumerator<byte> GetEnumerator()
        {
            return ((IEnumerable<byte>)items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            return obj != null && ((IStructuralEquatable)items).Equals(((RnetPath)obj).items, StructuralComparisons.StructuralEqualityComparer);
        }

        public override int GetHashCode()
        {
            return ((IStructuralEquatable)items).GetHashCode(StructuralComparisons.StructuralEqualityComparer);
        }

        public string DebugView
        {
            get { return string.Join(".", items); }
        }

        public override string ToString()
        {
            return DebugView;
        }

    }

}
