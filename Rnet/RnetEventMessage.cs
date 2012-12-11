namespace Rnet
{

    /// <summary>
    /// Defines an RNet event message.
    /// </summary>
    class RnetEventMessage : RnetMessage
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="targetDeviceId"></param>
        /// <param name="sourceDeviceId"></param>
        /// <param name="targetPath"></param>
        /// <param name="sourcePath"></param>
        internal RnetEventMessage(RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId, RnetPath targetPath,
            RnetPath sourcePath, ushort id, ushort timestamp, ushort data, byte priority)
            : base(targetDeviceId, sourceDeviceId, RnetMessageType.Event)
        {
            TargetPath = targetPath;
            SourcePath = sourcePath;
            Id = id;
            Timestamp = timestamp;
            Data = data;
            Priority = priority;
        }

        /// <summary>
        /// Gets the target path of the event.
        /// </summary>
        RnetPath TargetPath { get; set; }

        /// <summary>
        /// Gets the source path of the event.
        /// </summary>
        RnetPath SourcePath { get; set; }

        /// <summary>
        /// Gets the event ID.
        /// </summary>
        ushort Id { get; set; }

        /// <summary>
        /// Gets the event timestamp.
        /// </summary>
        ushort Timestamp { get; set; }

        /// <summary>
        /// Gets the event data.
        /// </summary>
        ushort Data { get; set; }

        /// <summary>
        /// Gets the event priority.
        /// </summary>
        byte Priority { get; set; }

        protected override void WriteBody(RnetMessageWriter writer)
        {
            writer.WritePath(TargetPath);
            writer.WritePath(SourcePath);
            writer.WriteUInt16(Id);
            writer.WriteUInt16(Timestamp);
            writer.WriteUInt16(Data);
            writer.WriteByte(Priority);
        }

    }

}
