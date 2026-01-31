using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    internal class IEnumerableFormatter<T> : ResolverFormatter<IEnumerable<T>>
    {
        private readonly IBinaryFormatter<T> _formatter;
        private readonly IBinaryFormatter<List<T>> _list;

        public IEnumerableFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _formatter = GetFormatter<T>();
            _list = GetFormatter<List<T>>();
        }

        public override IEnumerable<T> Deserialize(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
                return null;

            return _list.Deserialize(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, IEnumerable<T> value)
        {
            if (value == null)
            {
                WriteNil(ref buffer);
                return;
            }

            var list = new List<T>(value);
            _list.Serialize(ref buffer, list);
        }

        public override long GetLength(IEnumerable<T> value)
        {
            if (value == null)
                return 1;

            long length = 5;
            foreach (var item in value)
            {
                length += _formatter.GetLength(item);
            }
            return length;
        }
    }
}
