using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rnet.Service.Objects
{

    [CollectionDataContract(Name = "Objects", Namespace = "urn:Rnet.Objects", ItemName = "ObjectRef")]
    class ObjectRefCollection : List<ObjectRef>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ObjectRefCollection()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="refs"></param>
        public ObjectRefCollection(IEnumerable<ObjectRef> refs)
            : base(refs)
        {

        }

    }

}
