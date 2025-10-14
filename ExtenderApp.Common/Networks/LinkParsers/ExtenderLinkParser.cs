using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class ExtenderLinkParser : LinkParser
    {
        private readonly IBinaryParser _binaryParser;

        public ExtenderLinkParser(IBinaryParser binaryParser, SequencePool<byte> sequencePool) : base(sequencePool)
        {
            _binaryParser = binaryParser;
        }

        protected override void Receive(ref ByteBuffer buffer)
        {

        }

        public override void Serialize<T>(ref ByteBuffer buffer, T value)
        {
            
        }
    }
}
