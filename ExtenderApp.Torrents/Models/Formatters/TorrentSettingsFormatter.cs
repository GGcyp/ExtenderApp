using ExtenderApp.Abstract;
using ExtenderApp.Common.Serializations.Binary.Formatters;
using ExtenderApp.Contracts;

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

        public override void Serialize(ref ByteBuffer buffer, TorrentSettingsBuilderModel value)
        {
            if (value is null)
            {
                WriteNil(ref buffer);
                return;
            }

            // 按属性顺序序列化
            _bool.Serialize(ref buffer, value.AllowDht);
            _bool.Serialize(ref buffer, value.AllowInitialSeeding);
            _bool.Serialize(ref buffer, value.AllowPeerExchange);
            _bool.Serialize(ref buffer, value.CreateContainingDirectory);
            _int.Serialize(ref buffer, value.MaximumConnections);
            _int.Serialize(ref buffer, value.MaximumDownloadRate);
            _int.Serialize(ref buffer, value.MaximumUploadRate);
            _bool.Serialize(ref buffer, value.RequirePeerIdToMatch);
            _int.Serialize(ref buffer, value.UploadSlots);
        }

        public override TorrentSettingsBuilderModel Deserialize(ref ByteBuffer buffer)
        {
            TorrentSettingsBuilderModel builder = new();
            if (TryReadNil(ref buffer))
            {
                return builder;
            }

            // 按属性顺序反序列化
            builder.AllowDht = _bool.Deserialize(ref buffer);
            builder.AllowInitialSeeding = _bool.Deserialize(ref buffer);
            builder.AllowPeerExchange = _bool.Deserialize(ref buffer);
            builder.CreateContainingDirectory = _bool.Deserialize(ref buffer);
            builder.MaximumConnections = _int.Deserialize(ref buffer);
            builder.MaximumDownloadRate = _int.Deserialize(ref buffer);
            builder.MaximumUploadRate = _int.Deserialize(ref buffer);
            builder.RequirePeerIdToMatch = _bool.Deserialize(ref buffer);
            builder.UploadSlots = _int.Deserialize(ref buffer);
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
