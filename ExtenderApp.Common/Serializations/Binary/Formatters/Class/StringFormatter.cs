using System.Text;
using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 字符串格式化器类
    /// </summary>
    /// <remarks>继承自 <see cref="BinaryFormatter{T}"/> 泛型类，专门用于对字符串类型的对象进行格式化。</remarks>
    internal class StringFormatter : ResolverFormatter<string>
    {
        private readonly IBinaryFormatter<int> _int;
        private readonly Encoding encoding = Encoding.UTF8;

        public StringFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
        }

        public override string Deserialize(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
            {
                return string.Empty;
            }

            if (!TryReadMark(ref buffer, BinaryOptions.String))
            {
                ThrowOperationException("无法反序列化为字符串类型，数据标记不匹配。");
            }

            int length = _int.Deserialize(ref buffer);
            if (length < 4096)
            {
                Span<byte> span = stackalloc byte[length];
                buffer.Read(span);
                Span<char> chars = stackalloc char[span.Length];
                int charLength;
                unsafe
                {
                    fixed (byte* pBytes = span)
                    fixed (char* pChars = chars)
                    {
                        charLength = encoding.GetChars(pBytes, span.Length, pChars, chars.Length);
                    }
                }
                return new string(chars.Slice(0, charLength));
            }

            return buffer.ReadString(length, encoding);
        }

        public override void Serialize(ref ByteBuffer buffer, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteNil(ref buffer);
                return;
            }

            buffer.Write(BinaryOptions.String);
            _int.Serialize(ref buffer, value.Length);
            buffer.Write(value, encoding);
        }

        public override long GetLength(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return DefaultLength;
            }

            long result = _int.GetLength(value.Length) + 1;
            result += encoding.GetByteCount(value);

            return result;
        }
    }
}