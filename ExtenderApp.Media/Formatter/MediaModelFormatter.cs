using System.Collections.ObjectModel;
using ExtenderApp.Abstract;
using ExtenderApp.Common.File.Binary.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Media
{
    internal class MediaModelFormatter : ResolverFormatter<MediaModel>
    {
        private readonly IBinaryFormatter<bool> _boolFormatter;
        private readonly IBinaryFormatter<double> _doubleFormatter;
        private readonly IBinaryFormatter<ObservableCollection<VideoInfo>> _videoInfoFormatter;

        public MediaModelFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _boolFormatter = GetFormatter<bool>();
            _doubleFormatter = GetFormatter<double>();
            _videoInfoFormatter = GetFormatter<ObservableCollection<VideoInfo>>();
        }

        public override MediaModel Deserialize(ref ExtenderBinaryReader reader)
        {
            MediaModel mediaData = new();
            mediaData.RecordWatchingTime = _boolFormatter.Deserialize(ref reader);
            mediaData.Volume = _doubleFormatter.Deserialize(ref reader);
            mediaData.VideoInfos = _videoInfoFormatter.Deserialize(ref reader);
            mediaData.VideoInfos = mediaData.VideoInfos ?? new();
            return mediaData;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, MediaModel value)
        {
            _boolFormatter.Serialize(ref writer, value.RecordWatchingTime);
            _doubleFormatter.Serialize(ref writer, value.Volume);
            _videoInfoFormatter.Serialize(ref writer, value.VideoInfos);
        }
    }
}
