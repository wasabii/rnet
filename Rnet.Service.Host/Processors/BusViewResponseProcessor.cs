using System.Threading.Tasks;

namespace Rnet.Service.Host.Processors
{

    [ResponseProcessor(typeof(RnetBus))]
    public class BusViewResponseProcessor :
        IResponseProcessor
    {

        public Task<bool> Handle(IContext context, object target)
        {
            return Task.FromResult(false);
        }

    }

}
