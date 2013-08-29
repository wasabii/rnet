using Rnet.Profiles;

namespace Rnet.Drivers
{

    /// <summary>
    /// Required base class of logical bus objects. A <see cref="LogicalBusObject"/> represents an object that appears
    /// to be located on the RNET bus underneath another object, but which in reality has no physical representation
    /// as an addressable device. <see cref="LogicalBusObject"/>s can be present at multiple positions in the logical
    /// object tree. They can thus be returned from multiple <see cref="IContainer"/>s. A <see 
    /// cref="LogicalBusObject"/> requires an existing <see cref="RnetDevice"/> which provides the <see
    /// cref="IBuslObjectOwner"/> profile. This profile is required for consumers of a <see cref="LogicalBusObject"/>
    /// to be able to query for further profiles implemented by the <see cref="LogicalBusObject"/> itself.
    /// </summary>
    public abstract class LogicalBusObject : RnetBusObject
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="owner"></param>
        protected LogicalBusObject(RnetDevice owner)
            : base(owner.Bus)
        {
            this.SetContainerContext(owner, owner);
        }

    }

}
