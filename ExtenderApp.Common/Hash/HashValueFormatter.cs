using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Hash
{
    /// <summary>
    /// 哈希值格式化类
    /// </summary>
    internal class HashValueFormatter : ResolverFormatter<HashValue>
    {
        private readonly IBinaryFormatter<ReadOnlyMemory<ulong>> _ulongs;
        private readonly IBinaryFormatter<int> _int;

        /// <summary>
        /// 获取格式化后的长度
        /// </summary>
        public override int DefaultLength => _ulongs.DefaultLength;

        public HashValueFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _ulongs = GetFormatter<ReadOnlyMemory<ulong>>();
            _int = GetFormatter<int>();
        }

        /// <summary>
        /// 序列化哈希值
        /// </summary>
        /// <param name="writer">二进制写入器</param>
        /// <param name="value">要序列化的哈希值</param>
        public override void Serialize(ref ExtenderBinaryWriter writer, HashValue value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref writer);
                return;
            }

            _int.Serialize(ref writer, value.Length);
            _ulongs.Serialize(ref writer, value.HashMemory);
        }

        /// <summary>
        /// 反序列化哈希值
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        /// <returns>反序列化后的哈希值</returns>
        public override HashValue Deserialize(ref ExtenderBinaryReader reader)
        {
            if (TryReadNil(ref reader))
            {
                return HashValue.SHA1Empty;
            }

            var length = _int.Deserialize(ref reader);
            var memory = _ulongs.Deserialize(ref reader);
            return new HashValue(memory, length);
        }

        /// <summary>
        /// 获取序列化后的长度
        /// </summary>
        /// <param name="value">要序列化的哈希值</param>
        /// <returns>序列化后的长度</returns>
        public override long GetLength(HashValue value)
        {
            return value.IsEmpty ? 0 : value.Length + 5;
        }
    }
}
