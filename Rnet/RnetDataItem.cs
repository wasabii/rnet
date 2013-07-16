using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    public class RnetDataItem : RnetModelObject, IDisposable
    {

        /// <summary>
        /// Default lifetime for requested data before it expires.
        /// </summary>
        static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(15);

        MemoryStream stream;
        int packetCount;
        int packetNumber;
        byte[] buffer;
        DateTime timestamp;

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
        public void WriteBegin(int packetCount)
        {
            this.stream = new MemoryStream();
            this.packetCount = packetCount;
            this.packetNumber = -1;
        }

        /// <summary>
        /// Writes the data.
        /// </summary>
        /// <param name="data"></param>
        public void Write(byte[] data, int packetNumber)
        {
            stream.Write(data, 0, data.Length);
            this.packetNumber = packetNumber;
        }

        /// <summary>
        /// Closes the stream and sets the new data as the current data.
        /// </summary>
        public void WriteEnd()
        {
            Timestamp = DateTime.UtcNow;
            Buffer = stream.ToArray();
            stream = null;
            RaiseBufferAvailable();

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
        public byte[] Buffer
        {
            get { return buffer; }
            private set { buffer = value; RaisePropertyChanged("Buffer"); RaisePropertyChanged("Valid"); }
        }

        /// <summary>
        /// Gets the timestamp the data was entered.
        /// </summary>
        public DateTime Timestamp
        {
            get { return timestamp; }
            private set { timestamp = value; RaisePropertyChanged("Timestamp"); RaisePropertyChanged("Age"); RaisePropertyChanged("Valid"); }
        }

        /// <summary>
        /// Age of the data.
        /// </summary>
        public TimeSpan Age
        {
            get { return DateTime.UtcNow - Timestamp; }
        }

        /// <summary>
        /// Gets whether the data is valid.
        /// </summary>
        public bool Valid
        {
            get { return Buffer != null && Age < Lifetime; }
        }

        /// <summary>
        /// Raised when the data buffer is finished.
        /// </summary>
        public EventHandler<EventArgs> BufferAvailable;

        /// <summary>
        /// Raises the BufferAvailable event.
        /// </summary>
        void RaiseBufferAvailable()
        {
            if (BufferAvailable != null)
                BufferAvailable(this, new EventArgs());
        }

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
                if (Valid)
                    return Task.FromResult(Buffer);

                // subscribe to data
                var tcs = new TaskCompletionSource<byte[]>();
                cancellationToken.Register(() => tcs.SetCanceled());
                subscribers.Add(tcs);
                return tcs.Task;
            }
        }

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        public void Dispose()
        {
            // cancel all subscribers
            foreach (var subscriber in subscribers)
                if (!subscriber.Task.IsCompleted)
                    subscriber.SetCanceled();
        }
    }

}
