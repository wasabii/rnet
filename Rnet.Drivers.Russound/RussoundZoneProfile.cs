using System.ComponentModel;
using Rnet.Profiles.Basic;

namespace Rnet.Profiles.Russound
{

    class RussoundZoneProfile : ZoneProfileBase, IRussoundZone, IZone, IObject
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="zone"></param>
        public RussoundZoneProfile(RnetZone zone)
            : base(zone)
        {

        }

        public string Name
        {
            get { return "Zone " + (Zone.Id + 1); }
            set { }
        }

        string IObject.Name
        {
            get { return Name; }
        }

    }

}
