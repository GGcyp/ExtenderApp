using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// BooleanFormatter 类，继承自 StructFormatter<Bool> 类，用于格式化 Bool 类型的数据。
    /// </summary>
    internal sealed class BooleanFormatter : BinaryFormatter<Boolean>
    {
        public BooleanFormatter()
        {
        }

        public override bool Deserialize(ref ByteBuffer buffer)
        {
            byte value = buffer.Read();
            if (value == BinaryOptions.True)
            {
                return true;
            }
            else if (value == BinaryOptions.False)
            {
                return false;
            }
            throw new InvalidOperationException($"无法转换类型: {value}");
        }

        public override void Serialize(ref ByteBuffer buffer, bool value)
        {
            buffer.Write(value ? BinaryOptions.True : BinaryOptions.False);
        }

        public override long GetLength(bool value)
        {
            return DefaultLength;
        }
    }
}