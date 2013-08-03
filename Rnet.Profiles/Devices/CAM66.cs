using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rnet.Profiles.Devices
{

    [ProfileProvider]
    public class CAM66 : ProfileProvider
    {

        protected internal override IEnumerable<Task<IProfile>> GetProfilesAsync(RnetBusObject target)
        {
            yield break;
        }

    }

}
