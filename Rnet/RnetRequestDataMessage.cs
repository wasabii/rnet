using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;

namespace Rnet
{

    /// <summary>
    /// Defines an RNet event message.
    /// </summary>
    [DebuggerDisplay("{DebugView}")]
    public class RnetRequestDataMessage : RnetMessage
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetRequestDataMessage()
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
        public RnetRequestDataMessage(RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId, RnetPath targetPath,
            RnetPath sourcePath, RnetRequestMessageType type)
            : base(targetDeviceId, sourceDeviceId, RnetMessageType.RequestData)
        {
            TargetPath = targetPath;
            SourcePath = sourcePath;
            Type = type;
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
        /// Type of request.
        /// </summary>
        public RnetRequestMessageType Type { get; set; }

        internal protected override void WriteBody(RnetStreamWriter writer)
        {
            TargetPath.Write(writer);
            SourcePath.Write(writer);
            writer.WriteByte((byte)Type);
        }

        /// <summary>
        /// Reads an event message from the reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="targetDeviceId"></param>
        /// <param name="sourceDeviceId"></param>
        /// <returns></returns>
        internal static RnetRequestDataMessage Read(RnetMessageBodyReader reader, RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId)
        {
            Contract.Requires<ArgumentNullException>(reader != null);

            var targetPath = RnetPath.Read(reader);
            var sourcePath = RnetPath.Read(reader);
            var type = (RnetRequestMessageType)reader.ReadByte();

            return new RnetRequestDataMessage(
                targetDeviceId, sourceDeviceId,
                targetPath,
                sourcePath,
                type);
        }

        protected override void WriteBodyDebugView(TextWriter writer)
        {
            writer.WriteLine("/* request data */");
            writer.WriteLine("TargetPath = \"{0}\",", TargetPath);
            writer.WriteLine("SourcePath = \"{0}\",", SourcePath);
            writer.WriteLine("Type = {0},", Type);
        }

    }

}
