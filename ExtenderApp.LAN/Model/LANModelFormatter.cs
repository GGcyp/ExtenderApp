﻿using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.LAN
{
    internal class LANModelFormatter : ResolverFormatter<LANModel>
    {
        public override int Length => throw new NotImplementedException();

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
