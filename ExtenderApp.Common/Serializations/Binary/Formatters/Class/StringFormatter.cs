using System.Text;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Buffer.Reader;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 字符串格式化器类
    /// </summary>
    /// <remarks>继承自 <see cref="ResolverFormatter{T}"/>，用于序列化/反序列化字符串。</remarks>
    internal class StringFormatter : ResolverFormatter<string>
    {
        private const int MinStackallocLength = 2048;
        private readonly IBinaryFormatter<int> _int;
        private readonly Encoding encoding;

        public StringFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
            encoding = Encoding.UTF8;
        }

        public override string Deserialize(AbstractBufferReader<byte> reader)
        {
            if (TryReadNil(reader))
            {
                return string.Empty;
            }

            if (!TryReadMark(reader, BinaryOptions.String))
            {
                ThrowOperationException("无法反序列化为字符串类型，数据标记不匹配。");
            }

            int length = _int.Deserialize(reader);
            if (length > reader.Remaining)
                throw new InvalidOperationException("数据长度超过剩余数据长度，无法反序列化为字符串类型。");

            string result = string.Empty;
            if (reader is MemoryBlockReader<byte> memoryBlockReader)
            {
                result = encoding.GetString(memoryBlockReader.UnreadSpan.Slice(0, length));
                memoryBlockReader.Advance(length);
                return result;
            }

            Span<byte> span = stackalloc byte[MinStackallocLength];
            if (length < MinStackallocLength)
            {
                reader.Read(span.Slice(0, length));
                return encoding.GetString(span.Slice(0, length));
            }

            var decoder = encoding.GetDecoder();
            int remaining = length;
            MemoryBlock<char> charMemoryBlock = MemoryBlock<char>.GetBuffer(encoding.GetMaxCharCount(length));
            while (remaining > 0)
            {
                int read = System.Math.Min(remaining, span.Length);
                reader.Read(span.Slice(0, read));
                decoder.Convert(span.Slice(0, read), charMemoryBlock.RemainingSpan, remaining == read, out int bytesUsed, out int charsUsed, out _);
                charMemoryBlock.Advance(charsUsed);
                remaining -= bytesUsed;
            }

            result = new(charMemoryBlock.CommittedSpan);
            charMemoryBlock.TryRelease();
            return result;
        }

        public override string Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return string.Empty;
            }

            if (!TryReadMark(ref reader, BinaryOptions.String))
            {
                ThrowOperationException("无法反序列化为字符串类型，数据标记不匹配。");
            }

            int length = _int.Deserialize(ref reader);
            if (length > reader.Remaining)
                throw new InvalidOperationException("数据长度超过剩余数据长度，无法反序列化为字符串类型。");

            string result = encoding.GetString(reader.UnreadSpan.Slice(0, length));
            reader.Advance(length);
            return result;
        }

        public override void Serialize(AbstractBuffer<byte> buffer, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteNil(buffer);
                return;
            }

            int byteCount = encoding.GetByteCount(value);

            buffer.Write(BinaryOptions.String);
            _int.Serialize(buffer, byteCount);
            var span = buffer.GetSpan(byteCount);
            byteCount = encoding.GetBytes(value, span);
            buffer.Advance(byteCount);
        }

        public override void Serialize(ref SpanWriter<byte> writer, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteNil(ref writer);
                return;
            }

            int byteCount = encoding.GetByteCount(value);

            writer.Write(BinaryOptions.String);
            _int.Serialize(ref writer, byteCount);
            encoding.GetBytes(value, writer.UnwrittenSpan.Slice(0, byteCount));
            writer.Advance(byteCount);
        }

        public override long GetLength(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return DefaultLength;
            }

            int byteCount = encoding.GetByteCount(value);
            long result = _int.GetLength(byteCount) + 1;
            result += byteCount;

            return result;
        }
    }
}