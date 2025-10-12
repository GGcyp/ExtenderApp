using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Splitter
{
    /// <summary>
    /// 文件分割信息格式化器类
    /// </summary>
    internal class SplitterInfoFormatter : ResolverFormatter<SplitterInfo>
    {
        /// <summary>
        /// 用于反序列化 uint 类型的格式化器
        /// </summary>
        private readonly IBinaryFormatter<uint> _uint;

        /// <summary>
        /// 用于反序列化 int 类型的格式化器
        /// </summary>
        private readonly IBinaryFormatter<int> _int;

        /// <summary>
        /// 用于反序列化 string 类型的格式化器
        /// </summary>
        private readonly IBinaryFormatter<string> _string;

        /// <summary>
        /// 用于处理哈希值的二进制格式化器
        /// </summary>
        private readonly IBinaryFormatter<HashValue> _hash;

        /// <summary>
        /// 用于序列化和反序列化 PieceData 的二进制格式化器。
        /// </summary>
        private readonly IBinaryFormatter<PieceData> _pieceData;

        public override int DefaultLength => _uint.DefaultLength * 2 + _pieceData.DefaultLength + _int.DefaultLength * 2 + _string.DefaultLength + _hash.DefaultLength;

        /// <summary>
        /// 初始化 FileSplitterInfoFormatter 实例
        /// </summary>
        /// <param name="resolver">格式化器解析器</param>
        public SplitterInfoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _uint = GetFormatter<uint>();
            _int = GetFormatter<int>();
            _string = GetFormatter<string>();
            _hash = GetFormatter<HashValue>();
            _pieceData = GetFormatter<PieceData>();
        }

        /// <summary>
        /// 从 ExtenderBinaryReader 中反序列化 FileSplitterInfo 对象
        /// </summary>
        /// <param name="reader">ExtenderBinaryReader 对象</param>
        /// <returns>反序列化后的 FileSplitterInfo 对象</returns>
        public override SplitterInfo Deserialize(ref ExtenderBinaryReader reader)
        {
            int length = _int.Deserialize(ref reader);
            uint chunkCount = _uint.Deserialize(ref reader);
            uint progresst = _uint.Deserialize(ref reader);
            int maxChunkSize = _int.Deserialize(ref reader);
            string targetExtensions = _string.Deserialize(ref reader);
            string fileMD5HASH = _string.Deserialize(ref reader);
            PieceData pieceData = _pieceData.Deserialize(ref reader);
            HashValue fileHashValue = _hash.Deserialize(ref reader);

            return new SplitterInfo(length, chunkCount, progresst, maxChunkSize, targetExtensions, fileHashValue, pieceData);
        }

        /// <summary>
        /// 将 FileSplitterInfo 对象序列化到 ExtenderBinaryWriter 中
        /// </summary>
        /// <param name="writer">ExtenderBinaryWriter 对象</param>
        /// <param name="value">要序列化的 FileSplitterInfo 对象</param>
        public override void Serialize(ref ExtenderBinaryWriter writer, SplitterInfo value)
        {
            _int.Serialize(ref writer, value.Length);
            _uint.Serialize(ref writer, value.ChunkCount);
            _uint.Serialize(ref writer, value.Progress);
            _int.Serialize(ref writer, value.MaxChunkSize);
            _string.Serialize(ref writer, value.TargetExtensions);
            _hash.Serialize(ref writer, value.HashValue);
            _pieceData.Serialize(ref writer, value.pieceData);
        }

        public override long GetLength(SplitterInfo value)
        {
            if (value == null)
            {
                return _uint.DefaultLength * 2 + _int.DefaultLength * 2 + _string.DefaultLength * 2 + _pieceData.DefaultLength;
            }

            long result = _uint.DefaultLength * 2 + _int.DefaultLength * 2;
            result += _string.GetLength(value.TargetExtensions);
            result += _hash.GetLength(value.HashValue);
            result += _pieceData.GetLength(value.pieceData);
            return result;
        }
    }
}
