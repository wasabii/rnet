using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Describes a known controller on the RNET bus.
    /// </summary>
    public sealed class RnetController : RnetDevice
    {

        public override RnetDeviceId DeviceId
        {
            get { return new RnetDeviceId(Id, 0, RnetKeypadId.Controller); }
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id"></param>
        public RnetController(RnetBus bus, RnetControllerId id)
            : base(bus)
        {
            Id = id;
            Zones = new RnetZoneCollection(this);
        }

        /// <summary>
        /// Gets the controller ID.
        /// </summary>
        public RnetControllerId Id { get; private set; }

        /// <summary>
        /// Zones available underneath this controller.
        /// </summary>
        public RnetZoneCollection Zones { get; private set; }

        /// <summary>
        /// Requests a zone from the controller.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal async Task<RnetZone> GetZone(RnetZoneId id, CancellationToken cancellationToken)
        {
            // TODO replace with actual request to gather basic zone existence data
            Bus.SynchronizationContext.Post(async state =>
            {
                await Zones.AddAsync(new RnetZone(this, id));
            }, null);

            return await Zones.WaitAsync(id, cancellationToken);
        }

        public override string ToString()
        {
            return string.Format("Controller {0}", Id);
        }

    }

}
