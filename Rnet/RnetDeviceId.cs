﻿
using System.IO;
namespace Rnet
{

    /// <summary>
    /// RNET DeviceID structure.
    /// </summary>
    public struct RnetDeviceId
    {

        /// <summary>
        /// <see cref="RnetDeviceId"/> which targets the controller.
        /// </summary>
        public static readonly RnetDeviceId RootController = new RnetDeviceId(0, 0, RnetKeypadId.Controller);

        /// <summary>
        /// <see cref="RnetDeviceId"/> recommended by Russound for external control systems.
        /// </summary>
        public static readonly RnetDeviceId ExternalSource = new RnetDeviceId(0, 0, 0x70);

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
        /// Reads a device ID from the reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static RnetDeviceId Read(RnetMessageBodyReader reader)
        {
            return new RnetDeviceId(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }

        /// <summary>
        /// Writes a debug view of the current instance to the given writer.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteDebugView(TextWriter writer)
        {
            writer.WriteLine("{");
            using (var wrt = RnetUtils.CreateIndentedTextWriter(writer))
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

    }

}
