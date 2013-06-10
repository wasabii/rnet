using System.Diagnostics;
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
            RnetPath sourcePath)
            : base(targetDeviceId, sourceDeviceId, RnetMessageType.RequestData)
        {
            TargetPath = targetPath ?? new RnetPath();
            SourcePath = sourcePath ?? new RnetPath();
        }

        /// <summary>
        /// Gets the target path of the event.
        /// </summary>
        public RnetPath TargetPath { get; set; }

        /// <summary>
        /// Gets the source path of the event.
        /// </summary>
        public RnetPath SourcePath { get; set; }

        internal protected override void WriteBody(RnetStreamWriter writer)
        {
            TargetPath.Write(writer);
            SourcePath.Write(writer);
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
            var targetPath = RnetPath.Read(reader);
            var sourcePath = RnetPath.Read(reader);

            return new RnetRequestDataMessage(
                targetDeviceId, sourceDeviceId,
                targetPath,
                sourcePath);
        }

        protected override void WriteBodyDebugView(TextWriter writer)
        {
            writer.WriteLine("/* request data */");
            writer.WriteLine("TargetPath = \"{0}\",", TargetPath);
            writer.WriteLine("SourcePath = \"{0}\",", SourcePath);
        }

    }

}
