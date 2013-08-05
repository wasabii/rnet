using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

using Rnet.Profiles;

namespace Rnet.Manager
{

    /// <summary>
    /// Obtains the RNET profile of the given type for the path bound to.
    /// </summary>
    [MarkupExtensionReturnType(typeof(IProfile))]
    public class RnetProfileBindingExtension : MarkupExtension
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetProfileBindingExtension(Type profileType)
            : base()
        {
            ProfileType = profileType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // provides the bus object
            var binding = new Binding()
            {
                Mode = BindingMode.OneWay,
                Path = Path,
            };

            if (ElementName != null)
                binding.ElementName = ElementName;
            else if (RelativeSource != null)
                binding.RelativeSource = RelativeSource;
            else if (Source != null)
                binding.Source = Source;
            
            binding.Converter = new RnetBusObjectToProfileConverter();
            binding.ConverterParameter = ProfileType;

            return binding.ProvideValue(serviceProvider);
        }

        public string ElementName { get; set; }

        public PropertyPath Path { get; set; }

        public RelativeSource RelativeSource { get; set; }

        public object Source { get; set; }

        public object ProfileType { get; set; }

    }

}
