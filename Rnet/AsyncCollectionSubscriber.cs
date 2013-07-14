using System;
using System.Threading.Tasks;

namespace Rnet
{

    internal class AsyncCollectionSubscriber<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="completionSource"></param>
        public AsyncCollectionSubscriber(Predicate<T> predicate, TaskCompletionSource<T> completionSource, object userState)
        {
            Predicate = predicate;
            CompletionSource = completionSource;
            UserState = userState;
        }

        /// <summary>
        /// Expression to test for element.
        /// </summary>
        public Predicate<T> Predicate { get; private set; }

        /// <summary>
        /// Notify upon discovery.
        /// </summary>
        public TaskCompletionSource<T> CompletionSource { get; private set; }

        /// <summary>
        /// Various data that the user can append.
        /// </summary>
        public object UserState { get; private set; }

    }

}
