using System;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Rnet.Profiles;
using Rnet.Profiles.Core;

namespace Rnet.Drivers
{

    /// <summary>
    /// Provides various utilities that assist in working with the driver model.
    /// </summary>
    public static class Util
    {

        /// <summary>
        /// Generates unique names.
        /// </summary>
        class NameGenerator
        {

            int next;

            /// <summary>
            /// Gets the next available unique name.
            /// </summary>
            /// <returns></returns>
            public GeneratedName Next()
            {
                return new GeneratedName("unknown-" + next++);
            }

        }

        /// <summary>
        /// Stored auto-generated ID.
        /// </summary>
        class GeneratedName
        {

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="name"></param>
            public GeneratedName(string name)
            {
                Name = name;
            }

            /// <summary>
            /// Generated ID.
            /// </summary>
            public string Name { get; private set; }

        }

        /// <summary>
        /// Generates a new ID for the object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        static string GetGeneratedName(this RnetBusObject o)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            // cache generated ID on object, and cache ID generator on bus device.
            return
                o.Context.GetOrCreate<GeneratedName>(() =>
                    o.Bus.LocalDevice.Context.GetOrCreate<NameGenerator>(() =>
                        new NameGenerator()).Next()).Name;
        }

        /// <summary>
        /// Gets the name of the given <see cref="RnetBusObject"/> or generates one.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static async Task<string> GetId(this RnetBusObject o)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            var p = await o.GetProfile<IObject>();
            return p != null && !string.IsNullOrWhiteSpace(p.Id) ? p.Id : GetGeneratedName(o);
        }

        /// <summary>
        /// Projects each element into an ASCII string.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<string> ToAscii(this IObservable<byte[]> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            return source
                .Select(d => Rnet.RnetDataUtil.GetAsciiString(d));
        }

        /// <summary>
        /// Gets the container context.
        /// </summary>
        /// <param name="target"></param>
        static IContainerContext GetContainerContext(this RnetBusObject target)
        {
            Contract.Requires<ArgumentNullException>(target != null);
            Contract.Requires<ArgumentNullException>(target.Context != null);

            return target.Context.GetOrCreate<IContainerContext>(() => new ContainerContext(null, null));
        }

        /// <summary>
        /// Gets the container of the specified object.
        /// </summary>
        /// <param name="target"></param>
        public static RnetBusObject GetContainer(this RnetBusObject target)
        {
            Contract.Requires<ArgumentNullException>(target != null);

            return GetContainerContext(target).Container;
        }

        /// <summary>
        /// Gets the owner of the specified object.
        /// </summary>
        /// <param name="target"></param>
        public static RnetBusObject GetOwner(this RnetBusObject target)
        {
            Contract.Requires<ArgumentNullException>(target != null);

            return GetContainerContext(target).Owner;
        }

        /// <summary>
        /// Sets the container context values.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="owner"></param>
        /// <param name="container"></param>
        public static void SetContainerContext(this RnetBusObject target, RnetBusObject owner, RnetBusObject container)
        {
            Contract.Requires<ArgumentNullException>(target != null);
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(owner != null);

            var context = target.Context.Get<IContainerContext>();
            if (context == null ||
                context.Owner != owner ||
                context.Container != container)
                target.Context.Set<IContainerContext>(new ContainerContext(owner, container));
        }

    }

}
