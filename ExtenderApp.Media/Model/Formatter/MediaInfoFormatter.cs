using ExtenderApp.Common.IO.Binaries.Formatters;
using ExtenderApp.Data;
using ExtenderApp.Abstract;

namespace ExtenderApp.Media.Models
{
    /// <summary>
    /// VideoInfoFormatter 类是一个扩展格式化器，用于格式化
    /// VideoInfo 对象。
    /// </summary>
    /// <typeparam name="VideoInfo">
    /// 表示要格式化的 VideoInfo 类型。
    /// </typeparam>
    internal class MediaInfoFormatter : ResolverFormatter<MediaInfo>
    {
        private readonly IBinaryFormatter<string> _string;
        private readonly IBinaryFormatter<TimeSpan> _timeSpan;
        private readonly IBinaryFormatter<int> _int;
        private readonly IBinaryFormatter<long> _long;
        private readonly IBinaryFormatter<DateTime> _dateTime;
        private readonly IBinaryFormatter<bool> _bool;
        private readonly IBinaryFormatter<List<string>> _stringList;
        private readonly IBinaryFormatter<double> _double;
        private readonly IBinaryFormatter<Uri> _uri;

        public override int DefaultLength => _string.DefaultLength * 2 +
            _int.DefaultLength * 3 +
            _timeSpan.DefaultLength * 2 +
            _bool.DefaultLength +
            _double.DefaultLength +
            _long.DefaultLength +
            _dateTime.DefaultLength +
            _stringList.DefaultLength;

        public MediaInfoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _string = GetFormatter<string>();
            _timeSpan = GetFormatter<TimeSpan>();
            _int = GetFormatter<int>();
            _long = GetFormatter<long>();
            _dateTime = GetFormatter<DateTime>();
            _bool = GetFormatter<bool>();
            _stringList = GetFormatter<List<string>>();
            _double = GetFormatter<double>();
            _uri = GetFormatter<Uri>();
        }

        public override MediaInfo Deserialize(ref ExtenderBinaryReader reader)
        {
            MediaInfo info = new MediaInfo(null);

            info.Title = _string.Deserialize(ref reader);

            info.Height = _int.Deserialize(ref reader);
            info.Width = _int.Deserialize(ref reader);
            info.PlayCount = _int.Deserialize(ref reader);

            info.TotalVideoDuration = _long.Deserialize(ref reader);
            info.MediaWatchedPosition = _long.Deserialize(ref reader);

            info.IsFavorite = _bool.Deserialize(ref reader);

            return info;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, MediaInfo value)
        {
            _uri.Serialize(ref writer, value.MediaUri);
            _string.Serialize(ref writer, value.Title);

            _int.Serialize(ref writer, value.Height);
            _int.Serialize(ref writer, value.Width);
            _int.Serialize(ref writer, value.PlayCount);

            _long.Serialize(ref writer, value.TotalVideoDuration);
            _long.Serialize(ref writer, value.MediaWatchedPosition);

            _bool.Serialize(ref writer, value.IsFavorite);
        }

        public override long GetLength(MediaInfo value)
        {
            if (value is null)
            {
                return 1;
            }

            long result = _int.DefaultLength * 3 + _timeSpan.DefaultLength * 2
                + _bool.DefaultLength + _double.DefaultLength
                + _long.DefaultLength + _dateTime.DefaultLength;
            result += _string.GetLength(value.Title);
            return result;
        }
    }
}