
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    public class Torrent
    {
        private readonly Random _random;

        public TorrentFile TorrentFile { get; }

        public List<PeerId> Peers { get; }

        public LocalTorrentInfo LocalInfo { get; }

        private int transactionId;
        private TrackerProvider provider;
        public long Uploaded { get; private set; }
        public long Downloaded { get; private set; }
        public long Left => GetLeft();

        public Torrent(TorrentFile torrentFile, LocalTorrentInfo info, Random random, TrackerProvider trackerProvider)
        {
            TorrentFile = torrentFile;
            LocalInfo = info;
            _random = random;
            Peers = new();
            provider = trackerProvider;
        }

        public void AnnounceAsync()
        {
            transactionId = _random.Next();
            var trackerRequest = new TrackerRequest
            {
                Id = LocalInfo.Id,
                Port = (ushort)LocalInfo.Port,
                Uploaded = Uploaded,
                Downloaded = Downloaded,
                Left = 0,
                Compact = "1",
                NoPeerId = "1",
                Hash = TorrentFile.Hash,
                Event = (byte)AnnounceEventType.None,
                TransactionId = transactionId
            };

            var trackers = TorrentFile.AnnounceList;
            for (int i = 0; i < trackers.Count; i++)
            {
                var trackerUriSting = trackers[i];
                var tracker = provider.GetTracker(trackerUriSting);
                if (tracker == null)
                {
                    continue;
                }

                tracker.AnnounceAsync(trackerRequest);
            }
        }

        private long GetLeft()
        {
            long left = 0;
            TorrentFile.FileInfoNode.LoopAllChildNodes((t, l) =>
            {
                if (t.IsFile && t.IsDownload)
                    l += t.Length;
            }, ref left);
            return left;
        }
    }
}
