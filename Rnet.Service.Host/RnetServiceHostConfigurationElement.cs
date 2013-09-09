using System;
using System.Configuration;

namespace Rnet.Service.Host
{

    public class RnetServiceHostConfigurationElement : ConfigurationElement
    {

        [ConfigurationProperty("uri", IsRequired = true, IsKey = true)]
        public Uri Uri
        {
            get { return (Uri)this["uri"]; }
            set { this["uri"] = value; }
        }

        [ConfigurationProperty("bus")]
        public RnetBusConfigurationElement Bus
        {
            get { return (RnetBusConfigurationElement)this["bus"]; }
            set { this["bus"] = value; }
        }

    }

}
