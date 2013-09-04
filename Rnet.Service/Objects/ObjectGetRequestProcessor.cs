using System.Threading.Tasks;

using Nancy;

using Rnet.Drivers;
using Rnet.Service.Processors;

namespace Rnet.Service.Objects
{

    [RequestProcessor]
    public class ObjectGetRequestProcessor : GenericRequestProcessor<RnetBusObject>
    {

        public override Task<bool> CanProcess(NancyContext context, string method, string[] uri, RnetBusObject target)
        {
            return Task.FromResult(method == "GET");
        }

        public override async Task<object> Process(NancyContext context, string method, string[] uri, RnetBusObject target)
        {
            return new ObjectData()
            {
                Id = await target.GetId(),
                Name = await target.GetObjectName(context),
                Objects = await target.GetObjectRefs(context),
                Profiles = await target.GetProfileRefs(context),
            };
        }

    }

}
