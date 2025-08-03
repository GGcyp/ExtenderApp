using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace ExtenderApp.Views.Converters
{
    public class StringGeometryConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string icon)
            {
                if (parameter is Geometry geometry)
                    return geometry;

                if (parameter is not string defaultIcon)
                    return null;

                return Geometry.Parse(defaultIcon);
            }


            return Geometry.Parse(icon);
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
