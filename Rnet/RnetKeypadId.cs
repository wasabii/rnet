namespace Rnet
{

    /// <summary>
    /// Specifies an RNET KeypadID component of a <see cref="RnetDeviceId"/>.
    /// </summary>
    public struct RnetKeypadId
    {

        public static readonly RnetKeypadId Controller = 0x7f;
        public static readonly RnetKeypadId Reserved = 0x7e;
        public static readonly RnetKeypadId AllZone = 0x7d;
        public static readonly RnetKeypadId RequestId = 0x7c;

        /// <summary>
        /// Implicitly converts a <see cref="RnetKeypadId"/> to a <see cref="Byte"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static implicit operator byte(RnetKeypadId id)
        {
            return id.Value;
        }

        /// <summary>
        /// Implicitly converts a <see cref="Byte"/> to a <see cref="RnetKeypadId"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static implicit operator RnetKeypadId(byte value)
        {
            return new RnetKeypadId(value);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="value"></param>
        public RnetKeypadId(byte value)
            : this()
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the underlying value of the keypad ID.
        /// </summary>
        public byte Value { get; set; }

        public override string ToString()
        {
            return Value.ToString();
        }

    }

}
