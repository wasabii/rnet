using System;

using Xamarin.Forms;

using Rnet.Mobile.ViewModels;

namespace Rnet.Mobile.Views
{

    public partial class BusPage :
        ContentPage
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public BusPage(Uri uri)
        {
            InitializeComponent();

            this.BindingContext = new BusViewModel(uri);
        }

    }

}
