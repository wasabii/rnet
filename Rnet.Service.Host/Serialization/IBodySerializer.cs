using System.IO;

using Rnet.Service.Host.Net;

namespace Rnet.Service.Host.Serialization
{

    /// <summary>
    /// Provides serialization abilities.
    /// </summary>
    public interface IBodySerializer
    {

        /// <summary>
        /// Returns <c>true</c> if this <see cref="IBodySerializer"/> can serialize the given object to the given <see cref="MediaRange"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mediaRange"></param>
        /// <returns></returns>
        bool CanSerialize(object value, MediaRange mediaRange);

        /// <summary>
        /// Serializes the specified object to the given output <see cref="Stream"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mediaRange"></param>
        /// <param name="output"></param>
        void Serialize(object value, MediaRange mediaRange, Stream output);

    }

}
