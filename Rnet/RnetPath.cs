using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Rnet
{

    /// <summary>
    /// Represents an RNet path.
    /// </summary>
    [DebuggerDisplay("{DebugView}")]
    public struct RnetPath : IEnumerable<byte>, IComparable<RnetPath>, IComparable
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
        /// Gets an empty path instance.
        /// </summary>
        public static RnetPath Empty
        {
            get { return new RnetPath(); }
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
            if (a.Length < 0 || a.Length > 8)
                throw new ArgumentOutOfRangeException("path", "RnetPath only supports 8 nested levels.");
            for (int i = 0; i < a.Length; i++)
                for (int j = 0; j < a[i].Length; j++)
                    if (!char.IsDigit(a[i][j]))
                        throw new FormatException("RnetPath string is in an invalid format.");

            // generate new path instance
            var p = new RnetPath();
            p.length = (byte)a.Length;
            for (int i = 0; i < a.Length; i++)
                p.Set(i, byte.Parse(a[i]));

            return p;
        }

        byte a, b, c, d, e, f, g, h;
        byte length;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="items"></param>
        public RnetPath(IEnumerable<byte> items)
            : this()
        {
            foreach (var i in items)
            {
                if (length >= 8)
                    throw new ArgumentOutOfRangeException("items", "RnetPath can be a maximum of 8 levels.");

                // extend path and set value
                length++;
                Set(length - 1, i);
            }
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetPath(params byte[] items)
            : this((IEnumerable<byte>)items)
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="a"></param>
        public RnetPath(byte a)
            : this()
        {
            this.a = a;
            this.length = 1;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public RnetPath(byte a, byte b)
            : this(a)
        {
            this.b = b;
            this.length = 2;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public RnetPath(byte a, byte b, byte c)
            : this(a, b)
        {
            this.c = c;
            this.length = 3;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        public RnetPath(byte a, byte b, byte c, byte d)
            : this(a, b, c)
        {
            this.d = d;
            this.length = 4;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="e"></param>
        public RnetPath(byte a, byte b, byte c, byte d, byte e)
            : this(a, b, c, d)
        {
            this.e = e;
            this.length = 5;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <param name="f"></param>
        public RnetPath(byte a, byte b, byte c, byte d, byte e, byte f)
            : this(a, b, c, d, e)
        {
            this.f = f;
            this.length = 6;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <param name="f"></param>
        /// <param name="g"></param>
        public RnetPath(byte a, byte b, byte c, byte d, byte e, byte f, byte g)
            : this(a, b, c, e, d, f)
        {
            this.g = g;
            this.length = 7;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <param name="f"></param>
        /// <param name="g"></param>
        /// <param name="h"></param>
        public RnetPath(byte a, byte b, byte c, byte d, byte e, byte f, byte g, byte h)
            : this(a, b, c, d, e, f, g)
        {
            this.h = h;
            this.length = 8;
        }

        /// <summary>
        /// Gets the item contained in the path at the specified index.
        /// </summary>
        public byte this[int index]
        {
            get { return Get(index); }
            set { Set(index, value); }
        }

        /// <summary>
        /// Implements the default getter.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        byte Get(int index)
        {
            switch (index)
            {
                case 0:
                    return a;
                case 1:
                    return b;
                case 2:
                    return c;
                case 3:
                    return d;
                case 4:
                    return e;
                case 5:
                    return f;
                case 6:
                    return g;
                case 7:
                    return h;
                default:
                    throw new ArgumentOutOfRangeException("index", "The index must be between 0 and 7.");
            }
        }

        /// <summary>
        /// Implements the default setter.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        void Set(int index, byte value)
        {
            switch (index)
            {
                case 0:
                    a = value;
                    break;
                case 1:
                    b = value;
                    break;
                case 2:
                    c = value;
                    break;
                case 3:
                    d = value;
                    break;
                case 4:
                    e = value;
                    break;
                case 5:
                    f = value;
                    break;
                case 6:
                    g = value;
                    break;
                case 7:
                    h = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("index", "The index must be between 0 and 7.");
            }
        }

        /// <summary>
        /// Gets the number of items in the path.
        /// </summary>
        public int Length
        {
            get { return length; }
        }

        /// <summary>
        /// Writes the path to the writer.
        /// </summary>
        /// <param name="writer"></param>
        internal void Write(RnetStreamWriter writer)
        {
            writer.WriteByte((byte)length);
            foreach (var item in this)
                writer.WriteByte(item);
        }

        /// <summary>
        /// Reads a path from the reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal static RnetPath Read(RnetMessageBodyReader reader)
        {
            // generate new instance and configure it
            var path = new RnetPath();
            path.length = reader.ReadByte();
            for (int i = 0; i < path.length; i++)
                path[i] = reader.ReadByte();
            return path;
        }

        /// <summary>
        /// Navigates to the specified child item.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public RnetPath Navigate(byte index)
        {
            if (length >= 8)
                throw new IndexOutOfRangeException("An RnetPath cannot be more than 8 levels deep.");

            var path = this;
            path.length++;
            path[path.Length - 1] = index;
            return path;
        }

        /// <summary>
        /// Navigates to the parent item.
        /// </summary>
        /// <returns></returns>
        public RnetPath GetParent()
        {
            if (length == 0)
                throw new IndexOutOfRangeException("Path is already at the top level.");

            var path = this;
            path.length--;
            return path;
        }

        public IEnumerator<byte> GetEnumerator()
        {
            for (int i = 0; i < length; i++)
                yield return Get(i);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var path = (RnetPath)obj;
            if (length != path.length)
                return false;
            for (int i = 0; i < length; i++)
                if (Get(i) != path.Get(i))
                    return false;

            return true;
        }

        /// <summary>
        /// Returns the hashcode for this instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int h = length.GetHashCode();
            for (int i = 0; i < length; i++)
                h ^= Get(i).GetHashCode();
            return h;
        }

        public string DebugView
        {
            get { return string.Join(".", (IEnumerable<byte>)this); }
        }

        public override string ToString()
        {
            return DebugView;
        }

        /// <summary>
        /// Gets the path as an array of <see cref="Byte"/>s.
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            var a = new byte[length];
            for (int i = 0; i < length; i++)
                a[i] = Get(i);
            return a;
        }

        /// <summary>
        /// Compares this <see cref="RnetPath"/> to another <see cref="RnetPath"/>.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        int IComparable<RnetPath>.CompareTo(RnetPath other)
        {
            for (int i = 0; i < length; i++)
                if (Get(i).CompareTo(other.Get(i)) != 0)
                    return Get(i).CompareTo(other.Get(i));

            return 0;
        }

        /// <summary>
        /// Compares the current object with another object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        int IComparable.CompareTo(object obj)
        {
            return ((IComparable<RnetPath>)this).CompareTo((RnetPath)obj);
        }

    }

}
