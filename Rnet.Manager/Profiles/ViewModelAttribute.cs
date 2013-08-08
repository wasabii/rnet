using System;

namespace Rnet.Manager.Profiles
{

    /// <summary>
    /// Associates a view model with a profile interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ViewModelAttribute : Attribute
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="interface"></param>
        public ViewModelAttribute(Type @interface)
        {
            Interface = @interface;
        }

        /// <summary>
        /// The profile interface to be associated with this view model.
        /// </summary>
        public Type Interface { get; private set; }

    }

}
