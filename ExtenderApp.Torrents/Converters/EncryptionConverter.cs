using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ExtenderApp.Torrents.Converters
{
    public class EncryptionConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not MonoTorrent.Connections.EncryptionType encryptionType)
            {
                return string.Empty;
            }

            return encryptionType switch
            {
                MonoTorrent.Connections.EncryptionType.PlainText => "明文传输",
                MonoTorrent.Connections.EncryptionType.RC4Header => "加密握手",
                MonoTorrent.Connections.EncryptionType.RC4Full => "全程加密",
                _ => "明文"
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
