using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.File.Binary.Formatter
{
    internal class DefaultObjectFormatter<T> : IBinaryFormatter<T> where T : class, new()
    {
        public T Default => default(T);

        private readonly IBinaryFormatterResolver _resolver;

        public DefaultObjectFormatter(IBinaryFormatterResolver resolver)
        {
            _resolver = resolver;
        }

        public T Deserialize(ref ExtenderBinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public void Serialize(ref ExtenderBinaryWriter writer, T value)
        {
            throw new NotImplementedException();
        }
    }
}
