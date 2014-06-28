using System;
using System.Diagnostics.Contracts;

using Rnet.Profiles.Core;

namespace Rnet.Drivers.Default
{

    /// <summary>
    /// Provides a default implementation of <see cref="IZone"/> for a zone.
    /// </summary>
    public class Zone :
        ZoneBase,
        IZone
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="zone"></param>
        protected internal Zone(RnetZone zone)
            : base(zone)
        {
            Contract.Requires<ArgumentNullException>(zone != null);
        }

        public string Id
        {
            get { return "zone-" + (Zone.Id + 1); }
        }

        public string DisplayName
        {
            get { return "Zone " + (Zone.Id + 1); }
        }

    }

}
