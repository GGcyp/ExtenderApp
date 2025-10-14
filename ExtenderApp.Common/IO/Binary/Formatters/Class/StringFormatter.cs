using ExtenderApp.Data;
using Microsoft.VisualBasic;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 字符串格式化器类
    /// </summary>
    /// <remarks>
    /// 继承自<see cref="BinaryFormatter{T}"/>泛型类，专门用于对字符串类型的对象进行格式化。
    /// </remarks>
    internal class StringFormatter : BinaryFormatter<string>
    {
        public StringFormatter(ByteBufferConvert convert, BinaryOptions options) : base(convert, options)
        {
        }

        public override int DefaultLength => 5;

        public override long GetLength(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return DefaultLength;
            }
            return _binaryOptions.BinaryEncoding.GetMaxByteCount(value.Length) + DefaultLength;
            //return value.Length + Length;
        }

        public override string Deserialize(ref ByteBuffer buffer)
        {
            if (_bufferConvert.TryReadStringSpan(ref buffer, out ReadOnlySpan<byte> span))
            {
                if (span.Length < 4096)
                {
                    if (span.Length == 0)
                    {
                        return string.Empty;
                    }

                    Span<char> chars = stackalloc char[span.Length];
                    int charLength;
                    unsafe
                    {
                        fixed (byte* pBytes = span)
                        fixed (char* pChars = chars)
                        {
                            charLength = _bufferConvert.BinaryEncoding.GetChars(pBytes, span.Length, pChars, chars.Length);
                        }
                    }
                    return new string(chars.Slice(0, charLength));
                }
            }
            else
            {
                return string.Empty;
            }
            return _bufferConvert.UTF8ToString(span);
        }

        public override void Serialize(ref ByteBuffer buffer, string value)
        {
            _bufferConvert.Write(ref buffer, value);
        }
    }
}
