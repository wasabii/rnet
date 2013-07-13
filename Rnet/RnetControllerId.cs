namespace Rnet
{

    /// <summary>
    /// Specifies an RNET ControllerID component of a <see cref="RnetDeviceId"/>.
    /// </summary>
    public struct RnetControllerId
    {

        public static readonly RnetControllerId AllDevices = 0x7f;

        /// <summary>
        /// Implicitly converts a <see cref="RnetControllerId"/> to a <see cref="Byte"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static implicit operator byte(RnetControllerId id)
        {
            return id.Value;
        }

        /// <summary>
        /// Implicitly converts a <see cref="Byte"/> to a <see cref="RnetControllerId"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static implicit operator RnetControllerId(byte value)
        {
            return new RnetControllerId(value);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="value"></param>
        public RnetControllerId(byte value)
            : this()
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the underlying value of the controller ID.
        /// </summary>
        public byte Value { get; set; }

        /// <summary>
        /// Indicates whether this instance and the specified object are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is RnetControllerId ? ((RnetControllerId)obj).Value == Value : false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            if (this == AllDevices)
                return string.Format("{0} /* AllDevices */", Value);

            return Value.ToString();
        }

    }

}
