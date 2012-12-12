using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rnet
{

    /// <summary>
    /// Defines an RNet event message.
    /// </summary>
    [DebuggerDisplay("{DebugView}")]
    public class RnetEventMessage : RnetMessage
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetEventMessage()
        {

        }

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
        public RnetPath TargetPath { get; set; }

        /// <summary>
        /// Gets the source path of the event.
        /// </summary>
        public RnetPath SourcePath { get; set; }

        /// <summary>
        /// Gets the event ID.
        /// </summary>
        public RnetEvents Event { get; set; }

        /// <summary>
        /// Gets the event timestamp.
        /// </summary>
        public ushort Timestamp { get; set; }

        /// <summary>
        /// Gets the event data.
        /// </summary>
        public ushort Data { get; set; }

        /// <summary>
        /// Gets the event priority.
        /// </summary>
        public byte Priority { get; set; }

        internal protected override void WriteBody(RnetMessageWriter writer)
        {
            TargetPath.Write(writer);
            SourcePath.Write(writer);
            writer.WriteUInt16((ushort)Event);
            writer.WriteUInt16(Timestamp);
            writer.WriteUInt16(Data);
            writer.WriteByte(Priority);
        }

        /// <summary>
        /// Reads an event message from the reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="targetDeviceId"></param>
        /// <param name="sourceDeviceId"></param>
        /// <returns></returns>
        internal static RnetEventMessage Read(RnetMessageReader reader, RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId)
        {
            return new RnetEventMessage(
                targetDeviceId, sourceDeviceId,
                RnetPath.Read(reader),
                RnetPath.Read(reader),
                (RnetEvents)reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadByte());
        }

        /// <summary>
        /// Gets a developer debug string representation of the message.
        /// </summary>
        public override string DebugView
        {
            get { return GetDebugView(); }
        }

        /// <summary>
        /// Implements the getter for DebugView.
        /// </summary>
        /// <returns></returns>
        string GetDebugView()
        {
            return string.Format("{{base = {0}, TargetPath = {1}, SourcePath = {2}, Event = {3}, Timestamp = {4}, Data = {5}, Priority = {6}}}",
                base.DebugView,
                TargetPath.DebugView,
                SourcePath.DebugView, 
                Event, 
                Timestamp, 
                Data, 
                Priority);
        }

    }

}
