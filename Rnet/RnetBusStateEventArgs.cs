using System;

namespace Rnet
{

    /// <summary>
    /// Event arguments containing a <see cref="RnetBusState"/>.
    /// </summary>
    public class RnetBusStateEventArgs : EventArgs
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="state"></param>
        internal RnetBusStateEventArgs(RnetBusState state)
        {
            State = state;
        }

        public RnetBusState State { get; private set; }

    }

}
