using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// Guid格式化器
    /// </summary>
    internal class GuidFormatter : BinaryFormatter<Guid>
    {
        private const int GuidByteLength = 16;

        public override int DefaultLength => GuidByteLength;

        public override Guid Deserialize(ref ByteBuffer buffer)
        {
            Span<byte> span = stackalloc byte[GuidByteLength];
            buffer.TryCopyTo(span);
            buffer.Advance(GuidByteLength);
            return new Guid(span);
        }

        public override void Serialize(ref ByteBuffer buffer, Guid value)
        {
            var span = buffer.GetSpan(GuidByteLength);
            value.TryWriteBytes(span);
            buffer.Advance(GuidByteLength);
        }

        public override long GetLength(Guid value)
        {
            return GuidByteLength;
        }
    }
}