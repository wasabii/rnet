using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Rnet.Service.Processors;

namespace Rnet.Service.Objects
{

    [RequestProcessor(typeof(RnetBus))]
    public class BusRequestProcessor : RequestProcessor<RnetBus>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        [ImportingConstructor]
        protected BusRequestProcessor(
            ObjectModule module)
            : base(module)
        {

        }

        /// <summary>
        /// Gets the target bus.
        /// </summary>
        public RnetBus Bus
        {
            get { return base.Object; }
        }

        public override async Task<object> Get()
        {
            // all devices available on the bus
            var l = await Observable.Empty<RnetBusObject>()
                .Concat(Bus.Controllers.ToObservable())
                .Concat(Bus.Controllers.ToObservable().SelectMany(i => i.Zones).SelectMany(i => i.Devices))
                .OfType<RnetDevice>()
                .SelectAsync(i => Module.DeviceToData(i), true)
                .ToList();

            var devices = l
                .OfType<DeviceData>();

            var objects = l
                .OfType<ControllerData>();

            return new BusData()
            {
                Devices = new DeviceDataCollection(devices),
                Objects = new ObjectDataCollection(objects),
            };
        }

    }

}
