namespace Rnet
{

    /// <summary>
    /// Defines an RNet handshake message.
    /// </summary>
    class RnetHandshakeMessage : RnetMessage
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="targetDeviceId"></param>
        /// <param name="sourceDeviceId"></param>
        /// <param name="handshakeType"></param>
        RnetHandshakeMessage(RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId, RnetHandshakeType handshakeType)
            : base(targetDeviceId, sourceDeviceId, RnetMessageType.Handshake)
        {
            HandshakeType = handshakeType;
        }

        /// <summary>
        /// Gets the type of handshake.
        /// </summary>
        RnetHandshakeType HandshakeType { get; set; }

        internal protected override void WriteBody(RnetMessageWriter writer)
        {
            writer.WriteByte((byte)HandshakeType);
        }

    }

}
