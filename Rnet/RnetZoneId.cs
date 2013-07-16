using System;

namespace Rnet
{

    /// <summary>
    /// Specifies an RNET ZoneID component of a <see cref="RnetDeviceId"/>.
    /// </summary>
    public struct RnetZoneId : IComparable<RnetZoneId>, IComparable
    {

        public static readonly RnetZoneId Zone1 = 0x00;
        public static readonly RnetZoneId Zone2 = 0x01;
        public static readonly RnetZoneId Zone3 = 0x02;
        public static readonly RnetZoneId Zone4 = 0x03;
        public static readonly RnetZoneId Zone5 = 0x04;
        public static readonly RnetZoneId Zone6 = 0x05;

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
        /// Implicitly converts a <see cref="Int32"/> to an <see cref="RnetZoneId"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator RnetZoneId(int value)
        {
            return new RnetZoneId((byte)value);
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

        /// <summary>
        /// Indicates whether this instance and the specified object are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is RnetZoneId ? ((RnetZoneId)obj).Value == Value : false;
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
            if (this == Reserved)
                return string.Format("{0} /* AllDevices */", Value);
            if (this == ControllerLink)
                return string.Format("{0} /* Reserved */", Value);
            if (this == Peripheral)
                return string.Format("{0} /* AllZone */", Value);
            if (this == Trace)
                return string.Format("{0} /* RequestId */", Value);

            if (this == Zone1)
                return string.Format("{0} /* Zone1 */", Value);
            if (this == Zone2)
                return string.Format("{0} /* Zone2 */", Value);
            if (this == Zone3)
                return string.Format("{0} /* Zone3 */", Value);
            if (this == Zone4)
                return string.Format("{0} /* Zone4 */", Value);
            if (this == Zone5)
                return string.Format("{0} /* Zone5 */", Value);
            if (this == Zone6)
                return string.Format("{0} /* Zone6 */", Value);

            return Value.ToString();
        }

        /// <summary>
        /// Compares this <see cref="RnetZoneId"/> to another <see cref="RnetZoneId"/>.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        int IComparable<RnetZoneId>.CompareTo(RnetZoneId other)
        {
            return Value.CompareTo(other.Value);
        }

        /// <summary>
        /// Compares the current object with another object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        int IComparable.CompareTo(object obj)
        {
            return ((IComparable<RnetZoneId>)this).CompareTo((RnetZoneId)obj);
        }

    }

}
