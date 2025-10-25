using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    public sealed class TcpFramingPlugin : LinkClientPluginBase<ITcpLinker>
    {
        private static byte[] DefaultMagic = { 0x54, 0x43, 0x50, 0x21 }; // "TCP!"
        private readonly byte[]? _magic;
        private readonly int _magicLen;
        private ByteBlock receiveCacheBlock;
        private int reamingLength;

        public TcpFramingPlugin() : this((int)Utility.KilobytesToBytes(16), DefaultMagic)
        {
        }

        public TcpFramingPlugin(int cacheLength, byte[]? magic = null)
        {
            _magic = magic;
            _magicLen = magic?.Length ?? 0;
            receiveCacheBlock = new(cacheLength);
            reamingLength = 0;
        }


    }
}
