using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Torrents.Models
{
    internal class TorrentSettingsFormatter : ResolverFormatter<TorrentSettingsBuilderModel>
    {
        public override int DefaultLength => 1;

        // 获取基础类型格式化器
        private readonly IBinaryFormatter<bool> _bool;
        private readonly IBinaryFormatter<int> _int;

        public TorrentSettingsFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _bool = GetFormatter<bool>();
            _int = GetFormatter<int>();
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, TorrentSettingsBuilderModel value)
        {
            if (value is null)
            {
                WriteNil(ref writer);
                return;
            }

            // 按属性顺序序列化
            _bool.Serialize(ref writer, value.AllowDht);
            _bool.Serialize(ref writer, value.AllowInitialSeeding);
            _bool.Serialize(ref writer, value.AllowPeerExchange);
            _bool.Serialize(ref writer, value.CreateContainingDirectory);
            _int.Serialize(ref writer, value.MaximumConnections);
            _int.Serialize(ref writer, value.MaximumDownloadRate);
            _int.Serialize(ref writer, value.MaximumUploadRate);
            _bool.Serialize(ref writer, value.RequirePeerIdToMatch);
            _int.Serialize(ref writer, value.UploadSlots);
        }

        public override TorrentSettingsBuilderModel Deserialize(ref ExtenderBinaryReader reader)
        {
            TorrentSettingsBuilderModel builder = new();
            if (TryReadNil(ref reader))
            {
                return builder;
            }

            // 按属性顺序反序列化
            builder.AllowDht = _bool.Deserialize(ref reader);
            builder.AllowInitialSeeding = _bool.Deserialize(ref reader);
            builder.AllowPeerExchange = _bool.Deserialize(ref reader);
            builder.CreateContainingDirectory = _bool.Deserialize(ref reader);
            builder.MaximumConnections = _int.Deserialize(ref reader);
            builder.MaximumDownloadRate = _int.Deserialize(ref reader);
            builder.MaximumUploadRate = _int.Deserialize(ref reader);
            builder.RequirePeerIdToMatch = _bool.Deserialize(ref reader);
            builder.UploadSlots = _int.Deserialize(ref reader);
            return builder;
        }

        public override long GetLength(TorrentSettingsBuilderModel value)
        {
            if (value == null)
            {
                return 1;
            }

            return _bool.DefaultLength * 5 + _int.DefaultLength * 4;
        }
    }
}
