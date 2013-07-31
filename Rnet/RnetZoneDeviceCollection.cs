using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Nito.AsyncEx;

namespace Rnet
{

    public sealed class RnetZoneDeviceCollection : IEnumerable<RnetZoneDevice>, INotifyCollectionChanged
    {

        AsyncMonitor monitor = new AsyncMonitor();
        SortedDictionary<RnetKeypadId, RnetZoneDevice> devices =
            new SortedDictionary<RnetKeypadId, RnetZoneDevice>(Comparer<RnetKeypadId>.Default);

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="zone"></param>
        internal RnetZoneDeviceCollection(RnetZone zone)
        {
            Zone = zone;
        }

        /// <summary>
        /// The zone for which these devices are underneath.
        /// </summary>
        RnetZone Zone { get; set; }

        /// <summary>
        /// Gets the device with the given keypad ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RnetZoneDevice this[RnetKeypadId id]
        {
            get { return FindAsync(id).Result; }
        }

        /// <summary>
        /// Adds a device.
        /// </summary>
        /// <param name="device"></param>
        internal async Task AddAsync(RnetZoneDevice device)
        {
            using (await monitor.EnterAsync())
            {
                devices[device.Id] = device;
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, device));
                monitor.PulseAll();
            }
        }

        /// <summary>
        /// Removes a device.
        /// </summary>
        /// <param name="device"></param>
        internal async Task RemoveAsync(RnetZoneDevice device)
        {
            using (await monitor.EnterAsync())
            {
                devices.Remove(device.Id);
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, device));
                monitor.PulseAll();
            }
        }

        /// <summary>
        /// Gets the device with the given ID if it is already known.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<RnetZoneDevice> FindAsync(RnetKeypadId id)
        {
            return FindAsync(id, Zone.Controller.Bus.DefaultCancellationToken);
        }

        /// <summary>
        /// Gets the device with the given ID if it is already known.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<RnetZoneDevice> FindAsync(RnetKeypadId id, CancellationToken cancellationToken)
        {
            using (await monitor.EnterAsync(cancellationToken))
                return devices.GetOrDefault(id);
        }

        /// <summary>
        /// Waits for the specified device to become available.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task<RnetZoneDevice> WaitAsync(RnetKeypadId id, CancellationToken cancellationToken)
        {
            RnetZoneDevice device = null;
            using (await monitor.EnterAsync(cancellationToken))
                while ((device = await FindAsync(id)) == null && !cancellationToken.IsCancellationRequested)
                    await monitor.WaitAsync(cancellationToken);

            return device;
        }

        /// <summary>
        /// Gets the device given by the specified <see cref="RnetKeypadId"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<RnetZoneDevice> GetAsync(RnetKeypadId id)
        {
            return GetAsync(id, Zone.Controller.Bus.DefaultCancellationToken);
        }

        /// <summary>
        /// Gets the device given by the specified <see cref="RnetKeypadId"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<RnetZoneDevice> GetAsync(RnetKeypadId id, CancellationToken cancellationToken)
        {
            var device = await FindAsync(id, cancellationToken);
            if (device == null)
                device = await RequestAsync(id, cancellationToken);

            return device;
        }

        /// <summary>
        /// Requests the device from the bus.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<RnetZoneDevice> RequestAsync(RnetKeypadId id)
        {
            return RequestAsync(id, Zone.Controller.Bus.DefaultCancellationToken);
        }

        /// <summary>
        /// Initiates a device request.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        public async Task<RnetZoneDevice> RequestAsync(RnetKeypadId id, CancellationToken cancellationToken)
        {
            return (RnetZoneDevice)await Zone.Controller.Bus.RequestAsync(new RnetDeviceId(Zone.Controller.Id, Zone.Id, id), cancellationToken);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the devices within this zone.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<RnetZoneDevice> GetEnumerator()
        {
            return devices.Values.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raises the CollectionChanged event.
        /// </summary>
        /// <param name="args"></param>
        void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

    }

}
