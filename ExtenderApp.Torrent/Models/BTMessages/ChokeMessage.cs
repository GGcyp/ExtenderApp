using System.Buffers.Binary;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 阻塞消息
    /// </summary>
    public class ChokeMessage : BtMessage
    {
        public ChokeMessage()
        {
            LengthPrefix = 1;
            MessageId = BTMessageType.Choke;
        }

        public override void Encode(ExtenderBinaryWriter writer)
        {
            var span = writer.GetSpan(5);
            BinaryPrimitives.WriteInt32BigEndian(span, 1);
            span[4] = (byte)MessageId;
            writer.Advance(5);
        }
    }
}
