using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace Rnet.Android
{
    [Activity(Label = "Rnet.BasicApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity :
        TabActivity
    {

        readonly Uri baseUri = new Uri("http://kyoto.cogito.cx:12292/rnet/~0.0.127/");

        /// <summary>
        /// Called when the activity is created.
        /// </summary>
        /// <param name="bundle"></param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            // add six zone tabs
            for (int i = 1; i <= 6; i++)
                AddZoneTab(i, new Uri(baseUri, string.Format("zone-{0}/", i)));
        }

        /// <summary>
        /// Adds a zone tab.
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="uri"></param>
        void AddZoneTab(int zoneId, Uri uri)
        {
            var intent = new Intent(this, typeof(ZoneActivity));
            intent.AddFlags(ActivityFlags.NewTask);
            intent.PutExtra("Id", zoneId);
            intent.PutExtra("Uri", uri.ToString());

            var spec = TabHost.NewTabSpec("zone" + zoneId.ToString());
            spec.SetIndicator(zoneId.ToString());
            spec.SetContent(intent);
            TabHost.AddTab(spec);
        }

    }
}


