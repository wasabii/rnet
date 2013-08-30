using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Rnet.Profiles;

namespace Rnet.Drivers
{

    /// <summary>
    /// Provides various utilities that assist in working with the driver model.
    /// </summary>
    public static class Util
    {

        /// <summary>
        /// Generates unique IDs.
        /// </summary>
        class IdGenerator
        {

            int next;

            /// <summary>
            /// Gets the next available unique ID.
            /// </summary>
            /// <returns></returns>
            public GeneratedId Next()
            {
                return new GeneratedId("unknown-" + next++);
            }

        }

        /// <summary>
        /// Stored auto-generated ID.
        /// </summary>
        class GeneratedId
        {

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="id"></param>
            public GeneratedId(string id)
            {
                Id = id;
            }

            /// <summary>
            /// Generated ID.
            /// </summary>
            public string Id { get; private set; }

        }

        /// <summary>
        /// Generates a new ID for the object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        static string GetGeneratedId(this RnetBusObject o)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            // cache generated ID on object, and cache ID generator on bus device.
            return
                o.Context.GetOrCreate<GeneratedId>(() =>
                    o.Bus.LocalDevice.Context.GetOrCreate<IdGenerator>(() =>
                        new IdGenerator()).Next()).Id;
        }

        /// <summary>
        /// Gets the ID of the given <see cref="RnetBusObject"/>, or generates one.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static async Task<string> GetId(this RnetBusObject o)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            var p = await o.GetProfile<IObject>();
            return p != null && !string.IsNullOrWhiteSpace(p.Id) ? p.Id : GetGeneratedId(o);
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
