using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Rnet.Service
{

    /// <summary>
    /// Provides a <see cref="SynchronizationContext"/> implementation that is driven by a single thread.
    /// </summary>
    sealed class SingleThreadSynchronizationContext : SynchronizationContext, IDisposable
    {

        Thread thread;
        BlockingCollection<KeyValuePair<SendOrPostCallback, object>> queue =
            new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

        /// <summary>
        /// Posts a callback to the <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="state"></param>
        public override void Post(SendOrPostCallback d, object state)
        {
            queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
        }

        /// <summary>
        /// Starts the synchronization context.
        /// </summary>
        public void Start()
        {
            if (thread == null)
            {
                thread = new Thread(Main);
                thread.Start();
            }
        }

        /// <summary>
        /// Stops the synchronization context.
        /// </summary>
        public void Stop()
        {
            if (queue != null)
                queue.CompleteAdding();

            if (thread != null)
                thread.Join();
        }

        /// <summary>
        /// Main entry point for the private thread.
        /// </summary>
        void Main()
        {
            while (!queue.IsCompleted)
            {
                var prev = SynchronizationContext.Current;

                try
                {
                    // enter new context
                    SynchronizationContext.SetSynchronizationContext(this);

                    // process outstanding actions until the queue ends
                    KeyValuePair<SendOrPostCallback, object> work;
                    while (queue.TryTake(out work, Timeout.Infinite))
                        work.Key(work.Value);
                }
                catch (Exception e)
                {
                    // unknown
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(prev);
                }
            }
        }

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        public void Dispose()
        {
            // in case this was missed
            Stop();

            if (queue != null)
            {
                queue.Dispose();
                queue = null;
            }

            if (thread != null)
            {
                thread.Join();
                thread = null;
            }
        }

    }

}
