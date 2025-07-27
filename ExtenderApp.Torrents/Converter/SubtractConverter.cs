using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ExtenderApp.Torrents.Converter
{
    internal class SubtractConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value.ToString(), out double width) &&
                double.TryParse(parameter.ToString(), out double subtract))
            {
                return width - subtract;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
