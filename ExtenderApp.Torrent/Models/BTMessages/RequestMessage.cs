using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent.Models.BTMessages
{
    /// <summary>
    /// 请求消息
    /// </summary>
    public class RequestMessage : BtMessage
    {
        public int PieceIndex { get; }
        public int Begin { get; }
        public int Length { get; }

        public RequestMessage(int pieceIndex, int begin, int length)
        {
            PieceIndex = pieceIndex;
            Begin = begin;
            Length = length;
            LengthPrefix = 13;
            MessageId = BTMessageType.Request;
        }

        public static RequestMessage Decode(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < 12)
                throw new InvalidDataException("Request消息数据不足");

            int pieceIndex = BinaryPrimitives.ReadInt32BigEndian(buffer);
            int begin = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(4));
            int length = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(8));
            return new RequestMessage(pieceIndex, begin, length);
        }

        public override void Encode(ExtenderBinaryWriter writer)
        {
            var span = writer.GetSpan(17);
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(0, 4), 13);
            span[4] = (byte)MessageId;
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(5, 4), PieceIndex);
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(9, 4), Begin);
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(13, 4), Length);
            writer.Advance(17);
        }
    }
}
