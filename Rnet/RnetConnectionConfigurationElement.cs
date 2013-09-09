using System;
using System.Configuration;

namespace Rnet
{

    public class RnetConnectionConfigurationElement : ConfigurationElement
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
        /// Uri which specifies the address of the RNET connection.
        /// </summary>
        [ConfigurationProperty("uri", IsRequired = true)]
        public Uri Uri
        {
            get { return (Uri)this["uri"]; }
            set { this["uri"] = value; }
        }

    }

}
