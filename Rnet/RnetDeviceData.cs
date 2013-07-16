using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    public class RnetDeviceData : RnetModelObject
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
        /// Initializes a new instance.
        /// </summary>
        /// <param name="path"></param>
        internal RnetDeviceData(RnetDevice device, RnetPath path)
        {
            Device = device;
            Path = path;
        }

        /// <summary>
        /// Device that holds this data.
        /// </summary>
        public RnetDevice Device { get; private set; }

        /// <summary>
        /// Path to the data item.
        /// </summary>
        public RnetPath Path { get; private set; }

        /// <summary>
        /// Clears any active input buffer.
        /// </summary>
        internal void WriteBegin(int packetCount)
        {
            this.stream = new MemoryStream();
            this.packetCount = packetCount;
            this.packetNumber = -1;
        }

        /// <summary>
        /// Writes the data.
        /// </summary>
        /// <param name="data"></param>
        internal void Write(byte[] data, int packetNumber)
        {
            stream.Write(data, 0, data.Length);
            this.packetNumber = packetNumber;
        }

        /// <summary>
        /// Closes the stream and sets the new data as the current data.
        /// </summary>
        internal void WriteEnd()
        {
            Timestamp = DateTime.UtcNow;
            Buffer = stream.ToArray();
            stream = null;
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
            internal set { timestamp = value; RaisePropertyChanged("Timestamp"); RaisePropertyChanged("Age"); RaisePropertyChanged("Valid"); }
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
        /// Sets the value of the data in the device.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task SetBufferAsync(byte[] value)
        {
            return SetBufferAsync(value, RnetBus.CreateDefaultCancellationToken());
        }

        /// <summary>
        /// Sets the value of the data in the device.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task SetBufferAsync(byte[] value, CancellationToken cancellationToken)
        {
            Buffer = value;
            await Device.Data.SetAsync(this, cancellationToken);
        }

    }

}
