using System.IO;

namespace Rnet
{

    /// <summary>
    /// Provides methods by which to read messages from an RNet <see cref="Stream"/>.
    /// </summary>
    public class RnetMessageReader
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="stream"></param>
        public RnetMessageReader(Stream stream)
        {
            Stream = stream;
        }

        /// <summary>
        /// Gets a reference to the source <see cref="Stream"/>
        /// </summary>
        public Stream Stream { get; private set; }

    }

}
