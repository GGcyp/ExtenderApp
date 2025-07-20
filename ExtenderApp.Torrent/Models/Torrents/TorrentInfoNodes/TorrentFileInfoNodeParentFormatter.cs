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
        protected readonly IBinaryFormatter<BitFieldData> _bitFieldData;

        public TorrentFileInfoNodeParentFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _byteArray = resolver.GetFormatter<byte[]>();
            _long = resolver.GetFormatter<long>();
            _hashValues = resolver.GetFormatter<HashValues>();
            _infoHash = resolver.GetFormatter<InfoHash>();
            _bitFieldData = resolver.GetFormatter<BitFieldData>();
        }

        public override TorrentFileInfoNodeParent Deserialize(ref ExtenderBinaryReader reader)
        {
            var result = base.Deserialize(ref reader);
            result.TorrentFileInfo = _string.Deserialize(ref reader);
            result.PieceHashValues = _hashValues.Deserialize(ref reader);
            result.PieceLength = _long.Deserialize(ref reader);
            result.FileLength = _long.Deserialize(ref reader);
            result.Uploaded = _long.Deserialize(ref reader);
            result.Downloaded = _long.Deserialize(ref reader);
            result.Left = _long.Deserialize(ref reader);
            result.Hash = _infoHash.Deserialize(ref reader);
            result.LocalBiteField = _bitFieldData.Deserialize(ref reader);
            return result;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, TorrentFileInfoNodeParent value)
        {
            base.Serialize(ref writer, value);
            _string.Serialize(ref writer, value.TorrentFileInfo);
            _hashValues.Serialize(ref writer, value.PieceHashValues);
            _long.Serialize(ref writer, value.PieceLength);
            _long.Serialize(ref writer, value.FileLength);
            _long.Serialize(ref writer, value.Uploaded);
            _long.Serialize(ref writer, value.Downloaded);
            _long.Serialize(ref writer, value.Left);
            _infoHash.Serialize(ref writer, value.Hash);
            _bitFieldData.Serialize(ref writer, value.LocalBiteField);
        }

        public override long GetLength(TorrentFileInfoNodeParent value)
        {
            return base.GetLength(value)
                + _string.GetLength(value.TorrentFileInfo)
                + _hashValues.GetLength(value.PieceHashValues)
                + _long.Length * 5
                + _infoHash.GetLength(value.Hash)
                + _bitFieldData.GetLength(value.LocalBiteField);
        }
    }
}
