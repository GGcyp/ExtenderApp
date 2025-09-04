﻿using System.Collections.ObjectModel;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatters;
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

        public override TorrentModel Deserialize(ref ExtenderBinaryReader reader)
        {
            TorrentModel model = new TorrentModel();
            if (TryReadNil(ref reader))
            {
                return model;
            }

            model.SaveDirectory = _string.Deserialize(ref reader);
            model.EngineSettingsModel = _engineSettings.Deserialize(ref reader);
            model.TorrentSettingsModel = _torrentSettings.Deserialize(ref reader);
            model.DowloadTorrentCollection = _torrentInfos.Deserialize(ref reader);
            model.DowloadCompletedTorrentCollection = _torrentInfos.Deserialize(ref reader);
            model.RecycleBinCollection = _torrentInfos.Deserialize(ref reader);
            model.InfoHashHashSet = _hash.Deserialize(ref reader);
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
            _engineSettings.Serialize(ref writer, value.EngineSettingsModel);
            _torrentSettings.Serialize(ref writer, value.TorrentSettingsModel);
            _torrentInfos.Serialize(ref writer, value.DowloadTorrentCollection);
            _torrentInfos.Serialize(ref writer, value.DowloadCompletedTorrentCollection);
            _torrentInfos.Serialize(ref writer, value.RecycleBinCollection);
            _hash.Serialize(ref writer, value.InfoHashHashSet);
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
