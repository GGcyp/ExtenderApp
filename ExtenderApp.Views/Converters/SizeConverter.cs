using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ExtenderApp.Views.Converters
{
    public class SizeConverter : MarkupExtension, IValueConverter
    {
        private readonly string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double size = 0;
            if (value == null || !double.TryParse(value.ToString(), out size))
                return "--";

            int unit = 0;
            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                ++unit;
            }
            if (size == 0) return "--";
            return string.Format("{0:#,##0.##} {1}", size, units[unit]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter is not string unitString)
                return value;

            double size;
            if (!double.TryParse(value.ToString(), out size))
                return value;

            int unitIndex = Array.IndexOf(units, unitString);
            if (unitIndex < 0) return value;

            for (int i = 0; i < unitIndex; i++)
            {
                size *= 1024;
            }
            return System.Convert.ChangeType(size, targetType);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
