using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

using Rnet.Util;

namespace Rnet
{

    /// <summary>
    /// Reference to a RNET device present either implememented locally or remotely.
    /// </summary>
    public abstract class RnetDevice : RnetBusObject
    {

        readonly Dictionary<RnetPath, RnetDataHandle> handles =
            new Dictionary<RnetPath, RnetDataHandle>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        internal protected RnetDevice(RnetBus bus)
            : base(bus)
        {

        }

        /// <summary>
        /// Gets the ID of the device on the RNET bus.
        /// </summary>
        public abstract RnetDeviceId DeviceId { get; }

        /// <summary>
        /// Gets a data handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public RnetDataHandle this[RnetPath path]
        {
            get { return GetOrCreateDataHandle(path); }
        }

        /// <summary>
        /// Gets a data handle to the given path.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public RnetDataHandle this[byte a]
        {
            get { return GetOrCreateDataHandle(new RnetPath(a)); }
        }

        /// <summary>
        /// Gets a data handle to the given path.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public RnetDataHandle this[byte a, byte b]
        {
            get { return GetOrCreateDataHandle(new RnetPath(a, b)); }
        }

        /// <summary>
        /// Gets a data handle to the given path.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public RnetDataHandle this[byte a, byte b, byte c]
        {
            get { return GetOrCreateDataHandle(new RnetPath(a, b, c)); }
        }

        /// <summary>
        /// Gets a data handle to the given path.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public RnetDataHandle this[byte a, byte b, byte c, byte d]
        {
            get { return GetOrCreateDataHandle(new RnetPath(a, b, c, d)); }
        }

        /// <summary>
        /// Gets a data handle to the given path.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public RnetDataHandle this[byte a, byte b, byte c, byte d, byte e]
        {
            get { return GetOrCreateDataHandle(new RnetPath(a, b, c, d, e)); }
        }

        /// <summary>
        /// Gets a data handle to the given path.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public RnetDataHandle this[byte a, byte b, byte c, byte d, byte e, byte f]
        {
            get { return GetOrCreateDataHandle(new RnetPath(a, b, c, d, e, f)); }
        }

        /// <summary>
        /// Gets a data handle to the given path.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <param name="f"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        public RnetDataHandle this[byte a, byte b, byte c, byte d, byte e, byte f, byte g]
        {
            get { return GetOrCreateDataHandle(new RnetPath(a, b, c, d, e, f, g)); }
        }

        /// <summary>
        /// Gets a data handle to the given path.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <param name="f"></param>
        /// <param name="g"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public RnetDataHandle this[byte a, byte b, byte c, byte d, byte e, byte f, byte g, byte h]
        {
            get { return GetOrCreateDataHandle(new RnetPath(a, b, c, d, e, f, g, h)); }
        }

        /// <summary>
        /// Gets all the known data handles.
        /// </summary>
        public virtual IEnumerable<RnetDataHandle> Data
        {
            get { return handles.Values.Where(i => i != null).ToList(); }
        }

        /// <summary>
        /// Gets or creates a data handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected internal RnetDataHandle GetOrCreateDataHandle(RnetPath path)
        {
            Contract.Requires<ArgumentException>(path.Length > 0);
            Contract.Ensures(Contract.Result<RnetDataHandle>() != null);

            lock (handles)
                return handles
                    .GetOrCreate(path, i => CreateDataHandle(i));
        }

        /// <summary>
        /// Creates a new data handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected internal abstract RnetDataHandle CreateDataHandle(RnetPath path);

    }

}
