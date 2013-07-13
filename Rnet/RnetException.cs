using System;

namespace Rnet
{

    /// <summary>
    /// Represents errors that occur during RNet operations.
    /// </summary>
    public class RnetException : Exception
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetException()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

    }

}
