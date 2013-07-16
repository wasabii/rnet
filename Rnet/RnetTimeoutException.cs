using System;

namespace Rnet
{

    /// <summary>
    /// Raised when an operation timeout occurs.
    /// </summary>
    public class RnetTimeoutException : Exception
    {

        internal RnetTimeoutException(string message)
            : base(message)
        {

        }

    }

}
