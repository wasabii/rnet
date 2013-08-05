using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

using OLinq;
using System.Reactive.Linq;

namespace Rnet.Manager
{

    /// <summary>
    /// Provides a view to interact with a bus.
    /// </summary>
    public class BusViewModel : NotificationObject
    {

        BusObjectViewModel selectedObject;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public BusViewModel()
        {
            //Bus = new RnetBus(new RnetTcpConnection("tokyo.cogito.cx", 9999));
            Bus = new RnetBus(new RnetTcpConnection(IPAddress.Parse("192.168.175.1"), 9999));
            Bus.MessageSent += (s, a) => Messages.Add(new MessageViewModel(a.Message, MessageDirection.Sent));
            Bus.MessageReceived += (s, a) => Messages.Add(new MessageViewModel(a.Message, MessageDirection.Received));
            Bus.Error += Bus_Error;

            // wrap controllers in view model
            Controllers = Bus.Controllers.AsObservableQuery()
                .Select(i => new ControllerViewModel(i))
                .AsObservableQuery()
                .ToObservableView();

            // wrap messages in view model
            Messages = new ObservableCollection<MessageViewModel>();
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
        /// Available bus objects.
        /// </summary>
        public IEnumerable<ControllerViewModel> Controllers { get; private set; }

        /// <summary>
        /// Series of messages arriving on the bus.
        /// </summary>
        public ObservableCollection<MessageViewModel> Messages { get; private set; }

        /// <summary>
        /// Gets or sets the currently selected bus object.
        /// </summary>
        public BusObjectViewModel SelectedObject
        {
            get { return selectedObject; }
            set { selectedObject = value; RaisePropertyChanged(() => SelectedObject); }
        }

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

    }

}
