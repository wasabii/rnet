using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using OLinq;

using Rnet.Drivers;
using Rnet.Manager.Views;
using Rnet.Profiles;

namespace Rnet.Manager.Profiles
{

    [ViewModel(typeof(IContainer))]
    public class ContainerViewModel : ViewModel<IContainer>
    {

        IEnumerable<RnetBusObject> objects;
        IEnumerable<BusObjectViewModel> objectViewModels;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        public ContainerViewModel(Profile<IContainer> profile)
            : base(profile)
        {

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
