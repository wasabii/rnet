using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Rnet
{

    /// <summary>
    /// RNET DeviceID structure.
    /// </summary>
    public struct RnetDeviceId : IComparable<RnetDeviceId>, IComparable
    {

        /// <summary>
        /// <see cref="RnetDeviceId"/> which targets the root controller.
        /// </summary>
        public static readonly RnetDeviceId RootController = new RnetDeviceId(0, 0, RnetKeypadId.Controller);

        /// <summary>
        /// <see cref="RnetDeviceId"/> which targets all devices.
        /// </summary>
        public static readonly RnetDeviceId AllDevices = new RnetDeviceId(RnetControllerId.AllDevices, 0, 0);

        /// <summary>
        /// <see cref="RnetDeviceId"/> recommended by Russound for external control systems.
        /// </summary>
        public static readonly RnetDeviceId External = new RnetDeviceId(0, 0, RnetKeypadId.External);

        /// <summary>
        /// Returns <c>true</c> if the two device IDs are equal.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(RnetDeviceId x, RnetDeviceId y)
        {
            return object.Equals(x, y);
        }

        /// <summary>
        /// Returns <c>true</c> if the two device IDs are not equal.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(RnetDeviceId x, RnetDeviceId y)
        {
            return !object.Equals(x, y);
        }

        /// <summary>
        /// Reads a device ID from the reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static RnetDeviceId Read(RnetMessageBodyReader reader)
        {
            return new RnetDeviceId(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="zoneId"></param>
        /// <param name="keypadId"></param>
        public RnetDeviceId(RnetControllerId controllerId, RnetZoneId zoneId, RnetKeypadId keypadId)
            : this()
        {
            ControllerId = controllerId;
            ZoneId = zoneId;
            KeypadId = keypadId;
        }

        /// <summary>
        /// ID of the controller.
        /// </summary>
        public RnetControllerId ControllerId { get; private set; }

        /// <summary>
        /// ID of the zone.
        /// </summary>
        public RnetZoneId ZoneId { get; private set; }

        /// <summary>
        /// ID of the keypad.
        /// </summary>
        public RnetKeypadId KeypadId { get; private set; }

        /// <summary>
        /// Writes the device ID to the writer.
        /// </summary>
        /// <param name="writer"></param>
        public void Write(RnetStreamWriter writer)
        {
            writer.WriteByte(ControllerId);
            writer.WriteByte(ZoneId);
            writer.WriteByte(KeypadId);
        }

        /// <summary>
        /// Indicates whether this instance and the specified object are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is RnetDeviceId &&
                ((RnetDeviceId)obj).ControllerId == ControllerId &&
                ((RnetDeviceId)obj).ZoneId == ZoneId &&
                ((RnetDeviceId)obj).KeypadId == KeypadId;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return
                ControllerId.GetHashCode() ^
                ZoneId.GetHashCode() ^
                KeypadId.GetHashCode();
        }

        /// <summary>
        /// Writes a debug view of the current instance to the given writer.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteDebugView(TextWriter writer)
        {
            writer.WriteLine("{");
            using (var wrt = RnetUtil.CreateIndentedTextWriter(writer))
            {
                wrt.WriteLine("ControllerId = {0},", ControllerId);
                wrt.WriteLine("ZoneId = {0},", ZoneId);
                wrt.WriteLine("KeypadId = {0},", KeypadId);
            }
            writer.WriteLine("}");
        }

        /// <summary>
        /// Gets a string suitable for debugging this instance.
        /// </summary>
        public string DebugView
        {
            get
            {
                var b = new StringWriter();
                WriteDebugView(b);
                return b.ToString();
            }
        }

        public override string ToString()
        {
            return DebugView;
        }

        /// <summary>
        /// Compares this <see cref="RnetDeviceId"/> to another <see cref="RnetDeviceId"/>.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        int IComparable<RnetDeviceId>.CompareTo(RnetDeviceId other)
        {
            var a = Comparer<RnetControllerId>.Default.Compare(ControllerId, other.ControllerId);
            if (a != 0)
                return a;
            var b = Comparer<RnetZoneId>.Default.Compare(ZoneId, other.ZoneId);
            if (b != 0)
                return b;
            var c = Comparer<RnetKeypadId>.Default.Compare(KeypadId, other.KeypadId);
            if (c != 0)
                return c;

            return 0;
        }

        /// <summary>
        /// Compares the current object with another object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        int IComparable.CompareTo(object obj)
        {
            return ((IComparable<RnetDeviceId>)this).CompareTo((RnetDeviceId)obj);
        }

    }

}
