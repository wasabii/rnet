using System;
using System.ComponentModel;
using System.Xml.Linq;

using Mono.Upnp;
using Mono.Upnp.Control;

namespace Rnet.Upnp
{

    public class Bus : INotifyPropertyChanged
    {

        public static readonly ServiceType ServiceType = new ServiceType("tempuri.org", "RNetBus", new Version(1, 0));

        int currentMessageId;

        /// <summary>
        /// Sequence number of the previous message.
        /// </summary>
        [UpnpStateVariable]
        public int CurrentMessageId
        {
            get { return currentMessageId; }
            set { currentMessageId = value; }
        }

        public void Send(
            [UpnpArgument("MessageId")] out int messageId,
            [UpnpArgument("Message")] string message)
        {
            messageId = 0;
        }

        public void Receive(
            [UpnpArgument("MessageId")] int messageId,
            [UpnpArgument("Message")] out string message)
        {
            message = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(Schemas.RnetMessage + "message",
                    new XAttribute("id", messageId),
                    null))
                .ToString(SaveOptions.DisableFormatting);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
