using System.Collections.ObjectModel;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Media
{
    internal class MediaModelFormatter : ResolverFormatter<MediaModel>
    {
        private readonly IBinaryFormatter<bool> _boolFormatter;
        private readonly IBinaryFormatter<double> _doubleFormatter;
        private readonly IBinaryFormatter<ObservableCollection<VideoInfo>> _videoInfoFormatter;

        public override MediaModel Default => new MediaModel() { VideoInfos = _videoInfoFormatter.Default };

        public override int Count => _boolFormatter.Count * 2 + _doubleFormatter.Count + _videoInfoFormatter.Count;

        public MediaModelFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _boolFormatter = GetFormatter<bool>();
            _doubleFormatter = GetFormatter<double>();
            _videoInfoFormatter = GetFormatter<ObservableCollection<VideoInfo>>();
        }

        public override MediaModel Deserialize(ref ExtenderBinaryReader reader)
        {
            MediaModel mediaData = new();
            mediaData.VideoNotExist = _boolFormatter.Deserialize(ref reader);
            mediaData.RecordWatchingTime = _boolFormatter.Deserialize(ref reader);
            mediaData.Volume = _doubleFormatter.Deserialize(ref reader);
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

            _boolFormatter.Serialize(ref writer, value.VideoNotExist);
            _boolFormatter.Serialize(ref writer, value.RecordWatchingTime);
            _doubleFormatter.Serialize(ref writer, value.Volume);
            _videoInfoFormatter.Serialize(ref writer, value.VideoInfos);
        }

        public override int GetCount(MediaModel value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return _boolFormatter.Count * 2 + _doubleFormatter.Count + _videoInfoFormatter.GetCount(value.VideoInfos);
        }
    }
}
