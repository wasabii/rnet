using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Rnet.Profiles.Media;

namespace Rnet.Profiles.Russound
{

    class RussoundZoneAudio : ZoneProfileObject, IZoneAudio
    {

        RnetDirectoryWatcher watcher;
        Power power;
        int volume;
        int bass;
        int treble;
        Power loudness;
        int balance;
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

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            // set up data path watches
            watcher = Zone.Controller.Directory
                .Watch(w => w
                    .When(2, 0, Zone.Id, a => a

                        .When(0, b => b

                            .When(0, c => c
                                .Then(x =>
                                    Bass = x[0] - 10))

                            .When(1, c => c
                                .Then(x =>
                                    Treble = x[0] - 10))

                            .When(2, c => c
                                .Then(x =>
                                    Loudness = (Power)x[0]))

                            .When(3, c => c
                                .Then(x =>
                                    Balance = x[0] - 10))

                            .When(4, c => c
                                .Then(x =>
                                    Volume = x[0] * 2))

                            .When(6, c => c
                                .Then(x =>
                                    DoNotDisturbMode = (DoNotDisturbMode)x[0]))

                            .When(7, c => c
                                .Then(x =>
                                    PartyMode = (PartyMode)x[0])))

                        .When(7, b => b
                            .Then(x =>
                            {
                                Power = (Power)x[0];
                                Volume = x[2] * 2;
                                Bass = x[3] - 10;
                                Treble = x[4] - 10;
                                Loudness = (Power)x[5];
                                Balance = x[6] - 10;
                                PartyMode = (PartyMode)x[9];
                                DoNotDisturbMode = (DoNotDisturbMode)x[10];
                            }))));

            await LoadAsync();
        }

        public Task LoadAsync()
        {
            return watcher.LoadAsync();
        }

        public Task SaveAsync()
        {
            return Task.FromResult(false);
        }

        public Power Power
        {
            get { return power; }
            set { power = value; RaisePropertyChanged("Power"); }
        }

        public int Volume
        {
            get { return volume; }
            set { volume = value; RaisePropertyChanged("Volume"); }
        }

        /// <summary>
        /// 0-20 = -10:+10
        /// </summary>
        public int Bass
        {
            get { return bass; }
            set { bass = value; RaisePropertyChanged("Bass"); }
        }

        public int Treble
        {
            get { return treble; }
            set { treble = value; RaisePropertyChanged("Treble"); }
        }

        public Power Loudness
        {
            get { return loudness; }
            set { loudness = value; RaisePropertyChanged("Loudness"); }
        }

        public int Balance
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

        public async Task SetVolume(int value)
        {
            var dir = await Zone.Controller.Directory.GetAsync(2, 0, 0, 0, 4);
            if (dir != null)
                await dir.WriteAsync(new[] { (byte)(value / 2) });
        }

    }

}
