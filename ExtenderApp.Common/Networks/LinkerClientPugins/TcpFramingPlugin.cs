using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    public sealed class TcpFramingPlugin
    {
        private static byte[] DefaultMagic = { 0x54, 0x43, 0x50, 0x21 }; // "TCP!"

        private readonly IByteBufferFactory _bufferFactory;
        private readonly IBinaryFormatter<LinkHeader> _headerFormatter;
        private readonly byte[]? _magic;
        private readonly int _magicLen;
        private ByteBlock receiveCacheBlock;
        private int reamingLength;

        public TcpFramingPlugin(IBinaryFormatter<LinkHeader> headerFormatter, IByteBufferFactory bufferFactory) : this(headerFormatter, bufferFactory, (int)Utility.KilobytesToBytes(16), DefaultMagic)
        {
        }

        public TcpFramingPlugin(IBinaryFormatter<LinkHeader> headerFormatter, IByteBufferFactory bufferFactory, int cacheLength, byte[]? magic = null)
        {
            _bufferFactory = bufferFactory;
            _headerFormatter = headerFormatter;
            _magic = magic;
            _magicLen = magic?.Length ?? 0;
            receiveCacheBlock = new(cacheLength);
            reamingLength = 0;
        }

       
    }
}
