using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 字节块格式化器。
    /// </summary>
    internal class ByteBlockFormatter : ResolverFormatter<ByteBlock>
    {
        private readonly IBinaryFormatter<int> _int;

        public ByteBlockFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
        }

        public override ByteBlock Deserialize(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
            {
                return new();
            }

            if (!TryReadArrayHeader(ref buffer))
            {
                ThrowOperationException("无法将当前数据反序列化为 ByteBlock 类型，数据格式不匹配。");
            }

            var length = _int.Deserialize(ref buffer);
            ByteBlock block = new(length);
            block.Write(buffer);
            buffer.ReadAdvance(length);
            return block;
        }

        public override void Serialize(ref ByteBuffer buffer, ByteBlock value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref buffer);
                return;
            }
            WriteArrayHeader(ref buffer);
            _int.Serialize(ref buffer, value.Remaining);
            buffer.Write(value);
        }

        public override long GetLength(ByteBlock value)
        {
            if (value.IsEmpty)
            {
                return GetNilLength();
            }
            return _int.GetLength(value.Remaining) + 1 + value.Remaining;
        }
    }
}