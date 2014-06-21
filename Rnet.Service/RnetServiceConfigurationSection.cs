using System.Configuration;

using Rnet.Service.Host;

namespace Rnet.Service
{

    /// <summary>
    /// Provides a <see cref="ConfigurationSection"/> for the RNET service.
    /// </summary>
    public class RnetServiceConfigurationSection : 
        ConfigurationSection
    {

        /// <summary>
        /// Gets the <see cref="RnetServiceConfigurationSection"/> configured under 'rnet.service'.
        /// </summary>
        /// <returns></returns>
        public static RnetServiceConfigurationSection GetDefaultSection()
        {
            return (RnetServiceConfigurationSection)ConfigurationManager.GetSection("rnet.service") ?? new RnetServiceConfigurationSection();
        }

        /// <summary>
        /// Defines a collection of hosts.
        /// </summary>
        [ConfigurationProperty("hosts")]
        [ConfigurationCollection(typeof(RnetServiceHostConfigurationElement))]
        public RnetServiceHostConfigurationCollection Hosts
        {
            get { return (RnetServiceHostConfigurationCollection)this["hosts"]; }
            set { this["host"] = value; }
        }

    }

}
