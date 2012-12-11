namespace Rnet
{

    /// <summary>
    /// Defines a RNet message.
    /// </summary>
    struct RnetMessage
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="targetDeviceId"></param>
        /// <param name="sourceDeviceId"></param>
        /// <param name="messageType"></param>
        RnetMessage(RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId, RnetMessageType messageType)
            :this()
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

    }

}
