using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace Rnet
{

    /// <summary>
    /// Defines a RNet message.
    /// </summary>
    public abstract class RnetMessage
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected RnetMessage()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="targetDeviceId"></param>
        /// <param name="sourceDeviceId"></param>
        /// <param name="messageType"></param>
        protected RnetMessage(RnetDeviceId targetDeviceId, RnetDeviceId sourceDeviceId, RnetMessageType messageType)
            : this()
        {
            MessageTimestamp = DateTime.UtcNow;
            TargetDeviceId = targetDeviceId;
            SourceDeviceId = sourceDeviceId;
            MessageType = messageType;
        }

        /// <summary>
        /// Timestamp of message.
        /// </summary>
        public DateTime MessageTimestamp { get; private set; }

        /// <summary>
        /// Gets or sets the target Device ID.
        /// </summary>
        public RnetDeviceId TargetDeviceId { get; private set; }

        /// <summary>
        /// Gets or sets the source Device ID.
        /// </summary>
        public RnetDeviceId SourceDeviceId { get; private set; }

        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        public RnetMessageType MessageType { get; private set; }

        /// <summary>
        /// Writes the message using the given writer.
        /// </summary>
        /// <param name="writer"></param>
        internal void Write(RnetStreamWriter writer)
        {
            Contract.Requires<ArgumentNullException>(writer != null);

            writer.BeginMessage(TargetDeviceId, SourceDeviceId, MessageType);
            WriteBody(writer);
            writer.EndMessage();
        }

        /// <summary>
        /// Writes the body of the message.
        /// </summary>
        /// <param name="writer"></param>
        internal protected abstract void WriteBody(RnetStreamWriter writer);

        /// <summary>
        /// Writes out a debug view of the current instance.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteDebugView(TextWriter writer)
        {
            Contract.Requires<ArgumentNullException>(writer != null);

            writer.WriteLine("{");
            using (var wrt = RnetUtil.CreateIndentedTextWriter(writer))
            {
                wrt.WriteLine("TargetDeviceId = ");
                TargetDeviceId.WriteDebugView(wrt);
                wrt.WriteLine("SourceDeviceId = ");
                SourceDeviceId.WriteDebugView(wrt);
                wrt.WriteLine();

                WriteBodyDebugView(wrt);
            }
            writer.WriteLine("}");
        }

        protected virtual void WriteBodyDebugView(TextWriter writer)
        {
            Contract.Requires<ArgumentNullException>(writer != null);
        }

        /// <summary>
        /// Gets a string suitable for debugging the contents of this message.
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
