using System.Collections.ObjectModel;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatters;
using ExtenderApp.Data;
using ExtenderApp.Media.Models;

namespace ExtenderApp.Media
{
    internal class MediaModelFormatter : VersionDataFormatter<MediaModel>
    {
        private readonly IBinaryFormatter<bool> _boolFormatter;
        private readonly IBinaryFormatter<double> _doubleFormatter;
        private readonly IBinaryFormatter<ObservableCollection<VideoInfo>> _videoInfoFormatter;

        public override MediaModel Default => new MediaModel() { VideoInfos = _videoInfoFormatter.Default };

        public override int DefaultLength => _boolFormatter.DefaultLength * 2 + _doubleFormatter.DefaultLength + _videoInfoFormatter.DefaultLength;

        public override Version FormatterVersion => new Version(0, 0, 0, 1);

        public MediaModelFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _boolFormatter = GetFormatter<bool>();
            _doubleFormatter = GetFormatter<double>();
            _videoInfoFormatter = GetFormatter<ObservableCollection<VideoInfo>>();
        }

        public override MediaModel Deserialize(ref ExtenderBinaryReader reader)
        {
            MediaModel mediaData = new();
            mediaData.VideoInfos = _videoInfoFormatter.Deserialize(ref reader);
            mediaData.VideoInfos = mediaData.VideoInfos ?? new();
            return mediaData;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, MediaModel value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _videoInfoFormatter.Serialize(ref writer, value.VideoInfos);
        }

        public override long GetLength(MediaModel value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return _boolFormatter.DefaultLength * 2 + _doubleFormatter.DefaultLength + _videoInfoFormatter.GetLength(value.VideoInfos);
        }
    }
}
