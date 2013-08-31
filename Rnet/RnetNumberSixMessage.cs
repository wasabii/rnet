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
    public class RnetNumberSixMessage : RnetMessage
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetNumberSixMessage()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="targetDeviceId"></param>
        /// <param name="sourceDeviceId"></param>
        /// <param name="targetPath"></param>
        /// <param name="sourcePath"></param>
        public RnetNumberSixMessage(RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId, byte b1, byte b2, byte b3, byte b4, byte b5)
            : base(targetDeviceId, sourceDeviceId, RnetMessageType.Number6)
        {
            Byte1 = b1;
            Byte2 = b2;
            Byte3 = b3;
            Byte4 = b4;
            Byte5 = b5;
        }

        public byte Byte1 { get; set; }

        public byte Byte2 { get; set; }

        public byte Byte3 { get; set; }

        public byte Byte4 { get; set; }

        public byte Byte5 { get; set; }

        internal protected override void WriteBody(RnetStreamWriter writer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads an event message from the reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="targetDeviceId"></param>
        /// <param name="sourceDeviceId"></param>
        /// <returns></returns>
        internal static RnetNumberSixMessage Read(RnetMessageBodyReader reader, RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId)
        {
            Contract.Requires<ArgumentNullException>(reader != null);

            return new RnetNumberSixMessage(
                targetDeviceId, sourceDeviceId,
                reader.ReadByte(),
                reader.ReadByte(),
                reader.ReadByte(),
                reader.ReadByte(),
                reader.ReadByte());
        }

        protected override void WriteBodyDebugView(TextWriter writer)
        {
            writer.WriteLine("/* number six */");
            writer.WriteLine("Byte1 = \"{0}\",", Byte1);
            writer.WriteLine("Byte2 = \"{0}\",", Byte2);
            writer.WriteLine("Byte3 = \"{0}\",", Byte3);
            writer.WriteLine("Byte4 = \"{0}\",", Byte4);
            writer.WriteLine("Byte5 = \"{0}\",", Byte5);
        }

    }

}
