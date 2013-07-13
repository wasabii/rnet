namespace Rnet.Protocol
{

    public class RnetMessageEventArgs
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="message"></param>
        internal RnetMessageEventArgs(RnetMessage message)
        {
            Message = message;
        }

        /// <summary>
        /// Message that caused the event.
        /// </summary>
        public RnetMessage Message { get; private set; }

    }

}
