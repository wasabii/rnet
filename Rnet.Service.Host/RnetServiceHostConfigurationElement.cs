using System;
using System.Configuration;

namespace Rnet.Service.Host
{

    /// <summary>
    /// Defines a single configured RNET host.
    /// </summary>
    public class RnetServiceHostConfigurationElement : 
        ConfigurationElement
    {

        /// <summary>
        /// Gets or sets the <see cref="Uri"/> to which the host will be published.
        /// </summary>
        [ConfigurationProperty("uri", IsRequired = true, IsKey = true)]
        public string Uri
        {
            get { return (string)this["uri"]; }
            set { this["uri"] = value; }
        }

        /// <summary>
        /// Provides configuration for the associated <see cref="RnetBus"/>.
        /// </summary>
        [ConfigurationProperty("bus")]
        public RnetBusConfigurationElement Bus
        {
            get { return (RnetBusConfigurationElement)this["bus"]; }
            set { this["bus"] = value; }
        }

    }

}
