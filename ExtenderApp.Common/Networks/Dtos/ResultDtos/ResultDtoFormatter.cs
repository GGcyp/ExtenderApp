using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class ResultDtoFormatter : ResolverFormatter<ResultDto>
    {
        private readonly IBinaryFormatter<int> _int;
        private readonly IBinaryFormatter<string> _string;

        public override int DefaultLength => _int.DefaultLength + _string.DefaultLength;
        public ResultDtoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = resolver.GetFormatter<int>();
            _string = resolver.GetFormatter<string>();
        }

        public override ResultDto Deserialize(ref ByteBuffer buffer)
        {
            int stateCode = _int.Deserialize(ref buffer);
            string message = _string.Deserialize(ref buffer);
            return new ResultDto(stateCode, message);
        }

        public override void Serialize(ref ByteBuffer buffer, ResultDto value)
        {
            _int.Serialize(ref buffer, value.StateCode);
            _string.Serialize(ref buffer, value.Message);
        }

        public override long GetLength(ResultDto value)
        {
            return _int.DefaultLength + _string.GetLength(value.Message);
        }
    }
}
