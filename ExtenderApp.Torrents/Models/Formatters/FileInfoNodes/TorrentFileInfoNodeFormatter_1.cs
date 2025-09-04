using ExtenderApp.Abstract;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.IO;
using ExtenderApp.Common.IO.Binaries;
using ExtenderApp.Data;
using MonoTorrent;

namespace ExtenderApp.Torrents.Models
{
    internal class TorrentFileInfoNodeFormatter_1 : FileNodeFormatter<TorrentFileInfoNode>, IVersionDataFormatter<TorrentFileInfoNode>
    {
        protected readonly IBinaryFormatter<int> _int;

        public Version FormatterVersion { get; } = TorrentFormatterVersion.Version_1;

        public TorrentFileInfoNodeFormatter_1(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(resolver, binaryWriterConvert, binaryReaderConvert, options)
        {
            _int = resolver.GetFormatter<int>();
        }

        protected override TorrentFileInfoNode ProtectedDeserialize(ref ExtenderBinaryReader reader)
        {
            var node = base.ProtectedDeserialize(ref reader);
            node.Depth = _int.Deserialize(ref reader);
            node.Priority = (Priority)_int.Deserialize(ref reader);
            node.NeedDownloading = _bool.Deserialize(ref reader);
            node.DisplayNeedDownload = _bool.Deserialize(ref reader);
            return node;
        }

        protected override void ProtectedSerialize(ref ExtenderBinaryWriter writer, TorrentFileInfoNode value)
        {
            base.ProtectedSerialize(ref writer, value);
            _int.Serialize(ref writer, value.Depth);
            _int.Serialize(ref writer, (int)value.Priority);
            _bool.Serialize(ref writer, value.NeedDownloading);
            _bool.Serialize(ref writer, value.DisplayNeedDownload);
        }

        protected override void ProtectedGetLength(TorrentFileInfoNode value, DataBuffer<long> dataBuffer)
        {
            base.ProtectedGetLength(value, dataBuffer);
            dataBuffer.Item1 += _int.DefaultLength * 2;
            dataBuffer.Item1 += _bool.DefaultLength * 2;
        }
    }
}
