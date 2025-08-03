using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
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

        public override ErrorDto Deserialize(ref ExtenderBinaryReader reader)
        {
            ErrorDto dto = new ErrorDto();

            dto.StatrCode = _int.Deserialize(ref reader);
            dto.Message = _string.Deserialize(ref reader);

            return dto;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, ErrorDto value)
        {
            _int.Serialize(ref writer, value.StatrCode);
            _string.Serialize(ref writer, value.Message);
        }

        public override long GetLength(ErrorDto value)
        {
            return _int.DefaultLength + _string.GetLength(value.Message);
        }
    }
}
