using ExtenderApp.Abstract;
using ExtenderApp.Buffer;

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

        public override void Serialize(AbstractBuffer<byte> buffer, ByteBlock value)
        {
            if (value.IsEmpty)
            {
                WriteNil(buffer);
                return;
            }

            WriteArrayHeader(buffer);
            _int.Serialize(buffer, value.Committed);
            buffer.Write(value.CommittedSpan);
        }

        public override void Serialize(ref SpanWriter<byte> writer, ByteBlock value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref writer);
                return;
            }
            WriteArrayHeader(ref writer);
            _int.Serialize(ref writer, value.Committed);
            writer.Write(value);
        }

        public override ByteBlock Deserialize(AbstractBufferReader<byte> reader)
        {
            if (TryReadNil(reader))
            {
                return new();
            }

            if (!TryReadArrayHeader(reader))
            {
                ThrowOperationException("无法将当前数据反序列化为 ByteBlock 类型，数据格式不匹配。");
            }

            var length = _int.Deserialize(reader);
            ByteBlock block = new(length);
            var readLength = reader.Read(block.GetSpan(length).Slice(0, length));
            block.Advance(readLength);
            return block;
        }

        public override ByteBlock Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return new();
            }

            if (!TryReadArrayHeader(ref reader))
            {
                ThrowOperationException("无法将当前数据反序列化为 ByteBlock 类型，数据格式不匹配。");
            }

            var length = _int.Deserialize(ref reader);
            ByteBlock block = new(length);
            var readLength = reader.Read(block.GetSpan(length).Slice(0, length));
            block.Advance(readLength);
            return block;
        }

        public override long GetLength(ByteBlock value)
        {
            if (value.IsEmpty)
            {
                return NilLength;
            }
            return _int.GetLength(value.Committed) + 1 + value.Committed;
        }
    }
}