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
        public static readonly RnetKeypadId External = 0x70;

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

        public override bool Equals(object obj)
        {
            return obj is RnetKeypadId ? ((RnetKeypadId)obj).Value == Value : false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            if (this == Controller)
                return string.Format("{0} /* AllDevices */", Value);
            if (this == Reserved)
                return string.Format("{0} /* Reserved */", Value);
            if (this == AllZone)
                return string.Format("{0} /* AllZone */", Value);
            if (this == RequestId)
                return string.Format("{0} /* RequestId */", Value);

            return Value.ToString();
        }

    }

}
