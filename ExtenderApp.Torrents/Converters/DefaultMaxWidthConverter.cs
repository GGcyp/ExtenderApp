using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace ExtenderApp.Torrents.Converters
{
    public class DefaultMaxWidthConverter : MarkupExtension, IValueConverter
    {
        private const int DefaultManWidth = 200;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int Depth = (int)value;
            return DefaultManWidth - Depth * 20;
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
