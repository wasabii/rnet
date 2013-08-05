using System;
using System.Collections.Generic;

namespace Rnet
{

    /// <summary>
    /// Provides a collection of arbitrary items on bus objects for usage by external libraries.
    /// </summary>
    public sealed class RnetContext
    {

        Dictionary<Type, object> extensions =
            new Dictionary<Type, object>();

        /// <summary>
        /// Gets or creates an extension of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="create"></param>
        /// <returns></returns>
        public T GetOrCreate<T>(Func<T> create)
        {
            return (T)extensions.GetOrCreate(typeof(T), i => create());
        }

        /// <summary>
        /// Gets or creates an extension of the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        public object GetOrCreate(Type type, Func<object> create)
        {
            return extensions.GetOrCreate(type, i => create());
        }

        /// <summary>
        /// Gets the extension of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>()
        {
            return (T)extensions.GetOrDefault(typeof(T));
        }

        /// <summary>
        /// Gets the extension of the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Get(Type type)
        {
            return extensions.GetOrDefault(type);
        }

    }

}
