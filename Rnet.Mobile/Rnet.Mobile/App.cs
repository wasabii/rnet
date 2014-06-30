using System;

using Rnet.Mobile.Views;

using Xamarin.Forms;

namespace Rnet.Mobile
{

    public class App
    {

        static readonly Uri uri = new Uri("http://kyoto.cogito.cx:12292/rnet/");

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public App()
        {
            
        }

        public Page GetMainPage()
        {
            return new BusPage(uri);
        }

    }

}
