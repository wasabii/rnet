using System;

namespace Rnet.Protocol
{

    public class RnetProtocolException : RnetException
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetProtocolException()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetProtocolException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetProtocolException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

    }

}
