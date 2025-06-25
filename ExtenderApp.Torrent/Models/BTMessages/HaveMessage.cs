using System.Buffers.Binary;
using System.IO;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 拥有分片消息
    /// </summary>
    public class HaveMessage : BtMessage
    {
        public int PieceIndex { get; }

        public HaveMessage(int pieceIndex)
        {
            PieceIndex = pieceIndex;
            LengthPrefix = 5;
            MessageId = BTMessageType.Have;
        }

        public static HaveMessage Decode(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < 4)
                throw new InvalidDataException("Have消息数据不足");

            int pieceIndex = BinaryPrimitives.ReadInt32BigEndian(buffer);
            return new HaveMessage(pieceIndex);
        }

        public override void Encode(ExtenderBinaryWriter writer)
        {
            var span = writer.GetSpan(9);
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(0, 4), 5);
            span[4] = (byte)MessageId;
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(5, 4), PieceIndex);
            writer.Advance(9);
        }
    }
}
