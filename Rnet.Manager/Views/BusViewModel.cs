using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.ExceptionServices;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using OLinq;
using Rnet.Drivers;

namespace Rnet.Manager.Views
{

    /// <summary>
    /// Provides a view to interact with a bus.
    /// </summary>
    public class BusViewModel : NotificationObject
    {

        readonly Uri uri;
        RnetBus bus;
        IEnumerable<BusObjectViewModel> objects;
        ObservableCollection<MessageViewModel> messages;
        BusObjectViewModel selectedObject;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        [ImportingConstructor]
        public BusViewModel(
            ProfileManager profileManager)
        {
            this.uri = new Uri("rnet.tcp://tokyo.cogito.cx:9999");

            StartCommand = new DelegateCommand(Start, () => Bus == null);
            StopCommand = new DelegateCommand(Stop, () => Bus != null);
            ScanCommand = new DelegateCommand(Scan, () => Bus != null);
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
        public IEnumerable<BusObjectViewModel> Objects
        {
            get { return objects; }
            set { objects = value; RaisePropertyChanged(() => Objects); }
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
        public DelegateCommand StartCommand { get; private set; }

        async void Start()
        {
            Bus = new RnetBus(uri);
            Bus.MessageSent += (s, a) => Messages.Add(new MessageViewModel(a.Message, MessageDirection.Sent));
            Bus.MessageReceived += (s, a) => Messages.Add(new MessageViewModel(a.Message, MessageDirection.Received));
            Bus.UnhandledException += Bus_Error;

            // wrap controllers in view model
            Objects = Bus.Controllers.AsObservableQuery()
                .Select(i => new BusObjectViewModel(i))
                .AsObservableQuery()
                .ToObservableView();

            // wrap messages in view model
            Messages = new ObservableCollection<MessageViewModel>();

            // we are started now
            StartCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
            ScanCommand.RaiseCanExecuteChanged();

            // start the bus
            await Bus.Start();
        }

        /// <summary>
        /// Stops the bus.
        /// </summary>
        public DelegateCommand StopCommand { get; private set; }

        async void Stop()
        {
            await Bus.Stop();
            Bus = null;

            // we are stopped now
            StartCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
            ScanCommand.RaiseCanExecuteChanged();
        }

        public DelegateCommand ScanCommand { get; private set; }

        async void Scan()
        {
            await Bus.Client.Send(new RnetEventMessage(
                 new RnetDeviceId(RnetControllerId.AllControllers, 0, RnetKeypadId.External),
                 Bus.LocalDevice.DeviceId,
                 new RnetPath(1, 0),
                 new RnetPath(1, 0),
                 (RnetEvent)200,
                 0,
                 1,
                 RnetPriority.High));
        }

        void Bus_Error(object sender, RnetExceptionEventArgs args)
        {
            ExceptionDispatchInfo.Capture(args.Exception).Throw();
        }

    }

}
