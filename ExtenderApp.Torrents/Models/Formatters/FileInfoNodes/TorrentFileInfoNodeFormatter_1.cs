using ExtenderApp.Abstract;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.IO;
using ExtenderApp.Common.IO.Binary;
using ExtenderApp.Data;
using MonoTorrent;

namespace ExtenderApp.Torrents.Models
{
    internal class TorrentFileInfoNodeFormatter_1 : FileNodeFormatter<TorrentFileInfoNode>, IVersionDataFormatter<TorrentFileInfoNode>
    {
        protected readonly IBinaryFormatter<int> _int;

        public Version FormatterVersion { get; } = TorrentFormatterVersion.Version_1;


        public TorrentFileInfoNodeFormatter_1(IBinaryFormatterResolver resolver, ByteBufferConvert convert, BinaryOptions options) : base(resolver, convert, options)
        {
            _int = resolver.GetFormatter<int>();
        }


        protected override TorrentFileInfoNode ProtectedDeserialize(ref ByteBuffer buffer)
        {
            var node = base.ProtectedDeserialize(ref buffer);
            node.Depth = _int.Deserialize(ref buffer);
            node.Priority = (Priority)_int.Deserialize(ref buffer);
            node.NeedDownloading = _bool.Deserialize(ref buffer);
            node.DisplayNeedDownload = _bool.Deserialize(ref buffer);
            return node;
        }

        protected override void ProtectedSerialize(ref ByteBuffer buffer, TorrentFileInfoNode value)
        {
            base.ProtectedSerialize(ref buffer, value);
            _int.Serialize(ref buffer, value.Depth);
            _int.Serialize(ref buffer, (int)value.Priority);
            _bool.Serialize(ref buffer, value.NeedDownloading);
            _bool.Serialize(ref buffer, value.DisplayNeedDownload);
        }

        protected override void ProtectedGetLength(TorrentFileInfoNode value, DataBuffer<long> dataBuffer)
        {
            base.ProtectedGetLength(value, dataBuffer);
            dataBuffer.Item1 += _int.DefaultLength * 2;
            dataBuffer.Item1 += _bool.DefaultLength * 2;
        }
    }
}
