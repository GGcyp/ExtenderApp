using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class LinkHeaderFormatter : AutoFormatter<LinkHeade>
    {
        public LinkHeaderFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
        }

        protected override void Init(AutoMemberDetailsStore store)
        {
            store.Add(l => l.DataLength)
                .Add(l => l.DataType);
        }
    }
}