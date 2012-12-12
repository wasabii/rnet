using System;

namespace Rnet
{

    /// <summary>
    /// Defines an RNet event message.
    /// </summary>
    public class RnetEventMessage : RnetMessage
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="targetDeviceId"></param>
        /// <param name="sourceDeviceId"></param>
        /// <param name="targetPath"></param>
        /// <param name="sourcePath"></param>
        public RnetEventMessage(RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId, RnetPath targetPath,
            RnetPath sourcePath, RnetEvents evt, ushort timestamp, ushort data, byte priority)
            : base(targetDeviceId, sourceDeviceId, RnetMessageType.Event)
        {
            TargetPath = targetPath ?? new RnetPath();
            SourcePath = sourcePath ?? new RnetPath();
            Event = evt;
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
        RnetEvents Event { get; set; }

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

        internal protected override void WriteBody(RnetMessageWriter writer)
        {
            TargetPath.Write(writer);
            SourcePath.Write(writer);
            writer.WriteUInt16((ushort)Event);
            writer.WriteUInt16(Timestamp);
            writer.WriteUInt16(Data);
            writer.WriteByte(Priority);
        }

    }

}
