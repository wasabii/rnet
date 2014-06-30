using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Rnet.Drivers.Default;
using Rnet.Profiles.Core;
using Rnet.Profiles.Media.Audio;

namespace Rnet.Drivers.Russound.Media.Audio
{

    /// <summary>
    /// Implements <see cref="IZone"/> for a zone on a Russound controller.
    /// </summary>
    class Zone :
        ZoneBase,
        Rnet.Profiles.Media.IZone,
        Rnet.Profiles.Media.Audio.IEqualization
    {

        readonly RnetDataHandle runHandle;
        readonly RnetDataHandle zoneHandle;
        readonly RnetDataHandle powerHandle;
        readonly RnetDataHandle volumeHandle;
        readonly RnetDataHandle bassHandle;
        readonly RnetDataHandle trebleHandle;
        readonly RnetDataHandle loudnessHandle;
        readonly RnetDataHandle balanceHandle;
        readonly RnetDataHandle sourceHandle;

        Power power;
        int volume;
        int bass;
        int treble;
        Loudness loudness;
        int balance;
        int source;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="zone"></param>
        public Zone(RnetZone zone)
            : base(zone)
        {
            runHandle =
                Zone.Controller[2, 0];
            zoneHandle =
                Zone.Controller[2, 0, Zone.Id, 7];
            powerHandle =
                Zone.Controller[2, 0, Zone.Id, 6];
            volumeHandle =
                Zone.Controller[2, 0, Zone.Id, 1];
            bassHandle =
                Zone.Controller[2, 0, Zone.Id, 0, 0];
            trebleHandle =
                Zone.Controller[2, 0, Zone.Id, 0, 1];
            loudnessHandle =
                Zone.Controller[2, 0, Zone.Id, 0, 2];
            balanceHandle =
                Zone.Controller[2, 0, Zone.Id, 0, 3];
            sourceHandle =
                Zone.Controller[2, 0, Zone.Id, 2];
        }

        protected override async Task Initialize()
        {
            await base.Initialize();

            zoneHandle.Subscribe(d => ReceiveZone(d));
            powerHandle.Subscribe(d => ReceivePower(d[0]));
            volumeHandle.Subscribe(d => ReceiveVolume(d[0]));
            bassHandle.Subscribe(d => ReceiveBass(d[0]));
            trebleHandle.Subscribe(d => ReceiveTreble(d[0]));
            loudnessHandle.Subscribe(d => ReceiveLoudness(d[0]));
            balanceHandle.Subscribe(d => ReceiveBalance(d[0]));
            sourceHandle.Subscribe(d => ReceiveSource(d[0]));
        }

        /// <summary>
        /// Receives all the zone data.
        /// </summary>
        /// <param name="d"></param>
        void ReceiveZone(byte[] d)
        {
            Contract.Requires<ArgumentNullException>(d != null);
            Contract.Requires<ArgumentOutOfRangeException>(d.Length > 6);

            ReceivePower(d[0]);
            ReceiveVolume(d[2]);
            ReceiveBass(d[3]);
            ReceiveTreble(d[4]);
            ReceiveLoudness(d[5]);
            ReceiveBalance(d[6]);
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

        public async void ChangePower()
        {
            await Zone.Controller.SendEvent("2.0", RnetEvent.ZoneOnOff, (ushort)power, Zone.Id);
            await powerHandle.Refresh();
        }

        public async void PowerToggle()
        {
            await Zone.Controller.SendEvent("2.0", RnetEvent.ZoneOnOff, (ushort)(power == Power.On ? Power.Off : Power.On), Zone.Id);
            await powerHandle.Refresh();
        }

        public async void PowerOn()
        {
            await Zone.Controller.SendEvent("2.0", RnetEvent.ZoneOnOff, (ushort)Power.On, Zone.Id);
            await powerHandle.Refresh();
        }

        public async void PowerOff()
        {
            await Zone.Controller.SendEvent("2.0", RnetEvent.ZoneOnOff, (ushort)Power.Off, Zone.Id);
            await powerHandle.Refresh();
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

        async void ChangeVolume()
        {
            await runHandle.SendEvent(RnetEvent.SetZoneVolume, volume / 2, Zone.Id);
            await volumeHandle.Refresh();
        }

        public async void VolumeUp()
        {
            await volumeHandle.SendEvent(RnetEvent.Plus);
        }

        public async void VolumeDown()
        {
            await volumeHandle.SendEvent(RnetEvent.Minus);
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
            await bassHandle.Write((byte)(bass + 10));
        }

        public async void BassUp()
        {
            await bassHandle.SendEvent(RnetEvent.Plus);
        }

        public async void BassDown()
        {
            await bassHandle.SendEvent(RnetEvent.Minus);
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
            await trebleHandle.Write((byte)(treble + 10));
        }

        public async void TrebleUp()
        {
            await trebleHandle.SendEvent(RnetEvent.Plus);
        }

        public async void TrebleDown()
        {
            await trebleHandle.SendEvent(RnetEvent.Minus);
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
            await loudnessHandle.Write((byte)loudness);
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
            await balanceHandle.Write((byte)(balance + 10));
        }

        public async void BalanceLeft()
        {
            await balanceHandle.SendEvent(RnetEvent.Minus);
        }

        public async void BalanceRight()
        {
            await balanceHandle.SendEvent(RnetEvent.Plus);
        }

        public int Source
        {
            get { return source; }
            set { source = CoerceSource(value); RaisePropertyChanged("Source"); ChangeSource(); }
        }

        int CoerceSource(int value)
        {
            if (value < 0)
                return 0;
            else if (value > 32)
                return 32;
            else
                return value;
        }

        void ReceiveSource(byte value)
        {
            balance = value;
            RaisePropertyChanged("Source");
        }

        async void ChangeSource()
        {
            await sourceHandle.Write((byte)source);
        }

    }

}
