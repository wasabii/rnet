using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Rnet
{

    /// <summary>
    /// Provides a collection of controllers.
    /// </summary>
    public class RnetControllerCollection : IEnumerable<RnetController>, INotifyCollectionChanged
    {

        AsyncMonitor monitor = new AsyncMonitor();
        SortedDictionary<RnetControllerId, RnetController> controllers =
            new SortedDictionary<RnetControllerId, RnetController>(Comparer<RnetControllerId>.Default);

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
        /// Adds the given controller to the collection.
        /// </summary>
        /// <param name="controller"></param>
        internal async Task AddAsync(RnetController controller)
        {
            using (await monitor.EnterAsync())
            {
                controllers[controller.Id] = controller;
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, controller));
                monitor.PulseAll();
            }
        }

        /// <summary>
        /// Remvoes the given controller from the collection.
        /// </summary>
        /// <param name="controller"></param>
        internal async Task RemoveAsync(RnetController controller)
        {
            using (await monitor.EnterAsync())
            {
                var item = controllers.Values
                    .Select((i, j) => new { Controller = i, Index = j })
                    .FirstOrDefault(i => i.Controller == controller);
                if (item != null)
                {
                    controllers.Remove(controller.Id);
                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, controller, item.Index));
                    monitor.PulseAll();
                }
            }
        }

        /// <summary>
        /// Gets the controller if it exists.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RnetController this[RnetControllerId id]
        {
            get { return FindAsync(id).Result; }
        }

        /// <summary>
        /// Gets the controller with the given ID if it is already known.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<RnetController> FindAsync(RnetControllerId id)
        {
            return FindAsync(id, Bus.DefaultTimeoutToken);
        }

        /// <summary>
        /// Gets the controller with the given ID if it is already known.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<RnetController> FindAsync(RnetControllerId id, CancellationToken cancellationToken)
        {
            using (await monitor.EnterAsync(cancellationToken))
                return controllers.GetOrDefault(id);
        }

        /// <summary>
        /// Waits for the specified controller to become available.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task<RnetController> WaitAsync(RnetControllerId id, CancellationToken cancellationToken)
        {
            RnetController controller = null;
            while ((controller = await FindAsync(id, cancellationToken)) == null && !cancellationToken.IsCancellationRequested)
                using (await monitor.EnterAsync(cancellationToken))
                    await monitor.WaitAsync(cancellationToken);

            return controller;
        }

        /// <summary>
        /// Gets the device given by the specified <see cref="RnetControllerId"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<RnetController> GetAsync(RnetControllerId id)
        {
            try
            {
                return await GetAsync(id, Bus.DefaultTimeoutToken);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }

            return null;
        }

        /// <summary>
        /// Gets the controller given by the specified <see cref="RnetControllerId"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<RnetController> GetAsync(RnetControllerId id, CancellationToken cancellationToken)
        {
            var controller = await FindAsync(id, cancellationToken);
            if (controller == null)
                controller = await RequestAsync(id, cancellationToken);

            return controller;
        }

        /// <summary>
        /// Requests the controller device from the bus.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<RnetController> RequestAsync(RnetControllerId id)
        {
            try
            {
                return await RequestAsync(id, Bus.DefaultTimeoutToken);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }

            return null;
        }

        /// <summary>
        /// Initiates a device request.
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="cancellationToken"></param>
        public async Task<RnetController> RequestAsync(RnetControllerId controllerId, CancellationToken cancellationToken)
        {
            return (RnetController)await Bus.RequestAsync(new RnetDeviceId(controllerId, 0, RnetKeypadId.Controller), cancellationToken);
        }

        public IEnumerator<RnetController> GetEnumerator()
        {
            return controllers.Values.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Raised when an device is added or removed.
        /// </summary>
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
