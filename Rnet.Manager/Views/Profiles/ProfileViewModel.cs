using System.Threading.Tasks;

using Microsoft.Practices.Prism.ViewModel;

using Rnet.Drivers;

namespace Rnet.Manager.Views.Profiles
{

    /// <summary>
    /// Base view model for a profile.
    /// </summary>
    public abstract class ProfileViewModel<T> : ProfileViewModel
        where T : class
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected ProfileViewModel(ProfileHandle<T> profile)
            : base(profile)
        {

        }

        /// <summary>
        /// Reference to the profile object being viewed.
        /// </summary>
        public new ProfileHandle<T> Profile
        {
            get { return (ProfileHandle<T>)base.Profile; }
        }

    }

    /// <summary>
    /// Base view model for a profile.
    /// </summary>
    public abstract class ProfileViewModel : NotificationObject
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected ProfileViewModel(
            ProfileHandle profile)
        {
            Profile = profile;
        }

        /// <summary>
        /// Reference to the profile object being viewed.
        /// </summary>
        public ProfileHandle Profile { get; private set; }

        /// <summary>
        /// Provides a method that is invoked when the view model is created.
        /// </summary>
        /// <returns></returns>
        internal virtual Task Initialize()
        {
            return Task.FromResult(false);
        }

    }

}
