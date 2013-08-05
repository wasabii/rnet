using System;
using System.Collections.Generic;
using System.Reactive.Linq;

using Microsoft.Practices.Prism.ViewModel;

using Rnet.Profiles;

namespace Rnet.Manager
{

    /// <summary>
    /// Provides a view for a single object on the bus.
    /// </summary>
    public class BusObjectViewModel : NotificationObject
    {

        IDictionary<Type, IProfile> profiles;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="o"></param>
        public BusObjectViewModel(RnetBusObject o)
        {
            Object = o;
            Initialize();
        }

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        async void Initialize()
        {
            // load the supported profiles
            Profiles = await Object.GetProfilesAsync()
                .ToDictionary(i => i.Key, i => i.Value);
        }

        /// <summary>
        /// Object being viewed.
        /// </summary>
        public RnetBusObject Object { get; private set; }

        /// <summary>
        /// Known set of profile types and implementation.
        /// </summary>
        public IDictionary<Type, IProfile> Profiles
        {
            get { return profiles; }
            set { profiles = value; RaisePropertyChanged(() => Profiles); }
        }

    }

}
