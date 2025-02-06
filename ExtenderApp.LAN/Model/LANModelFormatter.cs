using ExtenderApp.Abstract;
using ExtenderApp.Common.File.Binary.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.LAN
{
    internal class LANModelFormatter : ResolverFormatter<LANModel>
    {
        public LANModelFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
        }

        public override LANModel Deserialize(ref ExtenderBinaryReader reader)
        {
            var result = new LANModel();

            return result;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, LANModel value)
        {

        }
    }
}
