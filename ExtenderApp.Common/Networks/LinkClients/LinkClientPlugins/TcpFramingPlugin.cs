using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    public sealed class TcpFramingPlugin : LinkClientPluginBase<ITcpLinkClient>
    {
        private static byte[] DefaultMagic = { 0x54, 0x43, 0x50, 0x21 }; // "TCP!"
        private readonly byte[] _magic;
        private readonly int _magicLen;
        private ByteBlock receiveCacheBlock;
        private int reamingLength;

        public TcpFramingPlugin() : this((int)Utility.KilobytesToBytes(16), DefaultMagic)
        {
        }

        public TcpFramingPlugin(int cacheLength, byte[] magic)
        {
            _magic = magic ?? DefaultMagic;
            _magicLen = magic?.Length ?? 0;
            receiveCacheBlock = new(cacheLength);
            reamingLength = 0;
        }

        //public override void OnSend(ILinkClient<ITcpLinker> client, ref LinkClientPluginSendMessage message)
        //{
        //    message.FirstMessageBuffer.Write(_magic);

        //    int intLength = sizeof(int);
        //    int messageCount = 2;
        //    Span<byte> headSpan = message.FirstMessageBuffer.GetSpan(intLength * messageCount);
        //    BinaryPrimitives.WriteInt32BigEndian(headSpan.Slice(0, intLength), (int)message.OriginalMessageBuffer.Length);
        //    BinaryPrimitives.WriteInt32BigEndian(headSpan.Slice(intLength, intLength), message.MessageType);
        //    message.FirstMessageBuffer.WriteAdvance(intLength * messageCount);
        //}

        //public override void OnReceive(ILinkClient<ITcpLinker> client, ref LinkClientPluginReceiveMessage message)
        //{
        //    base.OnReceive(client, ref message);
        //}
    }
}
