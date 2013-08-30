using System;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel.Web;

namespace Rnet.Service
{

    public class RnetWebHttpElement : BehaviorExtensionElement
    {

        ConfigurationPropertyCollection properties;


        protected override object CreateBehavior()
        {
            return new RnetWebHttpBehavior
            {
                HelpEnabled = this.HelpEnabled,
                DefaultBodyStyle = this.DefaultBodyStyle,
                DefaultOutgoingResponseFormat = this.DefaultOutgoingResponseFormat,
                AutomaticFormatSelectionEnabled = this.AutomaticFormatSelectionEnabled,
                FaultExceptionEnabled = this.FaultExceptionEnabled
            };
        }

        public override Type BehaviorType
        {
            get { return typeof(RnetWebHttpBehavior); }
        }

        [ConfigurationProperty("automaticFormatSelectionEnabled")]
        public bool AutomaticFormatSelectionEnabled
        {
            get { return (bool)base["automaticFormatSelectionEnabled"]; }
            set { base["automaticFormatSelectionEnabled"] = value; }
        }

        [ConfigurationProperty("defaultBodyStyle")]
        public WebMessageBodyStyle DefaultBodyStyle
        {
            get { return (WebMessageBodyStyle)base["defaultBodyStyle"]; }
            set { base["defaultBodyStyle"] = value; }
        }

        [ConfigurationProperty("defaultOutgoingResponseFormat")]
        public WebMessageFormat DefaultOutgoingResponseFormat
        {
            get { return (WebMessageFormat)base["defaultOutgoingResponseFormat"]; }
            set { base["defaultOutgoingResponseFormat"] = value; }
        }

        [ConfigurationProperty("faultExceptionEnabled")]
        public bool FaultExceptionEnabled
        {
            get { return (bool)base["faultExceptionEnabled"]; }
            set { base["faultExceptionEnabled"] = value; }
        }

        [ConfigurationProperty("helpEnabled")]
        public bool HelpEnabled
        {
            get { return (bool)base["helpEnabled"]; }
            set { base["helpEnabled"] = value; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (properties == null)
                    properties = new ConfigurationPropertyCollection()
                    {
                        new ConfigurationProperty("helpEnabled", typeof(bool), false, null, null, ConfigurationPropertyOptions.None),
                        new ConfigurationProperty("defaultBodyStyle", typeof(WebMessageBodyStyle), WebMessageBodyStyle.Bare, null, null, ConfigurationPropertyOptions.None),
                        new ConfigurationProperty("defaultOutgoingResponseFormat", typeof(WebMessageFormat), WebMessageFormat.Xml, null, null, ConfigurationPropertyOptions.None),
                        new ConfigurationProperty("automaticFormatSelectionEnabled", typeof(bool), false, null, null, ConfigurationPropertyOptions.None),
                        new ConfigurationProperty("faultExceptionEnabled", typeof(bool), false, null, null, ConfigurationPropertyOptions.None),
                    };

                return properties;
            }
        }

    }

}
