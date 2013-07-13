using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Rnet.Protocol;

namespace Rnet.Model
{

    /// <summary>
    /// Stores a set of <see cref="Item"/>s.
    /// </summary>
    public class DataItemCollection : IEnumerable<DataItem>
    {

        Dictionary<RnetPath, DataItem> items = new Dictionary<RnetPath, DataItem>();

        /// <summary>
        /// Gets the data item at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        DataItem GetData(RnetPath path)
        {
            DataItem item;
            return items.TryGetValue(path, out item) ? item : null;
        }

        /// <summary>
        /// Gets the data at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public DataItem this[RnetPath path]
        {
            get
            { return GetData(path); }
        }

        /// <summary>
        /// Begins a write of new data to the specified path.
        /// </summary>
        /// <param name="path"></param>
        public void WriteBegin(RnetPath path)
        {
            var item = GetData(path);
            if (item == null)
                item = items[path] = new DataItem(path);

            item.WriteBegin();
        }

        /// <summary>
        /// Appends the data to the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public void Write(RnetPath path, byte[] buffer)
        {
            var item = GetData(path);
            if (item == null)
                throw new NullReferenceException();

            item.Write(buffer);
        }

        /// <summary>
        /// Finalizes writing to the path and makes the data available.
        /// </summary>
        /// <param name="path"></param>
        public void WriteEnd(RnetPath path)
        {
            var item = GetData(path);
            if (item == null)
                throw new NullReferenceException();

            item.WriteEnd();
        }

        /// <summary>
        /// Removes the data item at the given path.
        /// </summary>
        /// <param name="path"></param>
        public void Remove(RnetPath path)
        {
            if (items.ContainsKey(path))
                items.Remove(path);
        }

        public IEnumerator<DataItem> GetEnumerator()
        {
            return items.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
