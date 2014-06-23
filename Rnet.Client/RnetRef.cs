using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Rnet.Client
{

    /// <summary>
    /// Provides an base class for a reference to a remote RNET object.
    /// </summary>
    public class RnetRef :
        INotifyCompletion
    {

        readonly Uri uri;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="uri"></param>
        public RnetRef(Uri uri)
        {
            Contract.Requires<ArgumentNullException>(uri != null);

            this.uri = uri;
        }

        public RnetRef GetAwaiter()
        {
            return this;
        }

        public bool IsCompleted
        {
            get { return false; }
        }

        public void OnCompleted(Action continuation)
        {
            continuation();
        }

        public RnetObject GetResult()
        {
            return null;
        }

    }

    /// <summary>
    /// Provides an base class for a reference to a remote RNET object.
    /// </summary>
    public class RnetRef<T> :
        RnetRef
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="uri"></param>
        public RnetRef(Uri uri)
            : base(uri)
        {
            Contract.Requires<ArgumentNullException>(uri != null);
        }

        public new RnetRef<T> GetAwaiter()
        {
            return this;
        }

    }

}
