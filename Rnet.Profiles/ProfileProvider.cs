using System.Collections.Generic;
using System.Linq;
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
        /// Gets a series of tasks that execute the evaluation for whether or not a certain <see cref="IProfile"/>
        /// implementation is compatible with the <param name="target"/>. If not supported, a task should simply return
        /// <c>null</c>.
        /// </summary>
        /// <remarks>
        /// Override this method in <see cref="ProfileProvider"/> implementations to return supported profiles for the
        /// target.
        /// </remarks>
        /// <returns></returns>
        public abstract Task<IEnumerable<IProfile>> GetProfilesAsync(RnetBusObject target);

    }

}
