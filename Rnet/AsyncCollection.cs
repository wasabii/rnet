using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    public class AsyncCollection<T> : ObservableCollection<T>
    {

        class Subscriber
        {

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="predicate"></param>
            /// <param name="completionSource"></param>
            public Subscriber(Predicate<T> predicate, TaskCompletionSource<T> completionSource)
            {
                Predicate = predicate;
                CompletionSource = completionSource;
            }

            /// <summary>
            /// Expression to test for element.
            /// </summary>
            public Predicate<T> Predicate { get; private set; }

            /// <summary>
            /// Notify upon discovery.
            /// </summary>
            public TaskCompletionSource<T> CompletionSource { get; private set; }

        }

        /// <summary>
        /// Subscriptions to the list.
        /// </summary>
        List<Subscriber> subscribers = new List<Subscriber>();

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
        void Evaluate(IEnumerable<T> items)
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
        /// Returns the discovered item or waits until it is discovered.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public Task<T> GetAsync(Predicate<T> predicate)
        {
            return GetAsync(predicate, CancellationToken.None);
        }

        /// <summary>
        /// Returns the discovered item or waits until it is discovered.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<T> GetAsync(Predicate<T> predicate, CancellationToken cancellationToken)
        {
            lock (subscribers)
            {
                // cancels
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                    new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);

                var item = this.FirstOrDefault(i => predicate(i));
                if (item != null)
                    return Task.FromResult(item);

                // subscribe to device event
                var ts = new TaskCompletionSource<T>();
                subscribers.Add(new Subscriber(predicate, ts));
                return ts.Task;
            }
        }

    }

}
