using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class ErrorDtoFormatter : ResolverFormatter<ErrorDto>
    {
        private readonly IBinaryFormatter<int> _int;
        private readonly IBinaryFormatter<string> _string;

        public override int DefaultLength => _int.DefaultLength + _string.DefaultLength;

        public ErrorDtoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
            _string = GetFormatter<string>();
        }

        public override ErrorDto Deserialize(ref ByteBuffer buffer)
        {
            ErrorDto dto = new ErrorDto();

            dto.StatrCode = _int.Deserialize(ref buffer);
            dto.Message = _string.Deserialize(ref buffer);

            return dto;
        }

        public override void Serialize(ref ByteBuffer buffer, ErrorDto value)
        {
            _int.Serialize(ref buffer, value.StatrCode);
            _string.Serialize(ref buffer, value.Message);
        }

        public override long GetLength(ErrorDto value)
        {
            return _int.DefaultLength + _string.GetLength(value.Message);
        }
    }
}
