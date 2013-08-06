using System;

namespace Rnet
{

    /// <summary>
    /// Event arguments containing an <see cref="Exception"/>.
    /// </summary>
    public class RnetExceptionEventArgs : EventArgs
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="exception"></param>
        internal RnetExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }

        /// <summary>
        /// Exception that occurred.
        /// </summary>
        public Exception Exception { get; private set; }

    }

}
