using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;

using Microsoft.Practices.Prism.ViewModel;

using Rnet.Drivers;
using Rnet.Manager.Views.Profiles;

namespace Rnet.Manager.Views
{

    /// <summary>
    /// View model that wraps any bus object and provides access to it's profiles.
    /// </summary>
    public class BusObjectViewModel : NotificationObject
    {

        readonly RnetBusObject target;
        readonly ProfileManager profileManager;

        TypeDictionary<ProfileHandle> profiles;
        TypeDictionary<ProfileViewModel> profileViewModels;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        [ImportingConstructor]
        public BusObjectViewModel(
            RnetBusObject target,
            ProfileManager profileManager)
        {
            Contract.Requires<ArgumentNullException>(target != null);
            Contract.Requires<ArgumentNullException>(profileManager != null);

            this.profileManager = profileManager;
            this.target = target;

            Initialize();
        }

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        async void Initialize()
        {
            var t = profileManager.GetProfiles(Target);
            var p = await t;

            // load the supported profiles
            Profiles = new TypeDictionary<ProfileHandle>(p
                .ToDictionary(i => i.Metadata.Contract, i => i));
            ProfileViewModels = new TypeDictionary<ProfileViewModel>(Profiles
                .Select(i => ProfileViewModel.Create(i.Value))
                .Where(i => i != null)
                .ToDictionary(i => i.Profile.Metadata.Contract, i => i));
        }

        /// <summary>
        /// Object being viewed.
        /// </summary>
        public RnetBusObject Target
        {
            get { return target; }
        }

        /// <summary>
        /// Known set of profile types and implementation.
        /// </summary>
        public TypeDictionary<ProfileHandle> Profiles
        {
            get { return profiles; }
            set { profiles = value; RaisePropertyChanged(() => Profiles); }
        }

        /// <summary>
        /// Known set of profile types and view models.
        /// </summary>
        public TypeDictionary<ProfileViewModel> ProfileViewModels
        {
            get { return profileViewModels; }
            set { profileViewModels = value; RaisePropertyChanged(() => ProfileViewModels); }
        }

    }

}
