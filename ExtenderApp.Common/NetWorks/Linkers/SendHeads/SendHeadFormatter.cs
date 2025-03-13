using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.Linkers.SendDtos
{
    internal class SendHeadFormatter : ResolverFormatter<SendHead>
    {
        private readonly IBinaryFormatter<int> _int;

        public override int Length => _int.Length * 2 + Utility.HEAD_LENGTH;

        public SendHeadFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
        }

        public override SendHead Deserialize(ref ExtenderBinaryReader reader)
        {
            var result = new SendHead();

            result.HasSendHead = Utility.FindHead(reader.UnreadSpan, out var startIndex);
            if (!result.HasSendHead)
                return result;

            reader.Advance(Utility.HEAD_LENGTH);

            result.TypeCode = _int.Deserialize(ref reader);
            result.Length = _int.Deserialize(ref reader);

            return result;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, SendHead value)
        {
            //if (!value.HasSendHead) 
            //    return;

            Utility.WriteSendHead(writer.GetSpan(Utility.HEAD_LENGTH));
            writer.Advance(Utility.HEAD_LENGTH);

            _int.Serialize(ref writer, value.TypeCode);
            _int.Serialize(ref writer, value.Length);
        }
    }
}
