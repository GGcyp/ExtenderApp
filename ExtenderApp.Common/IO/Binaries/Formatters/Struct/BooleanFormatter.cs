using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// BooleanFormatter 类，继承自 ExtenderFormatter<Bool> 类，用于格式化 Bool 类型的数据。
    /// </summary>
    internal sealed class BooleanFormatter : StructFormatter<Boolean>
    {
        public override int DefaultLength => 1;

        public BooleanFormatter(ByteBlockConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
        }

        public override bool Deserialize(ref ByteBlock block)
        {
            return _blockConvert.ReadBoolean(ref block);
        }

        public override void Serialize(ref ByteBlock block, bool value)
        {
            _blockConvert.Write(ref block, value);
        }
    }
}
