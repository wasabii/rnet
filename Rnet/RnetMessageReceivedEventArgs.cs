using System;

namespace Rnet
{

    /// <summary>
    /// Arguments passed to the handler of the RnetMessageReceived event.
    /// </summary>
    public class RnetMessageReceivedEventArgs : EventArgs
    {

        /// <summary>
        /// Initalizes a new instance.
        /// </summary>
        /// <param name="message"></param>
        internal RnetMessageReceivedEventArgs(RnetMessage message)
        {
            Message = message;
        }

        /// <summary>
        /// Gets the message that was received.
        /// </summary>
        public RnetMessage Message { get; private set; }

    }

}
