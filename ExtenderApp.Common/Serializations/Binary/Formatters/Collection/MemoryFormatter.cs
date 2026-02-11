using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Buffer.Reader;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    internal class MemoryFormatter<T> : ResolverFormatter<Memory<T>>
    {
        private readonly IBinaryFormatter<T> _t;
        private readonly IBinaryFormatter<int> _int;

        public MemoryFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _t = GetFormatter<T>();
            _int = GetFormatter<int>();
        }

        public override Memory<T> Deserialize(AbstractBufferReader<byte> reader)
        {
            if (TryReadNil(reader))
            {
                return Memory<T>.Empty;
            }

            if (!TryReadArrayHeader(reader))
            {
                ThrowOperationException("无法将当前数据反序列化为 Memory<T> 类型的值，数据格式不匹配。");
            }

            var length = _int.Deserialize(reader);
            if (length == 0)
            {
                return Memory<T>.Empty;
            }

            var array = new T[length];
            for (var i = 0; i < length; i++)
            {
                array[i] = _t.Deserialize(reader);
            }
            return new Memory<T>(array);
        }

        public override Memory<T> Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return Memory<T>.Empty;
            }

            if (!TryReadArrayHeader(ref reader))
            {
                ThrowOperationException("无法将当前数据反序列化为 Memory<T> 类型的值，数据格式不匹配。");
            }

            var length = _int.Deserialize(ref reader);
            if (length == 0)
            {
                return Memory<T>.Empty;
            }

            var array = new T[length];
            for (var i = 0; i < length; i++)
            {
                array[i] = _t.Deserialize(ref reader);
            }
            return new Memory<T>(array);
        }

        public override void Serialize(AbstractBuffer<byte> buffer, Memory<T> value)
        {
            if (value.IsEmpty)
            {
                WriteNil(buffer);
                return;
            }

            WriteArrayHeader(buffer);
            _int.Serialize(buffer, value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                _t.Serialize(buffer, value.Span[i]);
            }
        }

        public override void Serialize(ref SpanWriter<byte> writer, Memory<T> value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref writer);
                return;
            }

            WriteArrayHeader(ref writer);
            _int.Serialize(ref writer, value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                _t.Serialize(ref writer, value.Span[i]);
            }
        }

        public override long GetLength(Memory<T> value)
        {
            if (value.IsEmpty)
                return NilLength;

            long length = 1 + _int.DefaultLength; // array header + length field (approx)
            for (var i = 0; i < value.Length; i++)
            {
                length += _t.GetLength(value.Span[i]);
            }

            return length;
        }
    }
}