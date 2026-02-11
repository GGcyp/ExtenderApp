using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary> BooleanFormatter 类，继承自 StructFormatter<Bool> 类，用于格式化 Bool 类型的数据。 </summary>
    internal sealed class BooleanFormatter : BinaryFormatter<Boolean>
    {
        public BooleanFormatter()
        {
        }

        public override void Serialize(AbstractBuffer<byte> buffer, bool value)
        {
            buffer.Write(value ? BinaryOptions.True : BinaryOptions.False);
        }

        public override void Serialize(ref SpanWriter<byte> writer, bool value)
        {
            writer.Write(value ? BinaryOptions.True : BinaryOptions.False);
        }

        public override bool Deserialize(AbstractBufferReader<byte> reader)
        {
            byte value = reader.Read();
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

        public override bool Deserialize(ref SpanReader<byte> reader)
        {
            byte value = reader.Read();
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

        public override long GetLength(bool value)
        {
            return NilLength;
        }
    }
}