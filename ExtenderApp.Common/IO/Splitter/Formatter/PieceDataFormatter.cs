using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Splitter
{
    internal class PieceDataFormatter : ResolverFormatter<PieceData>
    {
        public override int DefaultLength => _byteArray.DefaultLength;

        private readonly IBinaryFormatter<byte[]> _byteArray;
        private readonly IBinaryFormatter<int> _int;


        public PieceDataFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _byteArray = GetFormatter<byte[]>();
            _int = GetFormatter<int>();
        }

        public override PieceData Deserialize(ref ByteBuffer buffer)
        {
            var length = _int.Deserialize(ref buffer);
            var trueCount = _int.Deserialize(ref buffer);
            var pieces = _byteArray.Deserialize(ref buffer);
            return new PieceData(pieces, length, trueCount);
        }

        public override void Serialize(ref ByteBuffer buffer, PieceData value)
        {
            _int.Serialize(ref buffer, value.Length);
            _int.Serialize(ref buffer, value.TrueCount);
            _byteArray.Serialize(ref buffer, value.CopeToArray());
        }

        public override long GetLength(PieceData value)
        {
            return _int.DefaultLength * 2 + value.Length + _byteArray.DefaultLength;
        }
    }
}
