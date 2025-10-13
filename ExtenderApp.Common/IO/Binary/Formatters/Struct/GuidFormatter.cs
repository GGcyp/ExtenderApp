using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// Guid格式化器
    /// </summary>
    internal class GuidFormatter : ResolverFormatter<Guid>
    {
        private readonly IBinaryFormatter<string> _formatter;

        public override int DefaultLength => throw new NotImplementedException();

        public GuidFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _formatter = GetFormatter<string>();
        }

        public override Guid Deserialize(ref ByteBuffer buffer)
        {
            var result = _formatter.Deserialize(ref buffer);
            return Guid.Parse(result);
        }

        public override void Serialize(ref ByteBuffer buffer, Guid value)
        {
            _formatter.Serialize(ref buffer, value.ToString());
        }

        public override long GetLength(Guid value)
        {
            return _formatter.GetLength(value.ToString());
        }
    }
}
