using System;

namespace Rnet
{

    public class RnetClientErrorEventArgs : EventArgs
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="exception"></param>
        internal RnetClientErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }

        /// <summary>
        /// Error that occurred.
        /// </summary>
        public Exception Exception { get; private set; }

    }

}
