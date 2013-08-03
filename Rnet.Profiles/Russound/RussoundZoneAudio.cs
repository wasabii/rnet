using System.Threading.Tasks;
using Rnet.Profiles.Media;

namespace Rnet.Profiles.Russound
{

    class RussoundZoneAudio : ZoneProfileObject, IZoneAudio
    {

        RnetDeviceDirectory directory;
        Power power;
        ushort volume;
        ushort bass;
        ushort treble;
        Power loudness;
        ushort balance;
        PartyMode partyMode;
        DoNotDisturbMode doNotDisturbMode;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="zone"></param>
        public RussoundZoneAudio(RnetZone zone)
            : base(zone)
        {

        }

        public Power Power
        {
            get { return power; }
            set { power = value; RaisePropertyChanged("Power"); }
        }

        public ushort Volume
        {
            get { return volume; }
            set { volume = value; RaisePropertyChanged("Volume"); }
        }

        public ushort Bass
        {
            get { return bass; }
            set { bass = value; RaisePropertyChanged("Bass"); }
        }

        public ushort Treble
        {
            get { return treble; }
            set { treble = value; RaisePropertyChanged("Treble"); }
        }

        public Power Loudness
        {
            get { return loudness; }
            set { loudness = value; RaisePropertyChanged("Loudness"); }
        }

        public ushort Balance
        {
            get { return balance; }
            set { balance = value; RaisePropertyChanged("Balance"); }
        }

        public PartyMode PartyMode
        {
            get { return partyMode; }
            private set { partyMode = value; RaisePropertyChanged("PartyMode"); }
        }

        public DoNotDisturbMode DoNotDisturbMode
        {
            get { return doNotDisturbMode; }
            private set { doNotDisturbMode = value; RaisePropertyChanged("DoNotDisturbMode"); }
        }

        public async Task LoadAsync()
        {
            await directory.RequestAsync();
        }

        public Task SaveAsync()
        {
            return Task.FromResult(false);
        }

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            // subscribe to changes in the directory contents
            directory = await Zone.Controller.Directory.GetAsync(2, 0, Zone.Id, 7);
            directory.BufferChanged += directory_BufferChanged;
        }

        /// <summary>
        /// Invoked when the zone audio info data changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void directory_BufferChanged(object sender, ValueEventArgs<byte[]> args)
        {
            SetValues(args.Value);
        }

        /// <summary>
        /// Sets the values based on the data directory contents.
        /// </summary>
        /// <param name="buffer"></param>
        void SetValues(byte[] buffer)
        {
            Power = (Power)buffer[0];
            Volume = buffer[2];
            Bass = buffer[3];
            Treble = buffer[4];
            Loudness = (Power)buffer[5];
            Balance = buffer[6];
            PartyMode = (PartyMode)buffer[9];
            DoNotDisturbMode = (DoNotDisturbMode)buffer[10];
        }

    }

}
