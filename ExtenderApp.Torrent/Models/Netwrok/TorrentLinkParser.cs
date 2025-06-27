using ExtenderApp.Common.Networks;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    public class TorrentLinkParser : LinkParser<BTMessage>
    {
        private readonly BTMessageEncoder encoder;

        public TorrentLinkParser(BTMessageEncoder encoder, SequencePool<byte> sequencePool) : base(sequencePool)
        {
            this.encoder = encoder;
        }

        public override void Serialize<T>(ref ExtenderBinaryWriter writer, T value)
        {
            if (value is not BTMessage message)
            {
                throw new ArgumentException("不可以处理非BTMessage类型", nameof(value));
                return;
            }
            encoder.Encode(message, writer);
        }

        protected override void Receive(ref ExtenderBinaryReader reader)
        {
            var message = encoder.Decode(ref reader);
            ReceivedMessage(message);
        }
    }
}
