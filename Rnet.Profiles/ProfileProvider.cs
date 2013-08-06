using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rnet.Profiles
{

    /// <summary>
    /// Provides a set of profiles for a bus object.
    /// </summary>
    public abstract class ProfileProvider
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected ProfileProvider()
        {

        }

        /// <summary>
        /// Returns a series of <see cref="IProfile"/> instances that the <see cref="ProfileProvider"/> knows are
        /// supported against the given <see cref="RnetBusObject"/>.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public abstract Task<IEnumerable<IProfile>> GetProfiles(RnetBusObject target);

    }

}
