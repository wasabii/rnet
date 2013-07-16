using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    class AsyncCollection<T> : ObservableCollection<T>
    {

        /// <summary>
        /// Subscriptions to the list.
        /// </summary>
        List<AsyncCollectionSubscriber<T>> subscribers = new List<AsyncCollectionSubscriber<T>>();

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            base.OnCollectionChanged(args);

            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    Evaluate(args.NewItems.OfType<T>());
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Evaluate(this);
                    break;
            }
        }

        /// <summary>
        /// Scans the set of items and notifies waiters.
        /// </summary>
        /// <param name="items"></param>
        internal void Evaluate(IEnumerable<T> items)
        {
            lock (subscribers)
            {
                // attempt to fill any waiting subscriptions
                foreach (var subscriber in subscribers.ToList())
                {
                    // remove canceled subscriptions
                    if (subscriber.CompletionSource.Task.IsCanceled)
                    {
                        subscribers.Remove(subscriber);
                        continue;
                    }

                    // find subscriber item and notify
                    var item = items.FirstOrDefault(i => subscriber.Predicate(i));
                    if (item != null)
                    {
                        subscriber.CompletionSource.SetResult(item);
                        subscribers.Remove(subscriber);
                    }
                }
            }
        }

        /// <summary>
        /// Raised when a new subscription is added.
        /// </summary>
        internal EventHandler<AsyncCollectionSubscriberEventArgs<T>> SubscriberAdded;

        /// <summary>
        /// Raises the SubscriberAdded event.
        /// </summary>
        /// <param name="subscriber"></param>
        void RaiseSubscriberAdded(AsyncCollectionSubscriber<T> subscriber)
        {
            if (SubscriberAdded != null)
                SubscriberAdded(this, new AsyncCollectionSubscriberEventArgs<T>(subscriber));
        }

        /// <summary>
        /// Adds a new subscriber.
        /// </summary>
        /// <param name="subscriber"></param>
        void AddSubscriber(Predicate<T> predicate, TaskCompletionSource<T> taskSource, object userState)
        {
            var subscriber = new AsyncCollectionSubscriber<T>(predicate, taskSource, userState);
            subscribers.Add(subscriber);
            RaiseSubscriberAdded(subscriber);
        }

        /// <summary>
        /// Returns the discovered item or waits until it is discovered.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public Task<T> GetAsync(Predicate<T> predicate)
        {
            return GetAsync(predicate, CancellationToken.None, null);
        }

        /// <summary>
        /// Returns the discovered item or waits until it is discovered.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<T> GetAsync(Predicate<T> predicate, CancellationToken cancellationToken)
        {
            return GetAsync(predicate, cancellationToken, null);
        }

        /// <summary>
        /// Returns the discovered item or waits until it is discovered.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="userState"></param>
        /// <returns></returns>
        public Task<T> GetAsync(Predicate<T> predicate, object userState)
        {
            return GetAsync(predicate, CancellationToken.None, userState);
        }

        /// <summary>
        /// Returns the discovered item or waits until it is discovered.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<T> GetAsync(Predicate<T> predicate, CancellationToken cancellationToken, object userState)
        {
            lock (subscribers)
            {
                var item = this.FirstOrDefault(i => predicate(i));
                if (item != null)
                    return Task.FromResult(item);

                // subscribe to device event
                var tcs = new TaskCompletionSource<T>();
                cancellationToken.Register(() => tcs.TrySetCanceled());
                AddSubscriber(predicate, tcs, userState);
                return tcs.Task;
            }
        }

    }

}
