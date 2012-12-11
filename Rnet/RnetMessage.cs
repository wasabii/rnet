namespace Rnet
{

    /// <summary>
    /// Defines a RNet message.
    /// </summary>
    abstract class RnetMessage
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="targetDeviceId"></param>
        /// <param name="sourceDeviceId"></param>
        /// <param name="messageType"></param>
        protected RnetMessage(RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId, RnetMessageType messageType)
        {
            TargetDeviceId = targetDeviceId;
            SourceDeviceId = sourceDeviceId;
            MessageType = messageType;
        }

        /// <summary>
        /// Gets or sets the target Device ID.
        /// </summary>
        RnetDeviceId TargetDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the source Device ID.
        /// </summary>
        RnetDeviceId SourceDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        RnetMessageType MessageType { get; set; }

        /// <summary>
        /// Writes the message using the given writer.
        /// </summary>
        /// <param name="writer"></param>
        internal void Write(RnetMessageWriter writer)
        {
            writer.WriteStart();
            writer.WriteDeviceId(TargetDeviceId);
            writer.WriteDeviceId(SourceDeviceId);
            writer.WriteMessageType(MessageType);
            WriteBody(writer);
            writer.WriteChecksum();
            writer.WriteEnd();
        }

        /// <summary>
        /// Writes the body of the message.
        /// </summary>
        /// <param name="writer"></param>
        protected abstract void WriteBody(RnetMessageWriter writer);

    }

}
