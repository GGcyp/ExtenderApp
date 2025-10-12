using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// CharFormatter 类是一个内部类，继承自 ExtenderFormatter<char>，用于格式化字符类型的数据。
    /// </summary>
    internal class CharFormatter : StructFormatter<char>
    {
        public CharFormatter(ByteBlockConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
        }

        public override char Deserialize(ref ByteBlock block)
        {
            return _blockConvert.ReadChar(ref block);
        }

        public override void Serialize(ref ByteBlock block, char value)
        {
            _blockConvert.Write(ref block, value);
        }
    }
}
