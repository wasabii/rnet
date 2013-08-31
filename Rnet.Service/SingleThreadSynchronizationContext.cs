using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Rnet.Service
{

    sealed class SingleThreadSynchronizationContext : SynchronizationContext
    {

        Thread thread;
        BlockingCollection<KeyValuePair<SendOrPostCallback, object>> queue = 
            new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

        public override void Post(SendOrPostCallback d, object state)
        {
            queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
        }

        /// <summary>
        /// Entry point into the synchronization context.
        /// </summary>
        public void Start()
        {
            Contract.Assert(thread == null);

            thread = new Thread(Main);
            thread.Start();
        }

        void Main()
        {
            var prev = SynchronizationContext.Current;

            try
            {
                // enter new context
                SynchronizationContext.SetSynchronizationContext(this);

                // process outstanding actions
                KeyValuePair<SendOrPostCallback, object> work;
                while (queue.TryTake(out work, Timeout.Infinite))
                    work.Key(work.Value);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prev);
            }
        }

        /// <summary>
        /// Finishes the synchronization context.
        /// </summary>
        public void Complete()
        {
            Contract.Assert(queue != null);
            Contract.Assert(thread != null);

            queue.CompleteAdding();
            thread.Join();
        }

    }

}
