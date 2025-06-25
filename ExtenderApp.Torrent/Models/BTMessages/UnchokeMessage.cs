using System.Buffers.Binary;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 解除阻塞消息
    /// </summary>
    public class UnchokeMessage : BtMessage
    {
        public UnchokeMessage()
        {
            LengthPrefix = 1;
            MessageId = BTMessageType.Unchoke;
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
