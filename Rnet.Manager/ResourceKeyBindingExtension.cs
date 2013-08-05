using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Rnet.Manager
{

    /// <summary>
    /// Searches for a resource given a name specified in a binding.
    /// </summary>
    public class ResourceKeyBindingExtension : MarkupExtension
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ResourceKeyBindingExtension()
            : base()
        {

        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var resourceKeyBinding = new Binding()
            {
                BindsDirectlyToSource = BindsDirectlyToSource,
                Mode = BindingMode.OneWay,
                Path = Path,
                XPath = XPath,
            };

            if (ElementName != null)
                resourceKeyBinding.ElementName = ElementName;
            else if (RelativeSource != null)
                resourceKeyBinding.RelativeSource = RelativeSource;
            else if (Source != null)
                resourceKeyBinding.Source = Source;

            var targetElementBinding = new Binding()
            {
                RelativeSource = new RelativeSource()
                {
                    Mode = RelativeSourceMode.Self,
                }
            };

            var multiBinding = new MultiBinding();
            multiBinding.Bindings.Add(targetElementBinding);
            multiBinding.Bindings.Add(resourceKeyBinding);

            multiBinding.Converter = new ResourceKeyToResourceConverter()
            {
                ResourceKeyConverter = Converter,
                ConverterParameter = ConverterParameter,
                StringFormat = StringFormat,
            };

            return multiBinding.ProvideValue(serviceProvider);
        }

        [DefaultValue("")]
        public object AsyncState { get; set; }

        [DefaultValue(false)]
        public bool BindsDirectlyToSource { get; set; }

        [DefaultValue("")]
        public IValueConverter Converter { get; set; }

        [TypeConverter(typeof(CultureInfoIetfLanguageTagConverter))]
        [DefaultValue("")]
        public CultureInfo ConverterCulture { get; set; }

        [DefaultValue("")]
        public object ConverterParameter { get; set; }

        [DefaultValue("")]
        public string ElementName { get; set; }

        [DefaultValue("")]
        public PropertyPath Path { get; set; }

        [DefaultValue("")]
        public RelativeSource RelativeSource { get; set; }

        [DefaultValue("")]
        public object Source { get; set; }

        [DefaultValue("")]
        public string XPath { get; set; }

        [DefaultValue("")]
        public string StringFormat { get; set; }

    }

}
