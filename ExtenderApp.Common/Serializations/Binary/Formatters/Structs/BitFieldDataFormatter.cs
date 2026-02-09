using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    internal class BitFieldDataFormatter : ResolverFormatter<BitFieldData>
    {
        private readonly IBinaryFormatter<int> _int;

        public BitFieldDataFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
        }

        public override BitFieldData Deserialize(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
            {
                return BitFieldData.Empty;
            }

            if (!TryReadArrayHeader(ref buffer))
            {
                ThrowOperationException("无法反序列化为 BitFieldData，数据格式不正确。");
            }

            int length = _int.Deserialize(ref buffer);
            var block = buffer.Read(length);
            var bitFieldData = new BitFieldData(block.CommittedSpan);
            block.Dispose();
            return bitFieldData;
        }

        public override void Serialize(ref ByteBuffer buffer, BitFieldData value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref buffer);
                return;
            }

            var bytes = ArrayPool<byte>.Shared.Rent(value.Length);
            value.ToBytes(bytes);
            WriteArrayHeader(ref buffer);
            _int.Serialize(ref buffer, value.Length);
            buffer.Write(bytes.AsMemory(0, value.Length));
            ArrayPool<byte>.Shared.Return(bytes);
        }

        public override long GetLength(BitFieldData value)
        {
            if (value.IsEmpty)
                return 1; // 如果是null，返回1字节表示null

            return _int.GetLength(value.Length) + 1 + value.Length;
        }
    }
}