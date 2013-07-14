using System;

namespace Rnet
{

    class AsyncCollectionSubscriberEventArgs<T> : EventArgs
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="subscriber"></param>
        public AsyncCollectionSubscriberEventArgs(AsyncCollectionSubscriber<T> subscriber)
        {
            Subscriber = subscriber;
        }

        public AsyncCollectionSubscriber<T> Subscriber { get; private set; }

    }

}
