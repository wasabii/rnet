using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Rnet.Android
{

    [Activity(Label = "RNet", MainLauncher = true, Icon = "@drawable/Icon")]
    public class MainActivity :
        TabActivity
    {

        Uri baseUri = new Uri("http://kyoto.cogito.cx:12292/rnet/~0.0.127/");

        /// <summary>
        /// Called when the activity is created.
        /// </summary>
        /// <param name="bundle"></param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);
            CreateTabs();
        }

        void CreateTabs()
        {
            TabHost.ClearAllTabs();
            for (int i = 1; i <= 6; i++)
                AddZoneTab(i, new Uri(baseUri, string.Format("zone-{0}/", i)));
        }

        /// <summary>
        /// Initializes the standard menu items.
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public override bool OnCreateOptionsMenu(global::Android.Views.IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.MainMenu, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(global::Android.Views.IMenuItem item)
        {
            if (item.ItemId == Resource.Id.server)
            {
                var d = new Dialog(this);
                d.RequestWindowFeature((int)WindowFeatures.NoTitle);
                d.SetContentView(Resource.Layout.ServerEdit);

                var t = d.FindViewById<EditText>(Resource.Id.uri);
                t.Text = baseUri.ToString();

                var b = d.FindViewById<Button>(Resource.Id.okButton);
                b.Click += (s, a) =>
                {
                    d.Dismiss();

                    // attempt to parse uri
                    Uri uri;
                    if (Uri.TryCreate(t.Text, UriKind.Absolute, out uri))
                        baseUri = uri;

                    // recreate tabs
                    CreateTabs();
                };

                d.Show();
            }

            return base.OnOptionsItemSelected(item);
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


