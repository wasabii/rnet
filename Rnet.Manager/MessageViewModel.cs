using Microsoft.Practices.Prism.ViewModel;

namespace Rnet.Manager
{

    /// <summary>
    /// View model for a message in the message log.
    /// </summary>
    public class MessageViewModel : NotificationObject
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="direction"></param>
        public MessageViewModel(RnetMessage message, MessageDirection direction)
        {
            Message = message;
            Direction = direction;
        }

        /// <summary>
        /// Message itself.
        /// </summary>
        public RnetMessage Message { get; private set; }

        /// <summary>
        /// Direction of the message.
        /// </summary>
        public MessageDirection Direction { get; private set; }

    }

}
