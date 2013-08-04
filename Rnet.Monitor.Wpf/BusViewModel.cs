using System.Collections.ObjectModel;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace Rnet.Monitor.Wpf
{

    public class BusViewModel : NotificationObject
    {

        RnetBusObject selectedBusObject;
        BusObjectViewModel selectedBusObjectViewModel;

        public BusViewModel()
        {
            Messages = new ObservableCollection<MessageViewModel>();

            //Bus = new RnetBus(new RnetTcpConnection("tokyo.cogito.cx", 9999));
            Bus = new RnetBus(new RnetTcpConnection(IPAddress.Parse("192.168.175.1"), 9999));
            Bus.ConnectionStateChanged += Bus_ConnectionStateChanged;
            Bus.MessageSent += (s, a) => Messages.Add(new MessageViewModel(a.Message, MessageDirection.Sent));
            Bus.MessageReceived += (s, a) => Messages.Add(new MessageViewModel(a.Message, MessageDirection.Received));
            Bus.Error += Bus_Error;
        }

        void Bus_ConnectionStateChanged(object sender, RnetConnectionStateEventArgs args)
        {

        }

        void Bus_Error(object sender, RnetClientErrorEventArgs args)
        {
            ExceptionDispatchInfo.Capture(args.Exception).Throw();
        }

        public RnetBus Bus { get; private set; }

        public ObservableCollection<MessageViewModel> Messages { get; private set; }

        public ICommand StartCommand
        {
            get { return new DelegateCommand(Start, CanStart); }
        }

        bool CanStart()
        {
            return Bus.ClientState == RnetClientState.Stopped;
        }

        async void Start()
        {
            await Bus.StartAsync();
        }

        public ICommand StopCommand
        {
            get { return new DelegateCommand(Stop, CanStop); }
        }

        bool CanStop()
        {
            return Bus.ClientState == RnetClientState.Started;
        }

        async void Stop()
        {
            await Bus.StopAsync();
        }

        public RnetBusObject SelectedBusObject
        {
            get { return selectedBusObject; }
            set { selectedBusObject = value; RaisePropertyChanged(() => SelectedBusObject); SelectedBusObjectViewModel = new BusObjectViewModel(value); }
        }

        public BusObjectViewModel SelectedBusObjectViewModel
        {
            get { return selectedBusObjectViewModel; }
            set { selectedBusObjectViewModel = value; RaisePropertyChanged(() => SelectedBusObjectViewModel); }
        }

    }

}
