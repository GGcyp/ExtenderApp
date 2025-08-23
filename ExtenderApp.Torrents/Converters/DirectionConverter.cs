

using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using MonoTorrent;

namespace ExtenderApp.Torrents.Converters
{
    public class DirectionConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Direction direction)
                return string.Empty;

            return direction switch
            {
                Direction.Incoming => "主动",
                Direction.Outgoing => "被动",
                Direction.None => "互相",
                _ => string.Empty
            };
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
