using System.Threading.Tasks;

using Rnet.Profiles.Media;

namespace Rnet.Profiles.Russound
{

    class RussoundAudio : ControllerProfileObject, IControllerAudio
    {

        Power power;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controller"></param>
        public RussoundAudio(RnetController controller)
            : base(controller)
        {

        }

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            // subscribe to sys power status from first zone
            await Controller.Subscribe(new RnetPath(2, 0, 0, 7), d =>
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
            var d = await Controller.Root.GetAsync(2, 0);
            if (d != null)
                await d.RaiseEvent(RnetEvent.AllZonesOnOff, (ushort)power, 0);
        }

    }

}
