using System;

namespace Rnet
{

    /// <summary>
    /// Represents errors that occur during RNET connection operations.
    /// </summary>
    public class RnetConnectionException : RnetException
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetConnectionException()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetConnectionException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetConnectionException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

    }

}
