using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Rnet.Drivers.Default;
using Rnet.Profiles.Core;
using Rnet.Profiles.Media.Audio;

namespace Rnet.Drivers.Russound.Media.Audio
{

    /// <summary>
    /// Implements <see cref="IEqualization"/> for a zone on a Russound controller.
    /// </summary>
    class Sink : ZoneBase, ISink
    {

        RnetDataHandle runHandle;

        RnetDataHandle sourceIdHandle;
        int sourceId;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="zone"></param>
        public Sink(RnetZone zone)
            : base(zone)
        {
            runHandle =
                Zone.Controller[0, 0];
            sourceIdHandle =
                Zone.Controller[2, 0, Zone.Id, 1];
        }

        protected override async Task Initialize()
        {
            await base.Initialize();

            sourceIdHandle.Subscribe(d =>
                ReceiveSourceId(d[0]));
        }

        public int SourceId
        {
            get { return sourceId; }
            set { sourceId = value; RaisePropertyChanged("SourceId"); ChangeSourceId(); }
        }

        void ReceiveSourceId(byte value)
        {
            sourceId = value * 2;
            RaisePropertyChanged("SourceId");
        }

        async void ChangeSourceId()
        {
            await runHandle.SendEvent(RnetEvent.SourceSelect, sourceId, Zone.Id);
            await sourceIdHandle.Refresh();
        }

    }

}
