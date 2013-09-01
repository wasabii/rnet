using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Rnet.Profiles.Metadata
{

    /// <summary>
    /// Describes a value available on a profile contract.
    /// </summary>
    public sealed class ValueDescriptor
    {

        ProfileDescriptor profile;
        PropertyInfo propertyInfo;
        bool ignore;
        string name;
        XName xmlName;
        bool isXmlElement;
        bool isXmlAttribute;
        Type type;
        int order;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        internal ValueDescriptor(ProfileDescriptor profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            this.profile = profile;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(profile != null);

            Contract.Invariant(propertyInfo == null || propertyInfo.DeclaringType == profile.Contract);
            Contract.Invariant(propertyInfo == null || !string.IsNullOrWhiteSpace(name));
            Contract.Invariant(propertyInfo == null || xmlName != null);
            Contract.Invariant(propertyInfo == null || isXmlElement || isXmlAttribute);
            Contract.Invariant(!(isXmlElement && isXmlAttribute));
            Contract.Invariant(propertyInfo == null || type != null);
        }

        /// <summary>
        /// Associated <see cref="ProfileDescriptor"/>.
        /// </summary>
        public ProfileDescriptor Profile
        {
            get { return profile; }
        }

        /// <summary>
        /// Gets the <see cref="propertyInfo"/> described by this <see cref="ValueDescriptor"/>.
        /// </summary>
        public PropertyInfo PropertyInfo
        {
            get { return propertyInfo; }
        }

        /// <summary>
        /// Determines whether or not the value property is hidden.
        /// </summary>
        internal bool Ignore
        {
            get { return ignore; }
        }

        /// <summary>
        /// Name of the value.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Element name to use when returning profile information as XML.
        /// </summary>
        public XName XmlName
        {
            get { return xmlName; }
        }

        /// <summary>
        /// Whether the value should be output as an XML attribute.
        /// </summary>
        public bool IsXmlAttribute
        {
            get { return isXmlAttribute; }
        }

        /// <summary>
        /// Whether the value should be output as an XML attribute.
        /// </summary>
        public bool IsXmlElement
        {
            get { return isXmlElement; }
        }

        /// <summary>
        /// Gets the type of the return value to be expected.
        /// </summary>
        public Type Type
        {
            get { return type; }
        }

        /// <summary>
        /// Order of the value.
        /// </summary>
        public int Order
        {
            get { return order; }
        }

        /// <summary>
        /// Loads the descriptor from the given property.
        /// </summary>
        /// <param name="property"></param>
        internal void Load(PropertyInfo property)
        {
            Contract.Requires<ArgumentNullException>(property != null);
            Contract.Requires<InvalidCastException>(property.DeclaringType == Profile.Contract);
            Contract.Ensures(propertyInfo != null);

            propertyInfo = property;
            LoadPropertyInfo();
            LoadDataMember();
            LoadXmlAttributes();
            LoadDataAnnotations();
        }

        /// <summary>
        /// Loads information from the property itself.
        /// </summary>
        void LoadPropertyInfo()
        {
            Contract.Requires(propertyInfo != null);
            Contract.Ensures(name != null);
            Contract.Ensures(type != null);

            name = propertyInfo.Name;
            type = propertyInfo.PropertyType;
            xmlName = XName.Get(name, profile.XmlName.NamespaceName);
            isXmlElement = true;
            isXmlAttribute = false;
        }

        /// <summary>
        /// Loads information from the <see cref="DataMemberAttribute"/>.
        /// </summary>
        void LoadDataMember()
        {
            Contract.Requires(propertyInfo != null);
            Contract.Ensures(name != null);

            var iattr = propertyInfo.GetCustomAttribute<IgnoreDataMemberAttribute>();
            if (iattr != null)
                ignore = true;

            var attr = propertyInfo.GetCustomAttribute<DataMemberAttribute>();
            if (attr == null)
                return;

            if (!string.IsNullOrWhiteSpace(attr.Name))
                xmlName = XName.Get(attr.Name, xmlName.NamespaceName);

            order = attr.Order;
        }

        /// <summary>
        /// Loads information from the XML attributes.
        /// </summary>
        void LoadXmlAttributes()
        {
            LoadXmlElementAttribute();
            LoadXmlAttributeAttribute();
        }

        /// <summary>
        /// Loads information from <see cref="XmlElementAttribute"/>.
        /// </summary>
        void LoadXmlElementAttribute()
        {
            var attr = propertyInfo.GetCustomAttribute<XmlElementAttribute>();
            if (attr == null)
                return;

            if (!string.IsNullOrWhiteSpace(attr.ElementName))
                xmlName = XName.Get(attr.ElementName, xmlName.NamespaceName);

            isXmlElement = true;
            isXmlAttribute = false;
        }

        /// <summary>
        /// Loads information from <see cref="XmlAttributeAttribute"/>.
        /// </summary>
        void LoadXmlAttributeAttribute()
        {
            var attr = propertyInfo.GetCustomAttribute<XmlAttributeAttribute>();
            if (attr == null)
                return;

            if (!string.IsNullOrWhiteSpace(attr.AttributeName))
                xmlName = XName.Get(attr.AttributeName, xmlName.NamespaceName);

            isXmlElement = false;
            isXmlAttribute = true;
        }

        /// <summary>
        /// Loads information from any data annotations.
        /// </summary>
        void LoadDataAnnotations()
        {

        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public T GetValue<T>(object instance)
        {
            Contract.Requires<ArgumentNullException>(instance != null);
            Contract.Requires<InvalidCastException>(typeof(T).IsAssignableFrom(Type));
            return (T)GetValue(instance);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public object GetValue(object instance)
        {
            Contract.Requires<ArgumentNullException>(instance != null);
            Contract.Requires<InvalidCastException>(Profile.Contract.IsInstanceOfType(instance));
            Contract.Requires<InvalidCastException>(PropertyInfo.DeclaringType.IsInstanceOfType(instance));
            return propertyInfo.GetValue(instance);
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void SetValue<T>(object instance, T value)
        {
            Contract.Requires<InvalidCastException>(Type.IsAssignableFrom(typeof(T)));
            SetValue(instance, value);
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void SetValue(object instance, object value)
        {
            Contract.Requires<ArgumentNullException>(instance != null);
            Contract.Requires<InvalidCastException>(Profile.Contract.IsInstanceOfType(instance));
            Contract.Requires<InvalidCastException>(PropertyInfo.DeclaringType.IsInstanceOfType(instance));
            propertyInfo.SetValue(instance, value);
        }

    }

}
