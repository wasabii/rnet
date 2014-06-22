using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Rnet.Mobile.ViewModels
{

    public class BusViewModel
    {

        readonly Uri uri;
        string name;
        readonly ObservableCollection<DeviceViewModel> devices;

        public BusViewModel(Uri uri)
        {
            this.uri = uri;
            this.devices = new ObservableCollection<DeviceViewModel>();

            Load();
        }

        public Uri Uri
        {
            get { return uri; }
        }

        public string Name
        {
            get {  return name;}
        }

        public ObservableCollection<DeviceViewModel> Devices
        {
            get { return devices; }
        }

        async void Load()
        {
                var c = new System.Net.Http.HttpClient();
                c.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                dynamic r = JObject.Load(new JsonTextReader(new StreamReader(await c.GetStreamAsync(uri))));

                name = (string)r.Name;

                foreach (var u in GetDevices(r))
                    devices.Add(new DeviceViewModel(u));
        }

        IEnumerable<Uri> GetDevices(dynamic bus)
        {
            foreach (var device in bus.Devices)
            {
                yield return device.Uri;
            }
        }

    }

}
