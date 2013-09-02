using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Rnet.Profiles.Metadata
{

    public sealed class CommandDescriptorCollection : IEnumerable<CommandDescriptor>
    {

        readonly Dictionary<string, CommandDescriptor> commands =
            new Dictionary<string, CommandDescriptor>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal CommandDescriptorCollection()
        {

        }

        /// <summary>
        /// Gets the <see cref="CommandDescriptor"/> for the given method.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public CommandDescriptor this[MethodInfo methodInfo]
        {
            get
            {
                Contract.Requires<ArgumentNullException>(methodInfo != null);
                return this[methodInfo.Name];
            }
        }

        /// <summary>
        /// Gets the <see cref="CommandDescriptor"/> for the given method.
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public CommandDescriptor this[string methodName]
        {
            get
            {
                Contract.Requires<ArgumentNullException>(methodName != null);
                CommandDescriptor command;
                return commands.TryGetValue(methodName, out command) ? command : null;
            }
        }

        /// <summary>
        /// Adds the descriptor.
        /// </summary>
        /// <param name="descriptor"></param>
        internal void Add(CommandDescriptor descriptor)
        {
            Contract.Requires<ArgumentNullException>(descriptor != null);
            commands[descriptor.MethodInfo.Name] = descriptor;
        }

        /// <summary>
        /// Removes the descriptor.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        internal bool Remove(CommandDescriptor descriptor)
        {
            Contract.Requires<ArgumentNullException>(descriptor != null);
            return commands.Remove(descriptor.MethodInfo.Name);
        }

        public IEnumerator<CommandDescriptor> GetEnumerator()
        {
            return commands.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
