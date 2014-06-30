using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rnet.Mobile.ViewModels
{

    public class DeviceViewModel
    {

        readonly Uri uri;

        public DeviceViewModel(Uri uri)
        {
            this.uri = uri;
        }

        public Uri Uri
        {
            get { return uri; }
        }

        public string Name
        {
            get { return "wweeee!"; }
        }

    }

}
