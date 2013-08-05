using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Provides a collection of controllers.
    /// </summary>
    public class RnetControllerCollection : IEnumerable<RnetController>, INotifyCollectionChanged
    {

        ConcurrentDictionary<RnetControllerId, WeakReference<RnetController>> controllers =
            new ConcurrentDictionary<RnetControllerId, WeakReference<RnetController>>();

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
                .GetOrAdd(id, i => new WeakReference<RnetController>(new RnetController(Bus, id)))
                .GetTargetOrDefault();
        }

        /// <summary>
        /// Scans for any controllers.
        /// </summary>
        /// <returns></returns>
        public IObservable<RnetController> Scan()
        {
            // active
            var l1 = controllers.Values
                .Select(i => i.GetTargetOrDefault())
                .Where(i => i != null)
                .Where(i => i.IsActive)
                .OrderBy(i => i.Id)
                .Select(i => i.Id)
                .ToList();

            // inactive
            var l2 = controllers.Values
                .Select(i => i.GetTargetOrDefault())
                .Where(i => i != null)
                .Where(i => !i.IsActive)
                .OrderBy(i => i.Id)
                .Select(i => i.Id)
                .ToList();

            // unknown
            var l3 = Enumerable.Range(0, 32)
                .Select(i => new RnetControllerId((byte)i))
                .Except(l1)
                .Except(l2)
                .OrderBy(i => i)
                .ToList();

            // query for a random bit of data for each
            return Enumerable.Empty<RnetControllerId>()
                .Concat(l1)
                .Concat(l2)
                .Concat(l3)
                .ToObservable()
                .Select(i =>
                    Observable.FromAsync(async () =>
                        new { Data = await this[i][0, 0].Refresh(), Id = i }))
                .Merge()
                .Where(i => i.Data != null)
                .Select(i => this[i.Id]);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the known controllers.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<RnetController> GetEnumerator()
        {
            return controllers.Values
                .Select(i => i.GetTargetOrDefault())
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
