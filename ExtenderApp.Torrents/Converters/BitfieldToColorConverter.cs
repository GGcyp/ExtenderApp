using System.Globalization;
using System.Numerics;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using ExtenderApp.Torrents.Models;

namespace ExtenderApp.Torrents.Converters
{
    public class BItfleToColorConverter : MarkupExtension, IValueConverter
    {
        private readonly SolidColorBrush _dontDownloadedBrush;
        private readonly SolidColorBrush _toBeDownloadedBrush;
        private readonly SolidColorBrush _downloadingBrush;
        private readonly SolidColorBrush _completeBrush;

        public BItfleToColorConverter()
        {
            // 未下载：深灰色
            _dontDownloadedBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x44, 0x44, 0x44));
            // 待下载：中灰色带蓝调
            _toBeDownloadedBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x60, 0x7D, 0x8B));
            // 正在下载：橙色/琥珀色
            _downloadingBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x98, 0x00));
            // 已完成：绿色
            _completeBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x4C, 0xAF, 0x50));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not TorrentPieceStateType type)
                return _dontDownloadedBrush;

            return type switch
            {
                TorrentPieceStateType.DontDownloaded => _dontDownloadedBrush,
                TorrentPieceStateType.ToBeDownloaded => _toBeDownloadedBrush,
                TorrentPieceStateType.Downloading => _downloadingBrush,
                TorrentPieceStateType.Complete => _completeBrush,
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
