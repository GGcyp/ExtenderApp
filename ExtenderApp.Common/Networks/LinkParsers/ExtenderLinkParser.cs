using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks.LinkParsers
{
    public class ExtenderLinkParser : LinkParser
    {
        private readonly IBinaryParser _binaryParser;

        public ExtenderLinkParser(IBinaryParser binaryParser)
        {
            _binaryParser = binaryParser ?? throw new ArgumentNullException(nameof(binaryParser));
        }

        public override T Deserialize<T>(byte[] bytes)
        {
            return _binaryParser.Deserialize<T>(bytes);
        }

        public override void Serialize<T>(T value, out byte[] bytes, out int start, out int length)
        {
            bytes = _binaryParser.SerializeForArrayPool(value, out length);
            start = 0;
        }
    }
}
