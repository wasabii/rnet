using System;
using System.Diagnostics.Contracts;

namespace Rnet
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
