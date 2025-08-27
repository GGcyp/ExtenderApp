using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ExtenderApp.Views.Converters
{
    public class ObjectToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 默认为正向转换：true -> Visible，false -> Collapsed
            bool isVisible = value != null;

            if (parameter is not string param)
                return isVisible ? Visibility.Visible : Visibility.Collapsed;

            // 检查是否需要反向转换
            if (param.Contains("Inverse", StringComparison.OrdinalIgnoreCase))
            {
                isVisible = !isVisible;
            }
            if (param.Contains("Hidden", StringComparison.OrdinalIgnoreCase))
            {
                return isVisible ? Visibility.Visible : Visibility.Hidden;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
