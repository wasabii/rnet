using System.Threading.Tasks;

using Rnet.Profiles.Media;

namespace Rnet.Profiles.Russound
{

    class RussoundZoneAudio : ZoneProfileObject, IZoneAudio
    {

        RnetDevicePathNode runNode;
        RnetDevicePathNode zoneNode;

        RnetDevicePathNode powerNode;
        Power power;

        RnetDevicePathNode volumeNode;
        int volume;

        RnetDevicePathNode bassNode;
        int bass;

        RnetDevicePathNode trebleNode;
        int treble;

        RnetDevicePathNode loudnessNode;
        Loudness loudness;

        RnetDevicePathNode balanceNode;
        int balance;

        RnetDevicePathNode partyModeNode;
        PartyMode partyMode;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="zone"></param>
        public RussoundZoneAudio(RnetZone zone)
            : base(zone)
        {

        }

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            runNode = await Zone.Controller.Root.GetAsync(
                new RnetPath(2, 0));

            zoneNode = await Zone.Controller.Subscribe(
                new RnetPath(2, 0, Zone.Id, 7), d =>
                    ReceiveZone(d));

            powerNode = await Zone.Controller.Subscribe(
                new RnetPath(2, 0, Zone.Id, 6), d =>
                    ReceivePower(d[0]));

            volumeNode = await Zone.Controller.Subscribe(
                new RnetPath(2, 0, Zone.Id, 1), d =>
                    ReceiveVolume(d[0]));

            bassNode = await Zone.Controller.Subscribe(
                new RnetPath(2, 0, Zone.Id, 0, 0), d =>
                    ReceiveBass(d[0]));

            trebleNode = await Zone.Controller.Subscribe(
                new RnetPath(2, 0, Zone.Id, 0, 1), d =>
                    ReceiveTreble(d[0]));

            loudnessNode = await Zone.Controller.Subscribe(
                new RnetPath(2, 0, Zone.Id, 0, 2), d =>
                    ReceiveLoudness(d[0]));

            balanceNode = await Zone.Controller.Subscribe(
                new RnetPath(2, 0, Zone.Id, 0, 3), d =>
                    ReceiveBalance(d[0]));
        }

        /// <summary>
        /// Receives all the zone data.
        /// </summary>
        /// <param name="d"></param>
        void ReceiveZone(byte[] d)
        {
            ReceivePower(d[0]);
            ReceiveVolume(d[2]);
            ReceiveBass(d[3]);
            ReceiveTreble(d[4]);
            ReceiveLoudness(d[5]);
            ReceiveBalance(d[6]);
            ReceivePartyMode(d[9]);
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
            await powerNode.RaiseEvent(RnetEvent.ZoneOnOff, (ushort)power, Zone.Id);
        }

        public int Volume
        {
            get { return volume; }
            set { volume = value; RaisePropertyChanged("Volume"); ChangeVolume(); }
        }

        void ReceiveVolume(byte value)
        {
            volume = value * 2;
            RaisePropertyChanged("Volume");
        }

        /// <summary>
        /// Sends a local change in volume to the controller.
        /// </summary>
        async void ChangeVolume()
        {
            await runNode.RaiseEvent(RnetEvent.SetZoneVolume, (ushort)(volume / 2), Zone.Id);
            await volumeNode.RequestBufferAsync();
        }

        public async void VolumeUp()
        {
            await volumeNode.RaiseEvent(RnetEvent.Plus);
        }

        public async void VolumeDown()
        {
            await volumeNode.RaiseEvent(RnetEvent.Minus);
        }

        public int Bass
        {
            get { return bass; }
            set { bass = value; RaisePropertyChanged("Bass"); ChangeBass(); }
        }

        void ReceiveBass(byte value)
        {
            bass = value - 10;
            RaisePropertyChanged("Bass");
        }

        async void ChangeBass()
        {
            await bassNode.WriteAsync((byte)(bass + 10));
        }

        public async void BassUp()
        {
            await bassNode.RaiseEvent(RnetEvent.Plus);
        }

        public async void BassDown()
        {
            await bassNode.RaiseEvent(RnetEvent.Minus);
        }

        public int Treble
        {
            get { return treble; }
            set { treble = value; RaisePropertyChanged("Treble"); ChangeTreble(); }
        }

        void ReceiveTreble(byte value)
        {
            treble = value - 10;
            RaisePropertyChanged("Treble");
        }

        async void ChangeTreble()
        {
            await trebleNode.WriteAsync((byte)(treble + 10));
        }

        public async void TrebleUp()
        {
            await trebleNode.RaiseEvent(RnetEvent.Plus);
        }

        public async void TrebleDown()
        {
            await trebleNode.RaiseEvent(RnetEvent.Minus);
        }

        public Loudness Loudness
        {
            get { return loudness; }
            set { loudness = value; RaisePropertyChanged("Loudness"); ChangeLoudness(); }
        }

        void ReceiveLoudness(byte value)
        {
            loudness = (Loudness)value;
            RaisePropertyChanged("Loudness");
        }

        async void ChangeLoudness()
        {
            await loudnessNode.WriteAsync((byte)loudness);
        }

        public int Balance
        {
            get { return balance; }
            set { balance = value; RaisePropertyChanged("Balance"); ChangeBalance(); }
        }

        void ReceiveBalance(byte value)
        {
            balance = value - 10;
            RaisePropertyChanged("Balance");
        }

        async void ChangeBalance()
        {
            await balanceNode.WriteAsync((byte)(balance + 10));
        }

        public async void BalanceLeft()
        {
            await balanceNode.RaiseEvent(RnetEvent.Minus);
        }

        public async void BalanceRight()
        {
            await balanceNode.RaiseEvent(RnetEvent.Plus);
        }

        public PartyMode PartyMode
        {
            get { return partyMode; }
            set { partyMode = value; RaisePropertyChanged("PartyMode"); ChangePartyMode(); }
        }

        void ReceivePartyMode(byte value)
        {
            partyMode = (PartyMode)value;
            RaisePropertyChanged("PartyMode");
        }

        async void ChangePartyMode()
        {
            await partyModeNode.WriteAsync((byte)partyMode);
        }

    }

}
