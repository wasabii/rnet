using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

using ReactiveUI;

namespace Rnet.Monitor.Wpf
{

    public class BusViewModel : ReactiveObject
    {

        Dispatcher dispatcher;
        RnetDevice selectedDevice;
        RnetDataItem selectedDataItem;

        public BusViewModel()
        {
            dispatcher = Dispatcher.CurrentDispatcher;

            SentMessages = new ObservableCollection<RnetMessage>();
            ReceivedMessages = new ObservableCollection<RnetMessage>();

            Client = new RnetClient(new RnetTcpConnection(IPAddress.Parse("192.168.175.1"), 9999));
            Client.StateChanged += Client_StateChanged;
            Client.MessageSent += (s, a) => dispatcher.Invoke(() => SentMessages.Add(a.Message));
            Client.MessageReceived += (s, a) => dispatcher.Invoke(() => ReceivedMessages.Add(a.Message));
            Bus = new RnetBus(Client);

            var canStart = this.WhenAny(i => i.Client.State, i => i.Value == RnetClientState.Stopped);
            StartCommand = new ReactiveCommand(canStart);
            StartCommand.RegisterAsyncAction(i => Bus.Start());

            var canStop = this.WhenAny(i => i.Client.State, i => i.Value == RnetClientState.Started);
            StopCommand = new ReactiveCommand(canStop);
            StopCommand.RegisterAsyncAction(i => Bus.Stop());

            var canProbeDevice = this.WhenAny(i => i.SelectedDevice, i => i.Value != null);
            ProbeDeviceCommand = new ReactiveCommand(canProbeDevice);
            ProbeDeviceCommand.RegisterAsyncAction(i => ProbeDevice(SelectedDevice));

            // wrap devices in synchronized collection
            Devices = new SynchronizedCollection<RnetDevice>(Bus.Devices);

            // subscribe data items collection to selected device
            dataItems = this.ObservableForProperty(i => i.SelectedDevice)
                .Select(i => i.Value.DataItems)
                .Select(i => new SynchronizedCollection<RnetDataItem>(i))
                .ToProperty(this, i => i.DataItems);

            selectedDataItemViewModel = this.ObservableForProperty(i => i.SelectedDataItem)
                .Select(i => new DataItemViewModel(i.Value))
                .ToProperty(this, i => i.SelectedDataItemViewModel);
        }

        /// <summary>
        /// Invoked when the client becomes connected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Client_StateChanged(object sender, RnetClientStateEventArgs args)
        {
            if (args.State == RnetClientState.Started)
                ProbeDevices();
        }

        public ObservableCollection<RnetMessage> SentMessages { get; private set; }

        public ObservableCollection<RnetMessage> ReceivedMessages { get; private set; }

        public RnetClient Client { get; private set; }

        public RnetBus Bus { get; private set; }

        public ReactiveCommand StartCommand { get; private set; }

        public ReactiveCommand StopCommand { get; private set; }

        public ReactiveCommand ProbeDeviceCommand { get; private set; }

        public SynchronizedCollection<RnetDevice> Devices { get; private set; }

        public RnetDevice SelectedDevice
        {
            get { return selectedDevice; }
            set { this.RaiseAndSetIfChanged(ref selectedDevice, value); }
        }

        ObservableAsPropertyHelper<SynchronizedCollection<RnetDataItem>> dataItems;
        public SynchronizedCollection<RnetDataItem> DataItems
        {
            get { return dataItems.Value; }
        }

        public RnetDataItem SelectedDataItem
        {
            get { return selectedDataItem; }
            set { this.RaiseAndSetIfChanged(ref selectedDataItem, value); }
        }

        ObservableAsPropertyHelper<object> selectedDataItemViewModel;
        public object SelectedDataItemViewModel
        {
            get { return selectedDataItemViewModel.Value; }
        }

        async void ProbeDevices()
        {
            foreach (var t in GetDevicesAsync())
                await t;
        }

        IEnumerable<Task<RnetDevice>> GetDevicesAsync()
        {
            for (int i = 0; i < 6; i++)
                yield return Bus.Devices.GetAsync(new RnetDeviceId(i, 0, RnetKeypadId.Controller));
        }

        async void ProbeDevice(RnetDevice device)
        {
            for (byte i = 0; i < 16; i++)
                await ProbeDevice(device, new RnetPath(i));
        }

        async Task<RnetDataItem> ProbeDevice(RnetDevice device, RnetPath path)
        {
            if (path.Length > 4)
                return null;

            var data = await GetDataItemAsync(device, path);
            for (byte i = 0; i < 16; i++)
                await ProbeDevice(device, path.Child(i));

            return data;
        }

        async Task<RnetDataItem> GetDataItemAsync(RnetDevice device, RnetPath path)
        {
            try
            {
                return await device.DataItems.GetAsync(path, new CancellationTokenSource(TimeSpan.FromSeconds(.25)).Token);
            }
            catch (TaskCanceledException e)
            {
                // ignore
            }

            return null;
        }

    }

}
