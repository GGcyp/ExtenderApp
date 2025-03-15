using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Splitter
{
    /// <summary>
    /// 文件分割信息格式化器类
    /// </summary>
    internal class SplitterInfoFormatter : ResolverFormatter<SplitterInfo>
    {
        /// <summary>
        /// 用于反序列化 long 类型的格式化器
        /// </summary>
        private readonly IBinaryFormatter<long> _long;

        /// <summary>
        /// 用于反序列化 uint 类型的格式化器
        /// </summary>
        private readonly IBinaryFormatter<uint> _uint;

        /// <summary>
        /// 用于反序列化 int 类型的格式化器
        /// </summary>
        private readonly IBinaryFormatter<int> _int;

        /// <summary>
        /// 用于反序列化 byte[] 类型的格式化器
        /// </summary>
        private readonly IBinaryFormatter<byte[]> _bytes;

        /// <summary>
        /// 用于反序列化 string 类型的格式化器
        /// </summary>
        private readonly IBinaryFormatter<string> _string;
        public override int Length => _uint.Length * 2 + _long.Length + _int.Length + _string.Length * 2 + _bytes.Length;

        /// <summary>
        /// 初始化 FileSplitterInfoFormatter 实例
        /// </summary>
        /// <param name="resolver">格式化器解析器</param>
        public SplitterInfoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _long = GetFormatter<long>();
            _uint = GetFormatter<uint>();
            _int = GetFormatter<int>();
            _bytes = GetFormatter<byte[]>();
            _string = GetFormatter<string>();
        }

        /// <summary>
        /// 从 ExtenderBinaryReader 中反序列化 FileSplitterInfo 对象
        /// </summary>
        /// <param name="reader">ExtenderBinaryReader 对象</param>
        /// <returns>反序列化后的 FileSplitterInfo 对象</returns>
        public override SplitterInfo Deserialize(ref ExtenderBinaryReader reader)
        {
            long length = _long.Deserialize(ref reader);
            uint chunkCount = _uint.Deserialize(ref reader);
            uint progresst = _uint.Deserialize(ref reader);
            int maxChunkSize = _int.Deserialize(ref reader);
            string targetExtensions = _string.Deserialize(ref reader);
            string fileMD5HASH = _string.Deserialize(ref reader);
            byte[] bytes = _bytes.Deserialize(ref reader);

            return new SplitterInfo(length, chunkCount, progresst, maxChunkSize, targetExtensions, fileMD5HASH, bytes);
        }

        /// <summary>
        /// 将 FileSplitterInfo 对象序列化到 ExtenderBinaryWriter 中
        /// </summary>
        /// <param name="writer">ExtenderBinaryWriter 对象</param>
        /// <param name="value">要序列化的 FileSplitterInfo 对象</param>
        public override void Serialize(ref ExtenderBinaryWriter writer, SplitterInfo value)
        {
            _long.Serialize(ref writer, value.Length);
            _uint.Serialize(ref writer, value.ChunkCount);
            _uint.Serialize(ref writer, value.Progress);
            _int.Serialize(ref writer, value.MaxChunkSize);
            _string.Serialize(ref writer, value.TargetExtensions);
            _string.Serialize(ref writer, value.FileMD5);
            _bytes.Serialize(ref writer, value.LoadedChunks);
        }

        public override long GetLength(SplitterInfo value)
        {
            if (value == null)
            {
                return _uint.Length * 2 + _long.Length + _int.Length + _string.Length * 2 + _bytes.Length;
            }

            long result = _uint.Length * 2 + _long.Length + _int.Length;
            result += _string.GetLength(value.TargetExtensions);
            result += _string.GetLength(value.FileMD5);
            result += _bytes.GetLength(value.LoadedChunks);
            return result;
        }
    }
}
