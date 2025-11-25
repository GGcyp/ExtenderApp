using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 浮点数格式化器类
    /// </summary>
    /// <remarks>
    /// 该类继承自<see cref="BinaryFormatter{T}"/>泛型类，其中T指定为<see cref="Single"/>类型，用于对浮点数进行格式化处理。
    /// </remarks>
    internal class SingleFormatter : StructFormatter<Single>
    {
        public SingleFormatter(ByteBufferConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
        }

        public override float Deserialize(ref ByteBuffer buffer)
        {
            return _bufferConvert.ReadSingle(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, float value)
        {
            _bufferConvert.Write(ref buffer, value);
        }

        public override long GetLength(float value)
        {
            return _bufferConvert.GetByteCount(value);
        }
    }
}
