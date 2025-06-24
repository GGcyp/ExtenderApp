using ExtenderApp.Common.Networks;

namespace ExtenderApp.Torrent
{
    internal class TorrentLinkParser : LinkParser
    {
        public override T? Deserialize<T>(byte[] bytes) where T : default
        {
            throw new NotImplementedException();
        }

        public override void Receive(byte[] bytes, int length)
        {
            throw new NotImplementedException();
        }

        public override void Serialize<T>(T value, out byte[] bytes, out int start, out int length)
        {
            throw new NotImplementedException();
        }
    }
}
