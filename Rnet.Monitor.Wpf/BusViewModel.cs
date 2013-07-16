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
        RnetDeviceData selectedDataItem;

        public BusViewModel()
        {
            dispatcher = Dispatcher.CurrentDispatcher;

            SentMessages = new ObservableCollection<RnetMessage>();
            ReceivedMessages = new ObservableCollection<RnetMessage>();

            Client = new RnetClient(new RnetTcpConnection(IPAddress.Parse("192.168.175.1"), 9999));
            Client.StateChanged += (s, a) => dispatcher.Invoke(() => Client_StateChanged(s, a));
            Client.MessageSent += (s, a) => dispatcher.Invoke(() => SentMessages.Add(a.Message));
            Client.MessageReceived += (s, a) => dispatcher.Invoke(() => ReceivedMessages.Add(a.Message));
            Bus = new RnetBus(Client);

            var canStart = this.WhenAny(i => i.Client.State, i => i.Value == RnetClientState.Stopped);
            StartCommand = new ReactiveCommand(canStart, false, System.Reactive.Concurrency.DispatcherScheduler.Current);
            StartCommand.RegisterAsyncAction(i => dispatcher.Invoke(() => Bus.Start()));

            var canStop = this.WhenAny(i => i.Client.State, i => i.Value == RnetClientState.Started);
            StopCommand = new ReactiveCommand(canStop, false, System.Reactive.Concurrency.DispatcherScheduler.Current);
            StopCommand.RegisterAsyncAction(i => dispatcher.Invoke(() => Bus.Stop()));

            var canProbeDevice = this.WhenAny(i => i.SelectedDevice, i => i.Value != null);
            ProbeDeviceCommand = new ReactiveCommand(canProbeDevice, false, System.Reactive.Concurrency.DispatcherScheduler.Current);
            ProbeDeviceCommand.RegisterAsyncAction(i => dispatcher.Invoke(() => DiscoverDeviceData(SelectedDevice)));

            // wrap devices in synchronized collection
            Devices = new SynchronizedCollection<RnetDevice>(Bus.Devices);

            selectedDataItemViewModel = this.ObservableForProperty(i => i.SelectedDataItem)
                .Select(i => i.Value != null ? new DataItemViewModel(i.Value) : null)
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
                DiscoverDevices();
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

        public RnetDeviceData SelectedDataItem
        {
            get { return selectedDataItem; }
            set { this.RaiseAndSetIfChanged(ref selectedDataItem, value); }
        }

        ObservableAsPropertyHelper<object> selectedDataItemViewModel;
        public object SelectedDataItemViewModel
        {
            get { return selectedDataItemViewModel.Value; }
        }

        async void DiscoverDevices()
        {
            await Task.WhenAll(GetDevices());
        }

        IEnumerable<Task<RnetDevice>> GetDevices()
        {
            for (int i = 0; i < 6; i++)
                yield return GetDeviceAsync(new RnetDeviceId(i, 0, RnetKeypadId.Controller));
        }

        async void DiscoverDeviceData(RnetDevice device)
        {
            for (byte i = 0; i < 6; i++)
                await device.Directories.GetAsync(2, 0, i, 7);

            //foreach (var path in GetPaths(2))
            //    await GetDataItemAsync(device, path);

            //foreach (var path in GetPaths(new RnetPath(2, 0), 4))
            //    await GetDataItemAsync(device, path);
        }

        IEnumerable<RnetPath> GetPaths(int maxDepth)
        {
            for (byte i = 0; i < 10; i++)
                foreach (var p in GetPaths(new RnetPath(i), maxDepth))
                    yield return p;
        }

        IEnumerable<RnetPath> GetPaths(RnetPath path, int maxDepth)
        {
            if (path.Length > maxDepth)
                yield break;

            yield return path;

            for (byte i = 0; i < 10; i++)
                foreach (var p in GetPaths(path.Navigate(i), maxDepth))
                    yield return p;
        }

        async Task<RnetDevice> GetDeviceAsync(RnetDeviceId deviceId)
        {
            try
            {
                return await Bus.Devices.GetAsync(deviceId);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        async Task<RnetDeviceDirectory> GetDataItemAsync(RnetDevice device, params byte[] path)
        {
            try
            {
                return await device.Directories.GetAsync(new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token, path);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

    }

}
