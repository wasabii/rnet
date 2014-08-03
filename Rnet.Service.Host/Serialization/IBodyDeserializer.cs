using System;
using System.IO;
using Rnet.Service.Host.Net;

namespace Rnet.Service.Host.Serialization
{

    /// <summary>
    /// Provides deserialization abilities.
    /// </summary>
    public interface IBodyDeserializer
    {

        /// <summary>
        /// Returns <c>true</c> if this <see cref="IBodyDeserializer"/> can deserialize the given object from the given <see cref="MediaRange"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="target"></param>
        /// <param name="mediaRange"></param>
        /// <returns></returns>
        bool CanDeserialize(Type type, object target, MediaRange mediaRange);

        /// <summary>
        /// Deserializes an object from the given input <see cref="Stream"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="target"></param>
        /// <param name="input"></param>
        object Deserialize(Type type, object target, Stream input);

    }

}
