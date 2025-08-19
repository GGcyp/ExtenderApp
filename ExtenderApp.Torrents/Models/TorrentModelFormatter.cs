using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Torrents.Models
{
    internal class TorrentModelFormatter : VersionDataFormatter<TorrentModel>
    {
        public override int DefaultLength => 1;

        public override Version FormatterVersion { get; }

        public TorrentModelFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            FormatterVersion = new Version(0, 0, 0, 1);
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
