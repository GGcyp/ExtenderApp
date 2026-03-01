using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Buffer.ValueBuffers;
using ExtenderApp.Common.IO.FileParsers;

namespace ExtenderApp.Common.Serializations.Binary
{
    /// <summary>
    /// 二进制解析器类
    /// </summary>
    internal sealed class BinarySerialization : Serialization, IBinarySerialization
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

        public override sealed void Serialize<T>(ref SpanWriter<byte> writer, T value)
        {
            if (writer.UnwrittenSpan.IsEmpty)
                throw new ArgumentNullException(nameof(writer));

            if (TryGetFormatter(out IBinaryFormatter<T> formatter))
            {
                formatter.Serialize(ref writer, value);
            }
        }

        public override sealed byte[] Serialize<T>(T value)
        {
            Serialize(value, out var buffer);
            var result = buffer.ToArray();
            buffer.TryRelease();
            return result;
        }

        public override sealed void Serialize<T>(ref BinaryWriterAdapter writer, T value)
        {
            if (writer.IsEmpty)
                throw new ArgumentNullException(nameof(writer));

            if (TryGetFormatter(out IBinaryFormatter<T> formatter))
            {
                formatter.Serialize(ref writer, value);
            }
        }

        public override sealed void Serialize<T>(T value, out AbstractBuffer<byte> buffer)
        {
            var sequence = FastSequence<byte>.GetBuffer();
            BinaryWriterAdapter writer = new(sequence);
            Serialize(ref writer, value);
            buffer = sequence.ToBuffer();
            sequence.TryRelease();
        }

        #endregion Serialize

        #region Deserialize

        public override sealed T Deserialize<T>(ref SpanReader<byte> reader)
        {
            if (TryGetFormatter(out IBinaryFormatter<T> formatter))
            {
                return formatter.Deserialize(ref reader);
            }
            return default!;
        }

        public override sealed T? Deserialize<T>(ref BinaryReaderAdapter reader) where T : default
        {
            if (TryGetFormatter(out IBinaryFormatter<T> formatter))
            {
                return formatter.Deserialize(ref reader);
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