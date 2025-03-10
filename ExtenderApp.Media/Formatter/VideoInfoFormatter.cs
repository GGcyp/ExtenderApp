﻿using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Common.IO.Binaries;
using ExtenderApp.Data;
using ExtenderApp.Abstract;

namespace ExtenderApp.Media.Model
{
    /// <summary>
    /// VideoInfoFormatter 类是一个扩展格式化器，用于格式化 VideoInfo 对象。
    /// </summary>
    /// <typeparam name="VideoInfo">表示要格式化的 VideoInfo 类型。</typeparam>
    internal class VideoInfoFormatter : ResolverFormatter<VideoInfo>
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

        public override int Length => _string.Length * 2 + _int.Length * 3 + _timeSpan.Length * 2 + _bool.Length + _double.Length + _long.Length + _dateTime.Length + _stringList.Length;

        public VideoInfoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
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

        public override VideoInfo Deserialize(ref ExtenderBinaryReader reader)
        {
            VideoInfo info = new VideoInfo(_uri.Deserialize(ref reader));

            info.VideoTitle = _string.Deserialize(ref reader);
            info.Category = _string.Deserialize(ref reader);

            info.VideoHeight = _int.Deserialize(ref reader);
            info.VideoWidth = _int.Deserialize(ref reader);
            info.PlayCount = _int.Deserialize(ref reader);

            info.TotalVideoDuration = _timeSpan.Deserialize(ref reader);
            info.VideoWatchedPosition = _timeSpan.Deserialize(ref reader);

            info.IsFavorite = _bool.Deserialize(ref reader);

            info.Rating = _double.Deserialize(ref reader);

            info.FileSize = _long.Deserialize(ref reader);

            info.CreationTime = _dateTime.Deserialize(ref reader);

            info.Tags = _stringList.Deserialize(ref reader);

            if (info.VideoUri.IsFile)
                info.VideoFileInfo = new LocalFileInfo(info.VideoUri.LocalPath);

            return info;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, VideoInfo value)
        {
            _uri.Serialize(ref writer, value.VideoUri);
            _string.Serialize(ref writer, value.VideoTitle);
            _string.Serialize(ref writer, value.Category);

            _int.Serialize(ref writer, value.VideoHeight);
            _int.Serialize(ref writer, value.VideoWidth);
            _int.Serialize(ref writer, value.PlayCount);

            _timeSpan.Serialize(ref writer, value.TotalVideoDuration);
            _timeSpan.Serialize(ref writer, value.VideoWatchedPosition);

            _bool.Serialize(ref writer, value.IsFavorite);

            _double.Serialize(ref writer, value.Rating);

            _long.Serialize(ref writer, value.FileSize);

            _dateTime.Serialize(ref writer, value.CreationTime);

            _stringList.Serialize(ref writer, value.Tags);
        }

        public override long GetLength(VideoInfo value)
        {
            if(value is null)
            {
                return 1;
            }

            long result = _int.Length * 3 + _timeSpan.Length * 2 + _bool.Length + _double.Length + _long.Length + _dateTime.Length;
            result += _string.GetLength(value.VideoTitle) + _string.GetLength(value.Category);
            result += _stringList.GetLength(value.Tags);
            return result;
        }
    }
}
