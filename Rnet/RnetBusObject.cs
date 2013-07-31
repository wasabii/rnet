namespace Rnet
{

    /// <summary>
    /// Base class for objects discovered on the RNET bus.
    /// </summary>
    public abstract class RnetBusObject : RnetModelObject
    {

        /// <summary>
        /// Gets a string to describe the object.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name ?? base.ToString();
        }

    }

}
