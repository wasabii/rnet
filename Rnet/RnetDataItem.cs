using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    public class RnetDataItem
    {

        MemoryStream stream;

        /// <summary>
        /// Subscribers waiting for data.
        /// </summary>
        List<TaskCompletionSource<byte[]>> subscribers =
            new List<TaskCompletionSource<byte[]>>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="path"></param>
        internal RnetDataItem(RnetPath path)
        {
            Path = path;
        }

        /// <summary>
        /// Path to the data item.
        /// </summary>
        public RnetPath Path { get; private set; }

        /// <summary>
        /// Clears any active input buffer.
        /// </summary>
        public void WriteBegin()
        {
            stream = new MemoryStream();
        }

        /// <summary>
        /// Writes the data.
        /// </summary>
        /// <param name="data"></param>
        public void Write(byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Closes the stream and sets the new data as the current data.
        /// </summary>
        public void WriteEnd()
        {
            Timestamp = DateTime.UtcNow;
            Buffer = stream.ToArray();
            stream = null;

            // notify subscribers
            lock (subscribers)
            {
                foreach (var subscriber in subscribers)
                    subscriber.SetResult(Buffer);

                // remove completed subscriptions
                foreach (var subscriber in subscribers.ToList())
                    if (subscriber.Task.IsCompleted)
                        subscribers.Remove(subscriber);
            }
        }

        /// <summary>
        /// Gets the active data.
        /// </summary>
        public byte[] Buffer { get; private set; }

        /// <summary>
        /// Gets the timestamp the data was entered.
        /// </summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> GetBufferAsync()
        {
            return GetBufferAsync(CancellationToken.None);
        }

        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> GetBufferAsync(CancellationToken cancellationToken)
        {
            lock (subscribers)
            {
                if (Buffer != null)
                    return Task.FromResult(Buffer);

                // subscribe to data
                var tcs = new TaskCompletionSource<byte[]>();
                cancellationToken.Register(() => tcs.SetCanceled());
                subscribers.Add(tcs);
                return tcs.Task;
            }
        }

    }

}
