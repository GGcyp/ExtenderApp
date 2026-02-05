using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    internal class ReadOnlyMemoryFormatter<T> : ResolverFormatter<ReadOnlyMemory<T>>
    {
        private readonly IBinaryFormatter<T> _t;
        private readonly IBinaryFormatter<int> _int;

        public ReadOnlyMemoryFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _t = GetFormatter<T>();
            _int = GetFormatter<int>();
        }

        public override ReadOnlyMemory<T> Deserialize(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
            {
                return Memory<T>.Empty;
            }

            if (!TryReadArrayHeader(ref buffer))
            {
                ThrowOperationException("无法将当前数据反序列化为 ReadOnlyMemory<TLinkClient> 类型。");
            }

            var length = _int.Deserialize(ref buffer);
            if (length == 0)
            {
                return Memory<T>.Empty;
            }
            var array = new T[length];
            for (var i = 0; i < length; i++)
            {
                array[i] = _t.Deserialize(ref buffer);
            }
            return new Memory<T>(array);
        }

        public override void Serialize(ref ByteBuffer buffer, ReadOnlyMemory<T> value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref buffer);
                return;
            }

            WriteArrayHeader(ref buffer);
            _int.Serialize(ref buffer, value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                _t.Serialize(ref buffer, value.Span[i]);
            }
        }

        public override long GetLength(ReadOnlyMemory<T> value)
        {
            if (value.IsEmpty)
                return NilLength;

            long length = _int.GetLength(value.Length) + 1;
            for (var i = 0; i < value.Length; i++)
            {
                length += _t.GetLength(value.Span[i]);
            }
            return length;
        }
    }
}