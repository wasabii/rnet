using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Practices.Prism.ViewModel;
using Rnet.Profiles;

namespace Rnet.Monitor.Wpf
{

    public class BusObjectViewModel : NotificationObject
    {

        Dictionary<Type, IProfile> profiles;

        public BusObjectViewModel(RnetBusObject o)
        {
            Object = o;
            Initialize();
        }

        async void Initialize()
        {
            Profiles = (await Object.GetProfilesAsync().ToList()).ToDictionary(i => i.Key, i => i.Value);
        }

        public RnetBusObject Object { get; private set; }

        public Dictionary<Type, IProfile> Profiles
        {
            get { return profiles; }
            set { profiles = value; RaisePropertyChanged(() => Profiles); }
        }

    }

}
