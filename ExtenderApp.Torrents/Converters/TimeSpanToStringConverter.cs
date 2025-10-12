using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ExtenderApp.Torrents.Converters
{
    public class TimeSpanToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan ts)
            {
                if (ts.Days >= 1)
                    return $"{ts.Days}天";

                return ts.ToString(@"hh\:mm\:ss");
            }
            // 支持秒数（int/long/Float64）
            if (value is double d)
                return Convert(TimeSpan.FromSeconds(d), targetType, parameter, culture);
            if (value is int i)
                return Convert(TimeSpan.FromSeconds(i), targetType, parameter, culture);
            if (value is long l)
                return Convert(TimeSpan.FromSeconds(l), targetType, parameter, culture);

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}