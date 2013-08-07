using System;
using System.Globalization;
using System.Windows.Data;

using Rnet.Drivers;

namespace Rnet.Manager
{

    public class RnetBusObjectToProfileConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var busObject = value as RnetBusObject;
            if (busObject == null)
                return null;

            var profile = busObject.GetProfile((parameter as Type) ?? targetType).Result;
            if (profile == null)
                return null;

            return profile;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

}
