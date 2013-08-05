using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

using Microsoft.Practices.Prism.Commands;

namespace Rnet.Manager
{

    /// <summary>
    /// Searches for a method with name specified through the parameter and converts it to a command.
    /// </summary>
    public class MethodNameToCommandConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var name = parameter as string;
            if (name == null)
                return null;

            var method = value.GetType()
                .GetMethods()
                .Where(i => i.Name == name)
                .FirstOrDefault();
            if (method == null)
                return null;

            return new DelegateCommand(() => method.Invoke(value, null));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

}
