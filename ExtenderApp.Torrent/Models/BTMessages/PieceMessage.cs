using System.Buffers.Binary;
using System.IO;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 分片数据消息
    /// </summary>
    public class PieceMessage : BtMessage
    {
        public int PieceIndex { get; }
        public int Begin { get; }
        public byte[] Block { get; }

        public PieceMessage(int pieceIndex, int begin, byte[] block)
        {
            PieceIndex = pieceIndex;
            Begin = begin;
            Block = block;
            LengthPrefix = 9 + block.Length;
            MessageId = BTMessageType.Piece;
        }

        public static PieceMessage Decode(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < 8)
                throw new InvalidDataException("Piece消息头部数据不足");

            int pieceIndex = BinaryPrimitives.ReadInt32BigEndian(buffer);
            int begin = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(4));
            byte[] block = buffer.Slice(8).ToArray();
            return new PieceMessage(pieceIndex, begin, block);
        }

        public override void Encode(ExtenderBinaryWriter writer)
        {
            var span = writer.GetSpan(13 + Block.Length);
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(0, 4), 9 + Block.Length);
            span[4] = (byte)MessageId;
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(5, 4), PieceIndex);
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(9, 4), Begin);
            writer.Advance(13 + Block.Length);
            writer.Write(Block);
        }
    }
}
