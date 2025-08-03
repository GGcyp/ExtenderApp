
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Torrents.Models
{
    internal class TorrentModelFormatter : ResolverFormatter<TorrentModel>
    {
        public override int DefaultLength => 1;

        public TorrentModelFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
        }

        public override TorrentModel Deserialize(ref ExtenderBinaryReader reader)
        {
            return new TorrentModel();
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, TorrentModel value)
        {
            
        }
    }
}
