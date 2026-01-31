using System.Collections.ObjectModel;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Serializations.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Torrents.Models
{
    internal class TorrentModelFormatter : VersionDataFormatter<TorrentModel>
    {
        private readonly IBinaryFormatter<ObservableCollection<TorrentInfo>> _torrentInfos;
        private readonly IBinaryFormatter<string> _string;
        private readonly IBinaryFormatter<TorrentSettingsBuilderModel> _torrentSettings;
        private readonly IBinaryFormatter<EngineSettingsBuilderModel> _engineSettings;
        private readonly IBinaryFormatter<HashSet<HashValue>> _hash;

        public override int DefaultLength => 1;

        public override Version FormatterVersion { get; } = new Version(1, 0, 0, 0);

        public TorrentModelFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _torrentInfos = GetFormatter<ObservableCollection<TorrentInfo>>();
            _string = GetFormatter<string>();
            _torrentSettings = GetFormatter<TorrentSettingsBuilderModel>();
            _engineSettings = GetFormatter<EngineSettingsBuilderModel>();
            _hash = GetFormatter<HashSet<HashValue>>();
        }

        public override TorrentModel Deserialize(ref ByteBuffer buffer)
        {
            TorrentModel model = new TorrentModel();
            if (TryReadNil(ref buffer))
            {
                return model;
            }

            model.SaveDirectory = _string.Deserialize(ref buffer);
            model.EngineSettingsModel = _engineSettings.Deserialize(ref buffer);
            model.TorrentSettingsModel = _torrentSettings.Deserialize(ref buffer);
            model.DowloadTorrentCollection = _torrentInfos.Deserialize(ref buffer);
            model.DowloadCompletedTorrentCollection = _torrentInfos.Deserialize(ref buffer);
            model.RecycleBinCollection = _torrentInfos.Deserialize(ref buffer);
            model.InfoHashHashSet = _hash.Deserialize(ref buffer);
            return model;
        }

        public override void Serialize(ref ByteBuffer buffer, TorrentModel value)
        {
            if (value == null)
            {
                WriteNil(ref buffer);
                return;
            }

            _string.Serialize(ref buffer, value.SaveDirectory);
            _engineSettings.Serialize(ref buffer, value.EngineSettingsModel);
            _torrentSettings.Serialize(ref buffer, value.TorrentSettingsModel);
            _torrentInfos.Serialize(ref buffer, value.DowloadTorrentCollection);
            _torrentInfos.Serialize(ref buffer, value.DowloadCompletedTorrentCollection);
            _torrentInfos.Serialize(ref buffer, value.RecycleBinCollection);
            _hash.Serialize(ref buffer, value.InfoHashHashSet);
        }

        public override long GetLength(TorrentModel value)
        {
            if (value == null)
            {
                return 1;
            }

            return _engineSettings.GetLength(value.EngineSettingsModel) +
                   _torrentSettings.GetLength(value.TorrentSettingsModel) +
                   _string.GetLength(value.SaveDirectory) +
                   _torrentInfos.GetLength(value.DowloadTorrentCollection) +
                   _torrentInfos.GetLength(value.DowloadCompletedTorrentCollection);
        }
    }
}
