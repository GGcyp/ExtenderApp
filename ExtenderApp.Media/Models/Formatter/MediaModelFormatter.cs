using System.Collections.ObjectModel;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;
using ExtenderApp.Media.Models;

namespace ExtenderApp.Media
{
    internal class MediaModelFormatter : VersionDataFormatter<MediaModel>
    {
        private readonly IBinaryFormatter<bool> _boolFormatter;
        private readonly IBinaryFormatter<double> _doubleFormatter;
        private readonly IBinaryFormatter<ObservableCollection<MediaInfo>> _videoInfoFormatter;

        public override int DefaultLength => _boolFormatter.DefaultLength * 2 + _doubleFormatter.DefaultLength + _videoInfoFormatter.DefaultLength;

        public override Version FormatterVersion => new Version(0, 0, 0, 1);

        public MediaModelFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _boolFormatter = GetFormatter<bool>();
            _doubleFormatter = GetFormatter<double>();
            _videoInfoFormatter = GetFormatter<ObservableCollection<MediaInfo>>();
        }

        public override MediaModel Deserialize(ref ByteBuffer buffer)
        {
            MediaModel mediaData = new();
            //mediaData.MediaInfos = _videoInfoFormatter.Deserialize(ref Reader);
            //mediaData.MediaInfos = mediaData.MediaInfos ?? new();
            return mediaData;
        }

        public override void Serialize(ref ByteBuffer buffer, MediaModel value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            //_videoInfoFormatter.Serialize(ref writer, Value.MediaInfos);
        }

        public override long GetLength(MediaModel value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return _boolFormatter.DefaultLength * 2 +
                _doubleFormatter.DefaultLength +
                _videoInfoFormatter.GetLength(value.MediaInfos);
        }
    }
}