using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.FileOperates.FileOperateNodes;
using ExtenderApp.Data;


namespace ExtenderApp.Torrent.Models.Torrents.TorrentDowns
{
    internal class TorrentFileInfoNodeParentFormatter : FileOperateNodeParentForamtter<TorrentFileInfoNodeParent, TorrentFileInfoNode>
    {
        protected readonly IBinaryFormatter<byte[]> _byteArray;
        protected readonly IBinaryFormatter<long> _long;
        protected readonly IBinaryFormatter<HashValues> _hashValues;
        protected readonly IBinaryFormatter<InfoHash> _infoHash;

        public TorrentFileInfoNodeParentFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _byteArray = resolver.GetFormatter<byte[]>();
            _long = resolver.GetFormatter<long>();
            _hashValues = resolver.GetFormatter<HashValues>();
            _infoHash = resolver.GetFormatter<InfoHash>();
        }

        public override TorrentFileInfoNodeParent Deserialize(ref ExtenderBinaryReader reader)
        {
            var result = base.Deserialize(ref reader);
            result.TorrentFileInfo = _string.Deserialize(ref reader);
            result.PieceHashValues = _hashValues.Deserialize(ref reader);
            result.PieceLength = _long.Deserialize(ref reader);
            result.Hash = _infoHash.Deserialize(ref reader);
            return result;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, TorrentFileInfoNodeParent value)
        {
            base.Serialize(ref writer, value);
            _string.Serialize(ref writer, value.TorrentFileInfo);
            _hashValues.Serialize(ref writer, value.PieceHashValues);
            _long.Serialize(ref writer, value.PieceLength);
            _infoHash.Serialize(ref writer, value.Hash);
        }

        public override long GetLength(TorrentFileInfoNodeParent value)
        {
            return base.GetLength(value) + _string.GetLength(value.TorrentFileInfo);
        }
    }
}
