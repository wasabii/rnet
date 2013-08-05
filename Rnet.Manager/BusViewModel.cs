using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using OLinq;

namespace Rnet.Manager
{

    /// <summary>
    /// Provides a view to interact with a bus.
    /// </summary>
    public class BusViewModel : NotificationObject
    {

        Uri uri;
        RnetBus bus;
        IEnumerable<ControllerViewModel> controllers;
        ObservableCollection<MessageViewModel> messages;
        BusObjectViewModel selectedObject;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public BusViewModel()
        {
            Uri = new Uri("rnet.tcp://tokyo.cogito.cx:9999");
        }

        /// <summary>
        /// Endpoint to connect bus to.
        /// </summary>
        public Uri Uri
        {
            get { return uri; }
            set { uri = value; RaisePropertyChanged(() => Uri); }
        }

        /// <summary>
        /// Bus being managed.
        /// </summary>
        public RnetBus Bus
        {
            get { return bus; }
            set { bus = value; RaisePropertyChanged(() => Bus); }
        }

        /// <summary>
        /// Available controllers.
        /// </summary>
        public IEnumerable<ControllerViewModel> Controllers
        {
            get { return controllers; }
            set { controllers = value; RaisePropertyChanged(() => Controllers); }
        }

        /// <summary>
        /// Series of messages arriving on the bus.
        /// </summary>
        public ObservableCollection<MessageViewModel> Messages
        {
            get { return messages; }
            set { messages = value; RaisePropertyChanged(() => Messages); }
        }

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
            get { return new DelegateCommand(Start, () => Bus == null); }
        }

        async void Start()
        {
            Bus = new RnetBus(uri);
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

            // start the bus
            await Bus.StartAsync();
        }

        /// <summary>
        /// Stops the bus.
        /// </summary>
        public ICommand StopCommand
        {
            get { return new DelegateCommand(Stop, () => Bus != null); }
        }

        async void Stop()
        {
            await Bus.StopAsync();
            Bus = null;
        }

        void Bus_Error(object sender, RnetClientErrorEventArgs args)
        {
            ExceptionDispatchInfo.Capture(args.Exception).Throw();
        }

    }

}
