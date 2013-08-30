using System.Linq;

using Microsoft.Practices.Prism.ViewModel;

using Rnet.Drivers;
using Rnet.Manager.Profiles;

namespace Rnet.Manager.Views
{

    /// <summary>
    /// View model that wraps any bus object and provides access to it's profiles.
    /// </summary>
    public class BusObjectViewModel : NotificationObject
    {

        TypeDictionary<Profile> profiles;
        TypeDictionary<ViewModel> profileViewModels;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        public BusObjectViewModel(RnetBusObject target)
        {
            Target = target;
            Initialize();
        }

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        async void Initialize()
        {
            var t = Target.GetProfiles();
            var p = await t;

            // load the supported profiles
            Profiles = new TypeDictionary<Profile>(p
                .ToDictionary(i => i.Metadata.Interface, i => i));
            ProfileViewModels = new TypeDictionary<ViewModel>(Profiles
                .Select(i => Rnet.Manager.Profiles.ViewModel.Create(i.Value))
                .Where(i => i != null)
                .ToDictionary(i => i.Profile.Metadata.Interface, i => i));
        }

        /// <summary>
        /// Object being viewed.
        /// </summary>
        public RnetBusObject Target { get; private set; }

        /// <summary>
        /// Known set of profile types and implementation.
        /// </summary>
        public TypeDictionary<Profile> Profiles
        {
            get { return profiles; }
            set { profiles = value; RaisePropertyChanged(() => Profiles); }
        }

        /// <summary>
        /// Known set of profile types and view models.
        /// </summary>
        public TypeDictionary<ViewModel> ProfileViewModels
        {
            get { return profileViewModels; }
            set { profileViewModels = value; RaisePropertyChanged(() => ProfileViewModels); }
        }

    }

}
