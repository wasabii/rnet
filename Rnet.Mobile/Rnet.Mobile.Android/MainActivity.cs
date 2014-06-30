using Android.App;
using Android.OS;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace Rnet.Mobile.Android
{

    [Activity(Label = "Rnet.Mobile", MainLauncher = true)]
    public class MainActivity :
        AndroidActivity
    {

        readonly App app;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MainActivity()
        {
            this.app = new App();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // initialize bundle
            Forms.Init(this, bundle);

            // set main application page
            SetPage(app.GetMainPage());
        }

    }

}

