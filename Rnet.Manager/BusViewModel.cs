using System.Collections.ObjectModel;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace Rnet.Manager
{

    /// <summary>
    /// Provides a view to interact with a bus.
    /// </summary>
    public class BusViewModel : NotificationObject
    {

        RnetBusObject selectedBusObject;
        BusObjectViewModel selectedBusObjectViewModel;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public BusViewModel()
        {
            Messages = new ObservableCollection<MessageViewModel>();

            //Bus = new RnetBus(new RnetTcpConnection("tokyo.cogito.cx", 9999));
            Bus = new RnetBus(new RnetTcpConnection(IPAddress.Parse("192.168.175.1"), 9999));
            Bus.MessageSent += (s, a) => Messages.Add(new MessageViewModel(a.Message, MessageDirection.Sent));
            Bus.MessageReceived += (s, a) => Messages.Add(new MessageViewModel(a.Message, MessageDirection.Received));
            Bus.Error += Bus_Error;
        }

        void Bus_Error(object sender, RnetClientErrorEventArgs args)
        {
            ExceptionDispatchInfo.Capture(args.Exception).Throw();
        }

        /// <summary>
        /// Bus being managed.
        /// </summary>
        public RnetBus Bus { get; private set; }

        /// <summary>
        /// Starts the bus.
        /// </summary>
        public ICommand StartCommand
        {
            get { return new DelegateCommand(async () => await Bus.StartAsync(), () => Bus.ClientState == RnetClientState.Stopped); }
        }

        /// <summary>
        /// Stops the bus.
        /// </summary>
        public ICommand StopCommand
        {
            get { return new DelegateCommand(async () => await Bus.StopAsync(), () => Bus.ClientState == RnetClientState.Started); }
        }

        /// <summary>
        /// Series of messages arriving on the bus.
        /// </summary>
        public ObservableCollection<MessageViewModel> Messages { get; private set; }

        /// <summary>
        /// Gets or sets the currently selected bus objects.
        /// </summary>
        public RnetBusObject SelectedBusObject
        {
            get { return selectedBusObject; }
            set { selectedBusObject = value; RaisePropertyChanged(() => SelectedBusObject); SelectedBusObjectViewModel = new BusObjectViewModel(value); }
        }

        /// <summary>
        /// Gets the view model for the selected view model.
        /// </summary>
        public BusObjectViewModel SelectedBusObjectViewModel
        {
            get { return selectedBusObjectViewModel; }
            set { selectedBusObjectViewModel = value; RaisePropertyChanged(() => SelectedBusObjectViewModel); }
        }

    }

}
