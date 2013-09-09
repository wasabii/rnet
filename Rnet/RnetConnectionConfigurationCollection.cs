using System.Configuration;

namespace Rnet
{

    [ConfigurationCollection(typeof(RnetConnectionConfigurationElement))]
    public class RnetConnectionConfigurationCollection : ConfigurationElementCollection
    {

        protected override ConfigurationElement CreateNewElement()
        {
            return new RnetConnectionConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RnetConnectionConfigurationElement)element).Name;
        }

    }

}
