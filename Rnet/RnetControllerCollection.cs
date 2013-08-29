using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Provides a collection of controllers.
    /// </summary>
    public sealed class RnetControllerCollection : IEnumerable<RnetController>, INotifyCollectionChanged
    {

        ConcurrentDictionary<RnetControllerId, RnetController> controllers =
            new ConcurrentDictionary<RnetControllerId, RnetController>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetControllerCollection(RnetBus bus)
        {
            Bus = bus;
        }

        /// <summary>
        /// Bus controllers are tracked under.
        /// </summary>
        RnetBus Bus { get; set; }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        internal void Clear()
        {
            controllers.Clear();

            // notify users of change
            RaiseCollectionChanged();
        }

        /// <summary>
        /// Gets a controller with the given identifier.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RnetController this[RnetControllerId id]
        {
            get { return GetOrCreate(id); }
        }

        /// <summary>
        /// Gets or creates a new controller object for the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        RnetController GetOrCreate(RnetControllerId id)
        {
            if (RnetControllerId.IsReserved(id))
                return null;

            return controllers
                .GetOrAdd(id, i => new RnetController(Bus, id));
        }

        /// <summary>
        /// Scans for any controllers.
        /// </summary>
        /// <returns></returns>
        public async Task Scan()
        {
            await Task.WhenAll(Enumerable.Range(0, 16)
                .Select(async i => await this[i][0, 0].Read()));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the known controllers.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<RnetController> GetEnumerator()
        {
            return controllers.Values
                .Where(i => i != null)
                .Where(i => i.IsActive)
                .OrderBy(i => i.Id)
                .ToList()
                .GetEnumerator();
        }

        /// <summary>
        /// Invoked when a controller becomes active.
        /// </summary>
        /// <param name="controller"></param>
        internal void OnControllerActive(RnetController controller)
        {
            RaiseCollectionChanged();
        }

        /// <summary>
        /// Raised when an device is added or removed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raises the CollectionChanged event.
        /// </summary>
        /// <param name="args"></param>
        void RaiseCollectionChanged()
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
