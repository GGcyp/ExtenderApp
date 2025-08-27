using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ExtenderApp.Torrents.Converters
{
    internal class MaxWidthConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double width = (double)value;

            if (parameter is not string minuend)
                return new GridLength(200);

            width -= double.Parse(minuend);
            if(width <= 0) return new GridLength(0);
            return new GridLength(width);
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
