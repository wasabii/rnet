using Microsoft.Practices.Prism.ViewModel;

namespace Rnet.Monitor.Wpf
{

    public class MessageViewModel : NotificationObject
    {

        public MessageViewModel(RnetMessage message, MessageDirection direction)
        {
            Message = message;
            Direction = direction;
            Initialize();
        }

        void Initialize()
        {

        }

        public RnetMessage Message { get; set; }

        public MessageDirection Direction { get; set; }

    }

}
