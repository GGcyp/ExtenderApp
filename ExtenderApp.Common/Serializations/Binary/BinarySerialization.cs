using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.FileParsers;
using ExtenderApp.Data;

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

        public override void Serialize<T>(T value, Span<byte> span)
        {
            if (span.IsEmpty)
                throw new ArgumentNullException(nameof(span));

            Serialize(value, out ByteBuffer buffer);
            if (buffer.Remaining > span.Length)
                throw new ArgumentException("数组内存空间不足", nameof(span));

            buffer.TryCopyTo(span);
            buffer.Dispose();
        }

        public override byte[] Serialize<T>(T value)
        {
            Serialize(value, out ByteBuffer buffer);
            var result = buffer.ToArray();
            buffer.Dispose();
            return result;
        }

        public override void Serialize<T>(T value, out ByteBuffer buffer)
        {
            buffer = new ();
            _resolver.GetFormatterWithVerify<T>().Serialize(ref buffer, value);
        }

        #endregion Serialize

        #region Deserialize

        public override T? Deserialize<T>(ReadOnlySpan<byte> span) where T : default
        {
            if (TryGetFormatter(out IBinaryFormatter<T> formatter))
            {
                ByteBuffer buffer = new ();
                buffer.Write(span);
                T result = formatter.Deserialize(ref buffer);
                buffer.Dispose();
                return result;
            }
            return default;
        }

        public override T? Deserialize<T>(ReadOnlyMemory<byte> memory) where T : default
        {
            return Deserialize<T>(new ReadOnlySequence<byte>(memory));
        }

        public override T? Deserialize<T>(ReadOnlySequence<byte> memories) where T : default
        {
            if (TryGetFormatter(out IBinaryFormatter<T> formatter))
            {
                ByteBuffer buffer = new(memories);
                T result = formatter.Deserialize(ref buffer);
                buffer.Dispose();
                return result;
            }
            return default;
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