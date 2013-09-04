using System;
using System.Diagnostics.Contracts;

using Nancy;

namespace Rnet.Service
{

    /// <summary>
    /// Serves as the base class for services.
    /// </summary>
    public abstract class BusModule : NancyModule
    {

        RnetBus bus;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        protected BusModule(RnetBus bus, string modulePath)
            : base(modulePath)
        {
            Contract.Requires(bus != null);

            this.bus = bus;
        }

        /// <summary>
        /// Gets the <see cref="RnetBus"/>.
        /// </summary>
        protected RnetBus Bus
        {
            get { return bus; }
        }

    }

}
