using System;

namespace Rnet
{

    public class RnetClientStateEventArgs : EventArgs
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="state"></param>
        internal RnetClientStateEventArgs(RnetClientState state)
        {
            State = state;
        }

        public RnetClientState State { get; private set; }

    }

}
