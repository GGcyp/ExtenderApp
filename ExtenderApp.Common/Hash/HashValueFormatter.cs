﻿using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
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
        public override int Length => _ulongs.Length;

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
                _ulongs.Serialize(ref writer, ReadOnlyMemory<ulong>.Empty);
                return;
            }

            _ulongs.Serialize(ref writer, value.HashMemory);
            _int.Serialize(ref writer, value.Length);
        }

        /// <summary>
        /// 反序列化哈希值
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        /// <returns>反序列化后的哈希值</returns>
        public override HashValue Deserialize(ref ExtenderBinaryReader reader)
        {
            var memory = _ulongs.Deserialize(ref reader);
            if (memory.IsEmpty)
            {
                return HashValue.Empty;
            }

            var length = _int.Deserialize(ref reader);
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
