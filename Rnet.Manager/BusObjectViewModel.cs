using System;
using System.Collections.Generic;
using System.Reactive.Linq;

using Microsoft.Practices.Prism.ViewModel;

using Rnet.Profiles;
using Rnet.Profiles.Basic;

namespace Rnet.Manager
{

    /// <summary>
    /// Wraps a bus object.
    /// </summary>
    public class BusObjectViewModel : NotificationObject
    {

        IDictionary<Type, Driver> profiles;
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
            Profiles = await Object.GetProfiles()
                .ToDictionary(i => i.Key, i => i.Value);

            // basic object profile provides a display name
            objectProfile = await Object.GetProfile<IObject>();
            if (objectProfile != null)
            {
                objectProfile.PropertyChanged += (s, a) => Name = objectProfile.Name;
                Name = objectProfile.Name;
            }
        }

        /// <summary>
        /// Object being viewed.
        /// </summary>
        public RnetBusObject Object { get; private set; }

        /// <summary>
        /// Known set of profile types and implementation.
        /// </summary>
        public IDictionary<Type, Driver> Profiles
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
