using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rnet.Service.Objects
{

    [CollectionDataContract(Name = "Profiles", Namespace = "urn:rnet:objects", ItemName = "ProfileRef")]
    class ProfileRefCollection : List<ProfileRef>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ProfileRefCollection()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="refs"></param>
        public ProfileRefCollection(IEnumerable<ProfileRef> refs)
            : base(refs)
        {

        }

    }

}
