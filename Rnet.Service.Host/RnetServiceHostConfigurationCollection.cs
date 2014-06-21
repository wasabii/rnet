using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace Rnet.Service.Host
{

    /// <summary>
    /// Defines a collection of <see cref="RnetServiceHostConfigurationElement"/>s.
    /// </summary>
    public class RnetServiceHostConfigurationCollection :
        ConfigurationElementCollection,
        IEnumerable<RnetServiceHostConfigurationElement>
    {

        protected override ConfigurationElement CreateNewElement()
        {
            return new RnetServiceHostConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RnetServiceHostConfigurationElement)element).Uri;
        }

        public new IEnumerator<RnetServiceHostConfigurationElement> GetEnumerator()
        {
            foreach (RnetServiceHostConfigurationElement i in (IEnumerable)this)
                yield return i;
        }

    }

}
