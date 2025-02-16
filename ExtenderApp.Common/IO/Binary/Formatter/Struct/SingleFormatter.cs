using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatter
{
    /// <summary>
    /// 浮点数格式化器类
    /// </summary>
    /// <remarks>
    /// 该类继承自<see cref="ExtenderFormatter{T}"/>泛型类，其中T指定为<see cref="Single"/>类型，用于对浮点数进行格式化处理。
    /// </remarks>
    internal class SingleFormatter : ExtenderFormatter<Single>
    {
        public override int Count => 5;

        public SingleFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override float Deserialize(ref ExtenderBinaryReader reader)
        {
            return _binaryReaderConvert.ReadSingle(ref reader);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, float value)
        {
            _binaryWriterConvert.Write(ref writer, value);
        }
    }
}
