using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Practices.Prism.ViewModel;

using Rnet.Drivers;

namespace Rnet.Manager.Profiles
{

    /// <summary>
    /// Base view model for a profile.
    /// </summary>
    public abstract class ViewModel<T> : ViewModel
        where T : class
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected ViewModel(Profile<T> profile)
            : base(profile)
        {

        }

        /// <summary>
        /// Reference to the profile object being viewed.
        /// </summary>
        public new Profile<T> Profile
        {
            get { return (Profile<T>)base.Profile; }
        }

    }

    /// <summary>
    /// Base view model for a profile.
    /// </summary>
    public abstract class ViewModel : NotificationObject
    {

        /// <summary>
        /// Creates a view model capable of handling the given <see cref="Profile"/>.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static ViewModel Create(Profile profile)
        {
            var vm = typeof(ViewModel).Assembly.GetTypes()
                .Where(i => i.IsSubclassOf(typeof(ViewModel)))
                .Select(i => new { Attribute = i.GetCustomAttribute<ViewModelAttribute>(), Type = i })
                .Where(i => i.Attribute != null)
                .Where(i => i.Attribute.Interface == profile.Metadata.Contract)
                .Select(i => i.Type)
                .Select(i => Activator.CreateInstance(i, profile))
                .Cast<ViewModel>()
                .FirstOrDefault();

            if (vm != null)
                vm.Initialize();

            return vm;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected ViewModel(Profile profile)
        {
            Profile = profile;
        }

        /// <summary>
        /// Reference to the profile object being viewed.
        /// </summary>
        public Profile Profile { get; private set; }

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
