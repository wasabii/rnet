using System.Windows.Data;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using System.Linq.Expressions;

namespace Rnet.Monitor.Wpf
{

    public class MethodNameToCommandConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
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

            return new DelegateCommand(() =>
            {
                method.Invoke(value, null);
            });
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

    }

}
