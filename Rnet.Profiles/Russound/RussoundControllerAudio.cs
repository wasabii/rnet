using System.Threading.Tasks;

using Rnet.Profiles.Media;

namespace Rnet.Profiles.Russound
{

    class RussoundControllerAudio : ControllerProfileObject, IControllerAudio
    {

        RnetDataHandle powerHandle;
        Power power;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controller"></param>
        public RussoundControllerAudio(RnetController controller)
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

        /// <summary>
        /// Sends a local change in power to the controller.
        /// </summary>
        async void ChangePower()
        {
            await powerHandle.SendEvent(RnetEvent.AllZonesOnOff, (int)power);
        }

    }

}
