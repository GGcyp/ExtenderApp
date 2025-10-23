using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class BinaryClientFormatter<T> : ClientFormatter<T>
    {
        private readonly IBinaryFormatter<T> _formatter;

        public BinaryClientFormatter(IByteBufferFactory factory, IBinaryFormatter<T> formatter) : base(factory)
        {
            _formatter = formatter;
        }

        protected override T Deserialize(ByteBuffer buffer)
        {
            return _formatter.Deserialize(ref buffer);
        }

        protected override void Serialize(T value, ref ByteBuffer buffer)
        {
            _formatter.Serialize(ref buffer, value);
        }
    }
}