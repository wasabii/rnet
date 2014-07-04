using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rnet.Android
{

    /// <summary>
    /// Zone activity.
    /// </summary>
    [Activity(Label = "ZoneActivity")]
    public class ZoneActivity :
        Activity
    {

        static readonly string[] AUDIO_SOURCES = new[]
        {
            "Source 1",
            "Source 2",
            "Source 3",
            "Source 4",
            "Source 5",
            "Source 6",
        };

        class ZoneData
        {

            public bool Power { get; set; }

            public int Source { get; set; }

        }

        class AudioEqualizationData
        {

            public int Volume { get; set; }

            public int Bass { get; set; }

            public int Treble { get; set; }

            public int Balance { get; set; }

        }

        /// <summary>
        /// Initializes the static instance.
        /// </summary>
        static WebClient OpenWebClient()
        {
            var web = new WebClient();
            web.Headers.Set(HttpRequestHeader.Accept, "application/json");
            return web;
        }

        int id;
        Uri uri;
        Timer timer;

        ToggleButton powerButton;
        SeekBar volumeSeekBar;
        Spinner sourceSpinner;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Zone);

            id = Intent.GetIntExtra("Id", 0);
            uri = new Uri(Intent.GetStringExtra("Uri"));

            timer = new Timer();
            timer.Interval = 5000;
            timer.Elapsed += timer_Elapsed;

            powerButton = FindViewById<ToggleButton>(Resource.Id.powerButton);
            powerButton.Click += powerButton_Click;

            volumeSeekBar = FindViewById<SeekBar>(Resource.Id.volumeSeekBar);
            volumeSeekBar.ProgressChanged += volumeSeekBar_ProgressChanged;

            sourceSpinner = FindViewById<Spinner>(Resource.Id.sourceSpinner);
            sourceSpinner.Adapter = new ArrayAdapter<string>(this, global::Android.Resource.Layout.SimpleSpinnerDropDownItem, AUDIO_SOURCES);
            sourceSpinner.ItemSelected += sourceSpinner_ItemSelected;

            // schedule load
            Task.Run(async () => await Load());
        }

        protected override void OnStart()
        {
            base.OnStart();

            timer.Start();
        }

        protected override void OnResume()
        {
            base.OnResume();

            timer.Start();
        }

        protected override void OnPause()
        {
            base.OnPause();

            timer.Stop();
        }

        protected override void OnStop()
        {
            base.OnStop();

            timer.Stop();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            timer.Dispose();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Task.Run(async () =>
            {
                await Load();
            });
        }

        async Task<JObject> LoadJson(Uri uri)
        {
            using (var web = OpenWebClient())
            using (var stm = await web.OpenReadTaskAsync(uri))
            using (var rdr = new JsonTextReader(new StreamReader(stm)))
                return JValue.Load(rdr) as JObject;
        }

        Task<JObject> LoadZone()
        {
            return LoadJson(new Uri(uri, "~media.Zone/"));
        }

        Task<JObject> LoadAudioEqualization()
        {
            return LoadJson(new Uri(uri, "~media.audio.Equalization/"));
        }

        async Task<ZoneData> LoadZoneData()
        {
            var obj = await LoadZone();
            if (obj == null)
                return null;

            return new ZoneData()
            {
                Power = (int)obj["Properties"]["Power"]["Value"] == 0 ? false : true,
                Source = (int)obj["Properties"]["Source"]["Value"],
            };
        }

        async Task<AudioEqualizationData> LoadAudioEqualizationData()
        {
            var obj = await LoadAudioEqualization();
            if (obj == null)
                return null;

            return new AudioEqualizationData()
            {
                Volume = (int)obj["Properties"]["Volume"]["Value"],
                Bass = (int)obj["Properties"]["Bass"]["Value"],
                Treble = (int)obj["Properties"]["Treble"]["Value"],
                Balance = (int)obj["Properties"]["Balance"]["Value"],
            };
        }

        async Task Load()
        {
            try
            {
                var zone = await LoadZoneData();
                if (zone == null)
                    return;


                var audio = await LoadAudioEqualizationData();
                if (audio == null)
                    return;

                RunOnUiThread(() =>
                {
                    powerButton.Checked = zone.Power;
                    volumeSeekBar.Progress = audio.Volume;
                    sourceSpinner.SetSelection(zone.Source);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void powerButton_Click(object sender, EventArgs args)
        {
            if (powerButton.Checked)
                PowerOn();
            else
                PowerOff();
        }

        void PowerOn()
        {
            Task.Run(async () =>
            {
                await LoadJson(new Uri(uri, "~media.Zone/PowerOn?Execute"));
                await Task.Delay(500);
                await LoadJson(new Uri(uri, "~media.Zone/PowerOn?Execute"));
                await Task.Delay(500);
                await Load();
            });
        }

        void PowerOff()
        {
            Task.Run(async () =>
            {
                await LoadJson(new Uri(uri, "~media.Zone/PowerOff?Execute"));
                await Task.Delay(500);
                await LoadJson(new Uri(uri, "~media.Zone/PowerOff?Execute"));
                await Task.Delay(500);
                await Load();
            });
        }

        void volumeSeekBar_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs args)
        {
            if (args.FromUser)
            {
                SetVolume(args.Progress);
            }
        }

        void SetVolume(int value)
        {
            Task.Run(async () =>
            {
                await LoadJson(new Uri(uri, string.Format("~media.audio.Equalization/Volume?Value={0}", value)));
                await Load();
            });
        }

        void sourceSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs args)
        {
            SetSource(args.Position);
        }

        void SetSource(int value)
        {
            Task.Run(async () =>
            {
                await LoadJson(new Uri(uri, string.Format("~media.Zone/Source?Value={0}", value)));
                await Load();
            });
        }

    }

}

