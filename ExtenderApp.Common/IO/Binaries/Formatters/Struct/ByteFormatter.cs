using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// ByteFormatter 类，继承自 ExtenderFormatter<Bool> 类，用于格式化 Bool 类型的数据。
    /// </summary>
    internal sealed class ByteFormatter : StructFormatter<Byte>
    {
        public ByteFormatter(ByteBlockConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
        }

        public override byte Deserialize(ref ByteBlock block)
        {
            return _blockConvert.ReadByte(ref block);
        }

        public override void Serialize(ref ByteBlock block, byte value)
        {
            _blockConvert.Write(ref block, value);
        }

        public override long GetLength(byte value)
        {
            if (value < _binaryOptions.BinaryRang.MaxFixPositiveInt)
            {
                return 1;
            }
            return 2;
        }
    }
}
