using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rnet.Service.Objects
{

    [CollectionDataContract(Name = "Profiles", Namespace = "urn:Rnet.Objects")]
    class ProfileCollection : List<int>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ProfileCollection()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profiles"></param>
        public ProfileCollection(IEnumerable<int> profiles)
            : base(profiles)
        {

        }

    }

}
