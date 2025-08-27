using System.Collections.ObjectModel;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Torrents.Models
{
    internal class TorrentModelFormatter : VersionDataFormatter<TorrentModel>
    {
        private readonly IBinaryFormatter<ObservableCollection<TorrentInfo>> _torrentInfos;
        private readonly IBinaryFormatter<string> _string;

        public override int DefaultLength => 1;

        public override Version FormatterVersion { get; } = new Version(1, 0, 0, 0);

        public TorrentModelFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _torrentInfos = GetFormatter<ObservableCollection<TorrentInfo>>();
            _string = GetFormatter<string>();
        }

        public override TorrentModel Deserialize(ref ExtenderBinaryReader reader)
        {
            TorrentModel model = new TorrentModel();
            if (TryReadNil(ref reader))
            {
                return model;
            }

            model.SaveDirectory = _string.Deserialize(ref reader);
            model.DowloadTorrentCollection = _torrentInfos.Deserialize(ref reader);
            model.DowloadCompletedTorrentCollection = _torrentInfos.Deserialize(ref reader);
            return model;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, TorrentModel value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            _string.Serialize(ref writer, value.SaveDirectory);
            _torrentInfos.Serialize(ref writer, value.DowloadTorrentCollection);
            _torrentInfos.Serialize(ref writer, value.DowloadCompletedTorrentCollection);
        }
    }
}
