using System;

namespace Rnet.Protocol
{

    public class RnetConnectionStateEventArgs : EventArgs
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="state"></param>
        internal RnetConnectionStateEventArgs(RnetConnectionState state)
        {
            State = state;
        }

        public RnetConnectionState State { get; private set; }

    }

}
