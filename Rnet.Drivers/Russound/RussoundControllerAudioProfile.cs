using System.Threading.Tasks;

using Rnet.Profiles.Media;

namespace Rnet.Profiles.Russound
{

    class RussoundControllerAudioProfile : ControllerProfileBase, IControllerAudio
    {

        RnetDataHandle powerHandle;
        Power power;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controller"></param>
        public RussoundControllerAudioProfile(RnetController controller)
            : base(controller)
        {
            powerHandle = controller[2, 0, 0, 7];
        }

        protected override async Task InitializeAsync()
        {
            // first zone provides a full system power variable
            await powerHandle.Subscribe(d =>
                ReceivePower(d[7]));
        }

        public Power Power
        {
            get { return power; }
            set { power = value; RaisePropertyChanged("Power"); ChangePower(); }
        }

        void ReceivePower(byte value)
        {
            power = (Power)value;
            RaisePropertyChanged("Power");
        }

        async void ChangePower()
        {
            await powerHandle.SendEvent(RnetEvent.AllZonesOnOff, (int)power);
        }

        public async void PowerToggle()
        {
            await powerHandle.SendEvent(RnetEvent.AllZonesOnOff, (int)(power == Power.On ? Power.Off : Power.On));
        }

        public async void PowerOn()
        {
            await powerHandle.SendEvent(RnetEvent.AllZonesOnOff, (int)Power.On);
        }

        public async void PowerOff()
        {
            await powerHandle.SendEvent(RnetEvent.AllZonesOnOff, (int)Power.Off);
        }

    }

}
