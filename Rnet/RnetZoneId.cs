namespace Rnet
{

    /// <summary>
    /// Specifies an RNET ZoneID component of a <see cref="RnetDeviceId"/>.
    /// </summary>
    public struct RnetZoneId
    {

        public static readonly RnetZoneId Reserved = 0x7f;
        public static readonly RnetZoneId ControllerLink = 0x7e;
        public static readonly RnetZoneId Peripheral = 0x7d;
        public static readonly RnetZoneId Trace = 0x7c;

        /// <summary>
        /// Implicitly converts a <see cref="RnetZoneId"/> to a <see cref="Byte"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static implicit operator byte(RnetZoneId id)
        {
            return id.Value;
        }

        /// <summary>
        /// Implicitly converts a <see cref="Byte"/> to an <see cref="RnetZoneId"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator RnetZoneId(byte value)
        {
            return new RnetZoneId(value);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="value"></param>
        public RnetZoneId(byte value)
            : this()
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the underlying value of the zone ID.
        /// </summary>
        public byte Value { get; set; }

        public override string ToString()
        {
            return Value.ToString();
        }

    }

}
