using System.Diagnostics;

namespace Rnet
{

    /// <summary>
    /// Represents an RNet path.
    /// </summary>
    [DebuggerDisplay("{DebugView}")]
    public class RnetPath
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetPath(byte directory)
        {
            Level = 1;
            Directory = directory;
        }

        /// <summary>
        /// Initializes a new instance with a previous item.
        /// </summary>
        /// <param name="previous"></param>
        public RnetPath(RnetPath previous, byte directory)
        {
            Level = (byte)(previous.Level + 1);
            Directory = directory;
            Previous = previous;
        }

        /// <summary>
        /// Gets the level of the item.
        /// </summary>
        internal byte Level { get; private set; }

        /// <summary>
        /// Gets the previous item, if this item is not the root.
        /// </summary>
        internal RnetPath Previous { get; private set; }

        /// <summary>
        /// Gets the directory of this item.
        /// </summary>
        internal byte Directory { get; private set; }

        /// <summary>
        /// Creates a new child path item.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        internal RnetPath Next(byte directory)
        {
            return new RnetPath(this, directory);
        }

        /// <summary>
        /// Generates a debug string for display.
        /// </summary>
        string DebugView
        {
            get
            {
                if (Previous == null)
                    return Directory.ToString();
                else
                    return Previous.DebugView + "." + Directory.ToString();
            }
        }

    }

}
