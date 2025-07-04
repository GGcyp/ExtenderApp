

namespace ExtenderApp.Torrent
{
    public class TorrentProvider
    {
        private readonly TorrentFileForamtter _torrentFileForamtter;
        private readonly TrackerProvider _trackerProvider;

        public TorrentProvider(TorrentFileForamtter torrentFileForamtter, TrackerProvider trackerProvider)
        {
            _torrentFileForamtter = torrentFileForamtter;
            _trackerProvider = trackerProvider;
        }

        public Torrent GetTorrent(Memory<byte> memory)
        {
            var torrentFile = _torrentFileForamtter.Decode(memory);
            Torrent result = new(torrentFile);



            return null;
        }
    }
}
