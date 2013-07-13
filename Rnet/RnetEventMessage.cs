using System.Diagnostics;
using System.IO;

namespace Rnet.Protocol
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

        internal protected override void WriteBody(RnetStreamWriter writer)
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
        internal static RnetEventMessage Read(RnetMessageBodyReader reader, RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId)
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

        protected override void WriteBodyDebugView(TextWriter writer)
        {
            writer.WriteLine("/* event */");
            writer.WriteLine("TargetPath = \"{0}\",", TargetPath);
            writer.WriteLine("SourcePath = \"{0}\",", SourcePath);
            writer.WriteLine("Event = {0},", Event);
            writer.WriteLine("Timestamp = {0},", Timestamp);
            writer.WriteLine("Data = {0},", Data);
            writer.WriteLine("Priority = {0},", Priority);
        }

    }

}
