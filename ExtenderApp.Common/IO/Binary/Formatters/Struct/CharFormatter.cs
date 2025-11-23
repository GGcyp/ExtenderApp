using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// CharFormatter 类是一个内部类，继承自 StructFormatter<char>，用于格式化字符类型的数据。
    /// </summary>
    internal class CharFormatter : StructFormatter<char>
    {
        public CharFormatter(ByteBufferConvert bufferConvert, BinaryOptions options) : base(bufferConvert, options)
        {
        }

        public override char Deserialize(ref ByteBuffer buffer)
        {
            return _bufferConvert.ReadChar(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, char value)
        {
            _bufferConvert.Write(ref buffer, value);
        }

        public override long GetLength(char value)
        {
            return _bufferConvert.GetByteCount(value);
        }
    }
}
