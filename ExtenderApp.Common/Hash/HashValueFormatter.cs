using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Hash
{
    /// <summary>
    /// 哈希值格式化类
    /// </summary>
    internal class HashValueFormatter : ResolverFormatter<HashValue>
    {
        /// <summary>
        /// 字符串格式化器
        /// </summary>
        private readonly IBinaryFormatter<string> _string;

        /// <summary>
        /// 获取格式化后的长度
        /// </summary>
        public override int Length => _string.Length;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="resolver">二进制格式化器解析器</param>
        public HashValueFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _string = GetFormatter<string>();
        }

        /// <summary>
        /// 反序列化哈希值
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        /// <returns>反序列化后的哈希值</returns>
        public override HashValue Deserialize(ref ExtenderBinaryReader reader)
        {
            var result = _string.Deserialize(ref reader);
            return HashValue.FromHexString(result);
        }

        /// <summary>
        /// 序列化哈希值
        /// </summary>
        /// <param name="writer">二进制写入器</param>
        /// <param name="value">要序列化的哈希值</param>
        public override void Serialize(ref ExtenderBinaryWriter writer, HashValue value)
        {
            _string.Serialize(ref writer, value.ToHexString());
        }

        /// <summary>
        /// 获取序列化后的长度
        /// </summary>
        /// <param name="value">要序列化的哈希值</param>
        /// <returns>序列化后的长度</returns>
        public override long GetLength(HashValue value)
        {
            return _string.GetLength(value.ToHexString());
        }
    }
}
