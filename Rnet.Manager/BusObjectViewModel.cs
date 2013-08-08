using System.Linq;

using Microsoft.Practices.Prism.ViewModel;

using Rnet.Drivers;

namespace Rnet.Manager
{

    /// <summary>
    /// Wraps a bus object.
    /// </summary>
    public class BusObjectViewModel : NotificationObject
    {

        Profiles.ViewModel[] profiles;

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
            // load the supported profiles
            Profiles = (await Target.GetProfiles())
                .Select(i => Rnet.Manager.Profiles.ViewModel.Create(i))
                .Where(i => i != null)
                .ToArray();
        }

        /// <summary>
        /// Object being viewed.
        /// </summary>
        public RnetBusObject Target { get; private set; }

        /// <summary>
        /// Known set of profile types and implementation.
        /// </summary>
        public Profiles.ViewModel[] Profiles
        {
            get { return profiles; }
            set { profiles = value; RaisePropertyChanged(() => Profiles); }
        }

    }

}
