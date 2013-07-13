using System;

namespace Rnet
{

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
