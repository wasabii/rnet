namespace Rnet
{

    /// <summary>
    /// Raised by an <see cref="RnetConnection"/> object when data is available.
    /// </summary>
    public class RnetDataReceivedEventArgs
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="data"></param>
        internal RnetDataReceivedEventArgs(byte[] data)
        {
            Data = data;
        }

        /// <summary>
        /// Gets the data received from the connection.
        /// </summary>
        public byte[] Data { get; private set; }

    }

}
