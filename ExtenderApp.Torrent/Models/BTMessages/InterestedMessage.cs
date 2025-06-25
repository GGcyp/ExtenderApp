using System.Buffers.Binary;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 感兴趣消息
    /// </summary>
    public class InterestedMessage : BtMessage
    {
        public InterestedMessage()
        {
            LengthPrefix = 1;
            MessageId = BTMessageType.Interested;
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
