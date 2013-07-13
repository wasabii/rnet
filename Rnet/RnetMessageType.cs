namespace Rnet
{

    /// <summary>
    /// Defines the type of message being sent.
    /// </summary>
    public enum RnetMessageType : byte
    {

        SetData = 0x00,
        RequestData = 0x01,
        Handshake = 0x02,
        Event = 0x05,

    }

}
