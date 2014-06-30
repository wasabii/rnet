using System.Configuration;

namespace Rnet
{

    /// <summary>
    /// Provides configuration for a <see cref="RnetBus"/>
    /// </summary>
    public class RnetBusConfigurationElement : 
        ConfigurationElement
    {

        /// <summary>
        /// Name of the bus.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = false)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        /// Configuration related to connecting to the RNET.
        /// </summary>
        [ConfigurationProperty("connection", IsRequired = true)]
        public RnetConnectionConfigurationElement Connection
        {
            get { return (RnetConnectionConfigurationElement)this["connection"]; }
            set { this["connection"] = value; }
        }

    }

}
