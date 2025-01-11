using ExtenderApp.Common.File.Binary.Formatter;
using ExtenderApp.Common.File.Binary;
using ExtenderApp.Data;
using ExtenderApp.Abstract;

namespace ExtenderApp.Media.Model
{
    /// <summary>
    /// VideoInfoFormatter 类是一个扩展格式化器，用于格式化 VideoInfo 对象。
    /// </summary>
    /// <typeparam name="VideoInfo">表示要格式化的 VideoInfo 类型。</typeparam>
    internal class VideoInfoFormatter : ExtenderFormatter<VideoInfo>
    {
        private readonly IBinaryFormatter<string> _stringFormatter;
        private readonly IBinaryFormatter<TimeSpan> _timeSpanFormatter;
        private readonly IBinaryFormatter<int> _intFormatter;
        private readonly IBinaryFormatter<long> _longFormatter;
        private readonly IBinaryFormatter<DateTime> _dateTimeFormatter;
        private readonly IBinaryFormatter<bool> _boolFormatter;
        private readonly IBinaryFormatter<List<string>> _stringListFormatter;
        private readonly IBinaryFormatter<double> _doubleFormatter;

        public VideoInfoFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
            _stringFormatter = resolver.GetFormatter<string>();
            _timeSpanFormatter = resolver.GetFormatter<TimeSpan>();
            _intFormatter = resolver.GetFormatter<int>();
            _longFormatter = resolver.GetFormatter<long>();
            _dateTimeFormatter = resolver.GetFormatter<DateTime>();
            _boolFormatter = resolver.GetFormatter<bool>();
            _stringListFormatter = resolver.GetFormatter<List<string>>();
            _doubleFormatter = resolver.GetFormatter<double>();
        }

        public override VideoInfo Deserialize(ref ExtenderBinaryReader reader)
        {
            VideoInfo info = new VideoInfo();

            info.VideoTitle = _stringFormatter.Deserialize(ref reader);
            info.Category = _stringFormatter.Deserialize(ref reader);
            info.VideoPath = _stringFormatter.Deserialize(ref reader);

            info.VideoHeight = _intFormatter.Deserialize(ref reader);
            info.VideoWidth = _intFormatter.Deserialize(ref reader);
            info.PlayCount = _intFormatter.Deserialize(ref reader);

            info.TotalVideoDuration = _timeSpanFormatter.Deserialize(ref reader);
            info.VideoWatchedDuration = _timeSpanFormatter.Deserialize(ref reader);

            info.IsFavorite = _boolFormatter.Deserialize(ref reader);

            info.Rating = _doubleFormatter.Deserialize(ref reader);

            info.FileSize = _longFormatter.Deserialize(ref reader);

            info.CreationTime = _dateTimeFormatter.Deserialize(ref reader);

            info.Tags = _stringListFormatter.Deserialize(ref reader);

            return info;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, VideoInfo value)
        {
            _stringFormatter.Serialize(ref writer, value.VideoTitle);
            _stringFormatter.Serialize(ref writer, value.Category);
            _stringFormatter.Serialize(ref writer, value.VideoPath);

            _intFormatter.Serialize(ref writer, value.VideoHeight);
            _intFormatter.Serialize(ref writer, value.VideoWidth);
            _intFormatter.Serialize(ref writer, value.PlayCount);

            _timeSpanFormatter.Serialize(ref writer, value.TotalVideoDuration);
            _timeSpanFormatter.Serialize(ref writer, value.VideoWatchedDuration);

            _boolFormatter.Serialize(ref writer, value.IsFavorite);

            _doubleFormatter.Serialize(ref writer, value.Rating);

            _longFormatter.Serialize(ref writer, value.FileSize);

            _dateTimeFormatter.Serialize(ref writer, value.CreationTime);

            _stringListFormatter.Serialize(ref writer, value.Tags);
        }
    }
}
