using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using Nito.AsyncEx;

namespace Rnet
{

    /// <summary>
    /// Holds a collection of zones.
    /// </summary>
    public sealed class RnetZoneCollection : IEnumerable<RnetZone>, INotifyCollectionChanged
    {

        AsyncMonitor monitor = new AsyncMonitor();
        SortedDictionary<RnetZoneId, RnetZone> zones =
            new SortedDictionary<RnetZoneId, RnetZone>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal RnetZoneCollection(RnetController controller)
        {
            Controller = controller;
        }

        /// <summary>
        /// Controller holding this zone collection.
        /// </summary>
        RnetController Controller { get; set; }

        /// <summary>
        /// Gets the zone with the given ZoneID.
        /// </summary>
        /// <param name="zoneId"></param>
        /// <returns></returns>
        public RnetZone this[RnetZoneId zoneId]
        {
            get { return FindAsync(zoneId).Result; }
        }

        /// <summary>
        /// Adds a zone.
        /// </summary>
        /// <param name="zone"></param>
        internal async Task AddAsync(RnetZone zone)
        {
            using (await monitor.EnterAsync())
            {
                zones[zone.Id] = zone;
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, zone));
                monitor.PulseAll();
            }
        }

        /// <summary>
        /// Removes a zone.
        /// </summary>
        /// <param name="zone"></param>
        internal async Task RemoveAsync(RnetZone zone)
        {
            using (await monitor.EnterAsync())
            {
                zones.Remove(zone.Id);
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, zone));
                monitor.PulseAll();
            }
        }

        /// <summary>
        /// Gets the zone with the given ID if it is already known.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<RnetZone> FindAsync(RnetZoneId id)
        {
            return FindAsync(id, Controller.Bus.DefaultTimeoutToken);
        }

        /// <summary>
        /// Gets the zone with the given ID if it is already known.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<RnetZone> FindAsync(RnetZoneId id, CancellationToken cancellationToken)
        {
            using (await monitor.EnterAsync(cancellationToken))
                return zones.GetOrDefault(id);
        }

        /// <summary>
        /// Waits for the specified controller to become available.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task<RnetZone> WaitAsync(RnetZoneId id, CancellationToken cancellationToken)
        {
            RnetZone zone = null;
            while ((zone = await FindAsync(id)) == null && !cancellationToken.IsCancellationRequested)
                using (await monitor.EnterAsync(cancellationToken))
                    await monitor.WaitAsync(cancellationToken);

            return zone;
        }

        /// <summary>
        /// Gets the zone given by the specified <see cref="RnetZoneId"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<RnetZone> GetAsync(RnetZoneId id)
        {
            return GetAsync(id, Controller.Bus.DefaultTimeoutToken);
        }

        /// <summary>
        /// Gets the zone given by the specified <see cref="RnetZoneId"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<RnetZone> GetAsync(RnetZoneId id, CancellationToken cancellationToken)
        {
            var zone = await FindAsync(id, cancellationToken);
            if (zone == null)
                zone = await RequestAsync(id, cancellationToken);

            return zone;
        }

        /// <summary>
        /// Requests the zone device from the bus.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<RnetZone> RequestAsync(RnetZoneId id)
        {
            return RequestAsync(id, Controller.Bus.DefaultTimeoutToken);
        }

        /// <summary>
        /// Initiates a zone request.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        public async Task<RnetZone> RequestAsync(RnetZoneId id, CancellationToken cancellationToken)
        {
            return await Controller.RequestAsync(id, cancellationToken);
        }

        public IEnumerator<RnetZone> GetEnumerator()
        {
            return zones.Values.ToList().GetEnumerator();
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
