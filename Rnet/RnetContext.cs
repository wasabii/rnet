﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Rnet.Util;

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
        /// Validates that the type is compatible.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        object Validate(Type type, object o)
        {
            Contract.Requires<ArgumentNullException>(type != null);


            if (o != null && !type.IsInstanceOfType(o))
                throw new InvalidCastException("Value must be of Key type.");

            return o;
        }

        /// <summary>
        /// Gets or creates an extension of the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        public object GetOrCreate(Type type, Func<object> create)
        {
            Contract.Requires<ArgumentNullException>(type != null);
            Contract.Requires<ArgumentNullException>(create != null);

            return extensions.GetOrCreate(type, i => Validate(i, create()));
        }

        /// <summary>
        /// Gets or creates an extension of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="create"></param>
        /// <returns></returns>
        public T GetOrCreate<T>(Func<T> create)
            where T : class
        {
            Contract.Requires<ArgumentNullException>(create != null);

            return (T)GetOrCreate(typeof(T), create);
        }

        /// <summary>
        /// Gets the extension of the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Get(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            return extensions.GetOrDefault(type);
        }

        /// <summary>
        /// Gets the extension of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>()
        {
            return (T)Get(typeof(T));
        }

        /// <summary>
        /// Sets the extension of the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public object Set(Type type, object value)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            return extensions[type] = Validate(type, value);
        }

        /// <summary>
        /// Sets the extension of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public T Set<T>(T value)
            where T : class
        {
            return (T)Set(typeof(T), value);
        }

    }

}
