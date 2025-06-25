using System.Buffers.Binary;
using System.IO;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 位字段消息
    /// </summary>
    public class BitFieldMessage : BtMessage
    {
        public byte[] BitField { get; }

        public BitFieldMessage(byte[] bitField)
        {
            BitField = bitField;
            LengthPrefix = 1 + bitField.Length;
            MessageId = BTMessageType.BitField;
        }

        public static BitFieldMessage Decode(ReadOnlySpan<byte> buffer, int length)
        {
            if (buffer.Length < length)
                throw new InvalidDataException("BitField消息数据不足");

            var bitField = buffer.Slice(0, length).ToArray();
            return new BitFieldMessage(bitField);
        }

        public override void Encode(ExtenderBinaryWriter writer)
        {
            var span = writer.GetSpan(5 + BitField.Length);
            BinaryPrimitives.WriteInt32BigEndian(span, 1 + BitField.Length);
            span[4] = (byte)MessageId;
            writer.Advance(5);
            writer.Write(BitField.AsSpan(4));
        }
    }
}
