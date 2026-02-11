using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Common.IO.FileParsers;

namespace ExtenderApp.Common.Serializations.Binary
{
    /// <summary>
    /// 二进制解析器类
    /// </summary>
    internal class BinarySerialization : Serialization, IBinarySerialization
    {
        /// <summary>
        /// 二进制格式化器解析器
        /// </summary>
        private readonly IBinaryFormatterResolver _resolver;

        public BinarySerialization(IBinaryFormatterResolver binaryFormatterResolver)
        {
            _resolver = binaryFormatterResolver;
        }

        #region Serialize

        public override void Serialize<T>(T value, ref SpanWriter<byte> writer)
        {
            if (writer.UnwrittenSpan.IsEmpty)
                throw new ArgumentNullException(nameof(writer));

            var formatter = _resolver.GetFormatterWithVerify<T>();
            formatter.Serialize(ref writer, value);
        }

        public override byte[] Serialize<T>(T value)
        {
            Serialize(value, out AbstractBuffer<byte> buffer);
            var result = buffer.ToArray();
            buffer.TryRelease();
            return result;
        }

        public override void Serialize<T>(T value, AbstractBuffer<byte> buffer)
        {
            _resolver.GetFormatterWithVerify<T>().Serialize(buffer, value);
        }

        public override void Serialize<T>(T value, out AbstractBuffer<byte> buffer)
        {
            buffer = SequenceBufferProvider<byte>.Shared.GetBuffer();
            _resolver.GetFormatterWithVerify<T>().Serialize(buffer, value);
        }

        #endregion Serialize

        #region Deserialize

        public override T Deserialize<T>(ReadOnlySpan<byte> span)
        {
            if (TryGetFormatter(out IBinaryFormatter<T> formatter))
            {
                SpanReader<byte> reader = new(span);
                return formatter.Deserialize(ref reader);
            }
            return default!;
        }

        public override T Deserialize<T>(ref SpanReader<byte> reader)
        {
            if (TryGetFormatter(out IBinaryFormatter<T> formatter))
            {
                return formatter.Deserialize(ref reader);
            }
            return default!;
        }

        public override T Deserialize<T>(AbstractBuffer<byte> buffer)
        {
            if (TryGetFormatter(out IBinaryFormatter<T> formatter))
            {
                var arr = buffer.ToArray();
                SpanReader<byte> reader = new(arr);
                var result = formatter.Deserialize(ref reader);
                return result;
            }
            return default!;
        }

        public override T Deserialize<T>(AbstractBufferReader<byte> reader)
        {
            if (TryGetFormatter(out IBinaryFormatter<T> formatter))
            {
                return formatter.Deserialize(reader);
            }
            return default!;
        }

        #endregion Deserialize

        #region Count

        public long GetLength<T>(T value)
        {
            return _resolver.GetFormatterWithVerify<T>().GetLength(value);
        }

        public long GetDefaulLength<T>()
        {
            return _resolver.GetFormatterWithVerify<T>().DefaultLength;
        }

        public bool TryGetFormatter<T>(out IBinaryFormatter<T> formatter)
        {
            try
            {
                formatter = _resolver.GetFormatter<T>();
                return formatter != null;
            }
            catch
            {
                formatter = null!;
                return false;
            }
        }

        #endregion Count
    }
}