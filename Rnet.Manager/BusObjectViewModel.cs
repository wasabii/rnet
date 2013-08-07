using System;

using Microsoft.Practices.Prism.ViewModel;

using Rnet.Drivers;
using Rnet.Profiles;

namespace Rnet.Manager
{

    /// <summary>
    /// Wraps a bus object.
    /// </summary>
    public class BusObjectViewModel : NotificationObject
    {

        Profile[] profiles;
        IObject objectProfile;
        string name;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="o"></param>
        public BusObjectViewModel(RnetBusObject o)
        {
            Object = o;

            // start async operations
            Initialize();
        }

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        async void Initialize()
        {
            // load the supported profiles
            Profiles = await Object.GetProfiles();

            // basic object profile provides a display name
            objectProfile = await Object.GetProfile<IObject>();
            if (objectProfile != null)
            {
                objectProfile.PropertyChanged += (s, a) => Name = objectProfile.DisplayName;
                Name = objectProfile.DisplayName;
            }
        }

        /// <summary>
        /// Object being viewed.
        /// </summary>
        public RnetBusObject Object { get; private set; }

        /// <summary>
        /// Known set of profile types and implementation.
        /// </summary>
        public Profile[] Profiles
        {
            get { return profiles; }
            set { profiles = value; RaisePropertyChanged(() => Profiles); }
        }

        /// <summary>
        /// Gets or sets the name associated with the object.
        /// </summary>
        public string Name
        {
            get { return name ?? "<unknown>"; }
            private set { name = value; RaisePropertyChanged(() => Name); }
        }

    }

}
