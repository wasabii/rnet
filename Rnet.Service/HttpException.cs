using System;

using Nancy;

namespace Rnet.Service
{

    class HttpException : Exception
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="statusCode"></param>
        public HttpException(HttpStatusCode statusCode)
            : base()
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        public HttpException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the <see cref="HttpStatusCode"/> representing the error type.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

    }

}
