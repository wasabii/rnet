using System.ComponentModel;
using Microsoft.Practices.Prism.ViewModel;
using Rnet.Profiles.Media;

namespace Rnet.Manager.Profiles
{

    public class ZoneAudioViewModel : NotificationObject
    {

        int volume;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="zoneAudio"></param>
        public ZoneAudioViewModel(IAudio zoneAudio)
        {
            ZoneAudio = zoneAudio;
            ZoneAudio.PropertyChanged += ZoneAudio_PropertyChanged;
        }

        void ZoneAudio_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            //switch (args.PropertyName)
            //{
            //    case ""
            //}
        }

        /// <summary>
        /// Reference to profile.
        /// </summary>
        public IAudio ZoneAudio { get; private set; }

        public int Volume
        {
            get { return volume; }
            set { volume = value; }
        }

    }

}
