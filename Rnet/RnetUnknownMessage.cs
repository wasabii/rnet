using System;
using System.Diagnostics;
using System.IO;

namespace Rnet
{

    /// <summary>
    /// An unknown RNET message.
    /// </summary>
    [DebuggerDisplay("{DebugView}")]
    public class RnetUnknownMessage : RnetMessage
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetUnknownMessage()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="targetDeviceId"></param>
        /// <param name="sourceDeviceId"></param>
        /// <param name="messageType"></param>
        public RnetUnknownMessage(RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId, RnetMessageType messageType)
            : base(targetDeviceId, sourceDeviceId, messageType)
        {

        }

        internal protected override void WriteBody(RnetStreamWriter writer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads an event message from the reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="targetDeviceId"></param>
        /// <param name="sourceDeviceId"></param>
        /// <returns></returns>
        internal static RnetUnknownMessage Read(RnetMessageBodyReader reader, RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId, RnetMessageType messageType)
        {
            return new RnetUnknownMessage(
                targetDeviceId, sourceDeviceId,
                messageType);
        }

        protected override void WriteBodyDebugView(TextWriter writer)
        {
            writer.WriteLine("/* unknown: {0} */", (int)MessageType);
        }

    }

}
