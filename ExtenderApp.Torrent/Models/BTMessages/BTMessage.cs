using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// BitTorrent 消息基类
    /// </summary>
    public abstract class BtMessage
    {
        public int LengthPrefix { get; protected set; }
        public BTMessageType MessageId { get; protected set; }

        public abstract void Encode(ExtenderBinaryWriter writer);

        public static BtMessage Decode(ReadOnlySpan<byte> buffer)
        {
            //if (buffer.Length < 4)
            //    throw new InvalidDataException("消息长度不足");

            //int length = BinaryPrimitives.ReadInt32BigEndian(buffer);
            //buffer = buffer.Slice(4);

            //if (length == 0)
            //    return new KeepAliveMessage();

            //if (buffer.Length < 1)
            //    throw new InvalidDataException("消息ID缺失");

            //byte messageId = buffer[0];
            //buffer = buffer.Slice(1);

            //switch ((MessageType)messageId)
            //{
            //    case MessageType.Choke:
            //        return new ChokeMessage();
            //    case MessageType.Unchoke:
            //        return new UnchokeMessage();
            //    case MessageType.Interested:
            //        return new InterestedMessage();
            //    case MessageType.NotInterested:
            //        return new NotInterestedMessage();
            //    case MessageType.Have:
            //        return HaveMessage.Decode(buffer);
            //    case MessageType.BitField:
            //        return BitFieldMessage.Decode(buffer, length - 1);
            //    case MessageType.Request:
            //        return RequestMessage.Decode(buffer);
            //    case MessageType.Piece:
            //        return PieceMessage.Decode(buffer);
            //    case MessageType.Cancel:
            //        return CancelMessage.Decode(buffer);
            //    case MessageType.Port:
            //        return PortMessage.Decode(buffer);
            //    default:
            //        return new UnknownMessage(messageId, buffer.ToArray());

            return null;
        }
    }
}
}
