namespace Rnet
{

    /// <summary>
    /// Defines a RNet message.
    /// </summary>
    public abstract class RnetMessage
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetMessage()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="targetDeviceId"></param>
        /// <param name="sourceDeviceId"></param>
        /// <param name="messageType"></param>
        protected RnetMessage(RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId, RnetMessageType messageType)
            : this()
        {
            TargetDeviceId = targetDeviceId;
            SourceDeviceId = sourceDeviceId;
            MessageType = messageType;
        }

        /// <summary>
        /// Gets or sets the target Device ID.
        /// </summary>
        public RnetDeviceId TargetDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the source Device ID.
        /// </summary>
        public RnetDeviceId SourceDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        public RnetMessageType MessageType { get; set; }

        /// <summary>
        /// Writes the message using the given writer.
        /// </summary>
        /// <param name="writer"></param>
        internal void Write(RnetMessageWriter writer)
        {
            writer.BeginMessage(TargetDeviceId, SourceDeviceId, MessageType);
            WriteBody(writer);
            writer.EndMessage();
        }

        /// <summary>
        /// Writes the body of the message.
        /// </summary>
        /// <param name="writer"></param>
        internal protected abstract void WriteBody(RnetMessageWriter writer);

        /// <summary>
        /// Gets a string suitable for debugging the contents of this message.
        /// </summary>
        public virtual string DebugView
        {
            get { return string.Format("{{TargetDeviceId = {0}, SourceDeviceId = {1}, MessageType = {2}}}", TargetDeviceId.DebugView, SourceDeviceId.DebugView, MessageType); }
        }

    }

}
