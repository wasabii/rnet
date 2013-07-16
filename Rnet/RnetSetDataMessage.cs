using System.Diagnostics;
using System.IO;

namespace Rnet
{

    /// <summary>
    /// Defines an RNet event message.
    /// </summary>
    [DebuggerDisplay("{DebugView}")]
    public class RnetSetDataMessage : RnetMessage
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetSetDataMessage()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="targetDeviceId"></param>
        /// <param name="sourceDeviceId"></param>
        /// <param name="targetPath"></param>
        /// <param name="sourcePath"></param>
        /// <param name="packetNumber"></param>
        /// <param name="packetCount"></param>
        /// <param name="data"></param>
        public RnetSetDataMessage(RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId, RnetPath targetPath,
            RnetPath sourcePath, ushort packetNumber, ushort packetCount, RnetData data)
            : base(targetDeviceId, sourceDeviceId, RnetMessageType.SetData)
        {
            TargetPath = targetPath;
            SourcePath = sourcePath;
            PacketNumber = packetNumber;
            PacketCount = packetCount;
            Data = data;
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
        public ushort PacketNumber { get; set; }

        /// <summary>
        /// Gets the event timestamp.
        /// </summary>
        public ushort PacketCount { get; set; }

        /// <summary>
        /// Gets the event data.
        /// </summary>
        public RnetData Data { get; set; }

        internal protected override void WriteBody(RnetStreamWriter writer)
        {
            TargetPath.Write(writer);
            SourcePath.Write(writer);
            writer.WriteUInt16(PacketNumber);
            writer.WriteUInt16(PacketCount);
            writer.WriteUInt16((ushort)Data.Length);
            for (int i = 0; i < Data.Length; i++)
                writer.WriteByte(Data[i]);
        }

        /// <summary>
        /// Reads an event message from the reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="targetDeviceId"></param>
        /// <param name="sourceDeviceId"></param>
        /// <returns></returns>
        internal static RnetSetDataMessage Read(RnetMessageBodyReader reader, RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId)
        {
            var targetPath = RnetPath.Read(reader);
            var sourcePath = RnetPath.Read(reader);
            var packetNumber = reader.ReadUInt16();
            var packetCount = reader.ReadUInt16();
            var data = RnetData.Read(reader);

            return new RnetSetDataMessage(
                targetDeviceId, sourceDeviceId,
                targetPath,
                sourcePath,
                packetNumber,
                packetCount,
                data);
        }

        protected override void WriteBodyDebugView(TextWriter writer)
        {
            writer.WriteLine("/* set data */");
            writer.WriteLine("TargetPath = \"{0}\",", TargetPath);
            writer.WriteLine("SourcePath = \"{0}\",", SourcePath);
            writer.WriteLine("PacketNumber = {0},", PacketNumber);
            writer.WriteLine("PacketCount = {0},", PacketCount);
            writer.WriteLine("Data = {0},", Data);
        }

    }

}
