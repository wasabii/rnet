namespace Rnet
{

    /// <summary>
    /// Keypad events.
    /// </summary>
    enum RnetEvents : ushort
    {

        Setup = 0x64,
        Previous = 0x67,
        Next = 0x68,
        Plus = 0x69,
        Minus = 0x6a,
        SourceStep = 0x6b,
        Power = 0x6c,
        Stop = 0x6d,
        Pause = 0x6e,
        Favorite1 = 0x6f,
        Favorite2 = 0x70,
        Play = 0x73,
        VolumeUp = 0x7f,
        VolumeDown = 0x80,

        RemoteControlKeyRelease = 0xbf,

        ZoneOnOff = 0xdc,
        AllZonesOnOff = 0xdd,

        SourceSelect = 0xc1,
        SetZoneVolume = 0xde,


    }

}
