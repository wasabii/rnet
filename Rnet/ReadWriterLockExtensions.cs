
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    public static class ReadWriterLockExtensions
    {

        abstract class Lock : IDisposable
        {

            protected ReaderWriterLockSlim lck;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="lck"></param>
            public Lock(ReaderWriterLockSlim lck)
            {
                this.lck = lck;
            }

            public abstract void Dispose();

        }

        class ReadLock : Lock
        {

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="lck"></param>
            public ReadLock(ReaderWriterLockSlim lck)
                :base(lck)
            {
                this.lck.EnterReadLock();
            }

            public override void Dispose()
            {
                this.lck.ExitReadLock();
            }

        }

        class WriteLock : Lock
        {

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="lck"></param>
            public WriteLock(ReaderWriterLockSlim lck)
                : base(lck)
            {
                this.lck.EnterWriteLock();
            }

            public override void Dispose()
            {
                this.lck.ExitWriteLock();
            }

        }

        /// <summary>
        /// Enters the read lock and returns a disposable to exit.
        /// </summary>
        /// <param name="lck"></param>
        /// <returns></returns>
        public static IDisposable Read(this ReaderWriterLockSlim lck)
        {
            return new ReadLock(lck);
        }

        /// <summary>
        /// Enters the write lock and returns a disposable to exit.
        /// </summary>
        /// <param name="lck"></param>
        /// <returns></returns>
        public static IDisposable Write(this ReaderWriterLockSlim lck)
        {
            return new WriteLock(lck);
        }

    }

}
