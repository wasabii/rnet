using System;

namespace Rnet
{

    /// <summary>
    /// Generic event args that holds a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValueEventArgs<T> : EventArgs
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="value"></param>
        public ValueEventArgs(T value)
        {
            Value = value;
        }

        public T Value { get; private set; }

    }

}
