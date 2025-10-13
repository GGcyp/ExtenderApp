using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    internal class NilFormatter : StructFormatter<Nil>
    {
        public NilFormatter(ByteBufferConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
        }

        public override Nil Deserialize(ref ByteBuffer buffer)
        {
            return _bufferConvert.ReadNil(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, Nil value)
        {
            if (value.IsNil)
            {
                _bufferConvert.WriteNil(ref buffer);
            }
        }
    }
}
