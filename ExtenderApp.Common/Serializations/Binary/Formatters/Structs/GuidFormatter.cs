using ExtenderApp.Buffer;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// Guid格式化器
    /// </summary>
    internal class GuidFormatter : BinaryFormatter<Guid>
    {
        private const int GuidByteLength = 16;

        public override int DefaultLength => GuidByteLength;

        public override void Serialize(AbstractBuffer<byte> buffer, Guid value)
        {
            var span = buffer.GetSpan(GuidByteLength);
            value.TryWriteBytes(span);
            buffer.Advance(GuidByteLength);
        }

        public override void Serialize(ref SpanWriter<byte> writer, Guid value)
        {
            value.TryWriteBytes(writer.UnwrittenSpan.Slice(0, GuidByteLength));
            writer.Advance(GuidByteLength);
        }

        public override Guid Deserialize(AbstractBufferReader<byte> reader)
        {
            Span<byte> span = stackalloc byte[GuidByteLength];
            reader.Read(span);
            return new Guid(span);
        }

        public override Guid Deserialize(ref SpanReader<byte> reader)
        {
            Span<byte> span = stackalloc byte[GuidByteLength];
            reader.Read(span);
            return new Guid(span);
        }

        public override long GetLength(Guid value)
        {
            return GuidByteLength;
        }
    }
}