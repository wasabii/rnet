using Microsoft.Practices.Prism.Commands;
using Rnet.Drivers;
using Rnet.Profiles.Media;

namespace Rnet.Manager.Profiles.Media
{

    [ViewModel(typeof(IAudio))]
    public class AudioViewModel : ViewModel<IAudio>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        public AudioViewModel(Profile<IAudio> profile)
            : base(profile)
        {
            PowerOnCommand = new DelegateCommand(() => Profile.Instance.PowerOn());
            PowerOffCommand = new DelegateCommand(() => Profile.Instance.PowerOff());
            VolumeUpCommand = new DelegateCommand(() => Profile.Instance.VolumeUp());
            VolumeDownCommand = new DelegateCommand(() => Profile.Instance.VolumeDown());
            BassUpCommand = new DelegateCommand(() => Profile.Instance.BassUp());
            BassDownCommand = new DelegateCommand(() => Profile.Instance.BassDown());
            TrebleUpCommand = new DelegateCommand(() => Profile.Instance.TrebleUp());
            TrebleDownCommand = new DelegateCommand(() => Profile.Instance.TrebleDown());
        }

        public DelegateCommand PowerOnCommand { get; private set; }

        public DelegateCommand PowerOffCommand { get; private set; }

        public DelegateCommand VolumeUpCommand { get; private set; }

        public DelegateCommand VolumeDownCommand { get; private set; }

        public DelegateCommand BassUpCommand { get; private set; }

        public DelegateCommand BassDownCommand { get; private set; }

        public DelegateCommand TrebleUpCommand { get; private set; }

        public DelegateCommand TrebleDownCommand { get; private set; }

    }

}
