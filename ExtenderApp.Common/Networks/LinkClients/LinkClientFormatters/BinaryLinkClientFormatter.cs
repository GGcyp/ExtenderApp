using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class BinaryLinkClientFormatter<T> : LinkClientFormatter<T>
    {
        private readonly IBinaryFormatter<T> _formatter;

        public BinaryLinkClientFormatter(IBinaryFormatter<T> formatter)
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