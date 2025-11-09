using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.LAN
{
    internal class LANModelFormatter : ResolverFormatter<LANModel>
    {
        public override int DefaultLength => 1;

        public LANModelFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
        }

        public override LANModel Deserialize(ref ByteBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public override void Serialize(ref ByteBuffer buffer, LANModel value)
        {
            throw new NotImplementedException();
        }

        public override long GetLength(LANModel value)
        {
            throw new NotImplementedException();
        }
    }
}
