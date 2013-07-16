using System.IO;

namespace Rnet
{

    /// <summary>
    /// Defines an RNet handshake message.
    /// </summary>
    public class RnetHandshakeMessage : RnetMessage
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="targetDeviceId"></param>
        /// <param name="sourceDeviceId"></param>
        /// <param name="handshakeType"></param>
        public RnetHandshakeMessage(RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId, RnetHandshakeType handshakeType)
            : base(targetDeviceId, sourceDeviceId, RnetMessageType.Handshake)
        {
            HandshakeType = handshakeType;
        }

        /// <summary>
        /// Gets the type of handshake.
        /// </summary>
        RnetHandshakeType HandshakeType { get; set; }

        internal protected override void WriteBody(RnetStreamWriter writer)
        {
            writer.WriteByte((byte)HandshakeType);
        }

        /// <summary>
        /// Reads an event message from the reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="targetDeviceId"></param>
        /// <param name="sourceDeviceId"></param>
        /// <returns></returns>
        internal static RnetHandshakeMessage Read(RnetMessageBodyReader reader, RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId)
        {
            var handshakeType = (RnetHandshakeType)reader.ReadByte();
            
            return new RnetHandshakeMessage(
                targetDeviceId, sourceDeviceId,
                handshakeType);
        }

        protected override void WriteBodyDebugView(TextWriter writer)
        {
            writer.WriteLine("/* handshake */");
            writer.WriteLine("HandshakeType = {0},", HandshakeType);
        }

    }

}
