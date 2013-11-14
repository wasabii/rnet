using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using OLinq;
using Rnet.Drivers;
using Rnet.Manager.Views;
using Rnet.Profiles.Core;

namespace Rnet.Manager.Views.Profiles.Core
{

    [ProfileViewModel(typeof(IContainer))]
    public class ContainerViewModel : ProfileViewModel<IContainer>
    {

        readonly ProfileManager profileManager;
        IEnumerable<RnetBusObject> objects;
        IEnumerable<BusObjectViewModel> objectViewModels;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        public ContainerViewModel(
            ProfileManager profileManager,
            ProfileHandle<IContainer> profile)
            : base(profile)
        {
            Contract.Requires<ArgumentNullException>(profileManager != null);
            Contract.Requires<ArgumentNullException>(profile != null);

            this.profileManager = profileManager;
        }

        internal override Task Initialize()
        {
            Objects = Profile.Instance;
            ObjectViewModels = Objects.AsObservableQuery()
                .Select(i => new BusObjectViewModel(i))
                .AsObservableQuery()
                .ToObservableView();

            return base.Initialize();
        }

        public IEnumerable<RnetBusObject> Objects
        {
            get { return objects; }
            set { objects = value; RaisePropertyChanged(() => Objects); }
        }

        public IEnumerable<BusObjectViewModel> ObjectViewModels
        {
            get { return objectViewModels; }
            set { objectViewModels = value; RaisePropertyChanged(() => ObjectViewModels); }
        }

    }

}
