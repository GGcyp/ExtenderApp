namespace ExtenderApp.Torrent
{
    public class Torrent
    {
        private readonly LocalTorrentInfo _localInfo;

        private readonly TorrentSender _torrentSender;

        public TorrentFile? TorrentFile { get; }

        public long Uploaded { get; private set; }

        public long Downloaded { get; private set; }

        public long Left { get; private set; }

        public TorrentFileInfoNodeParent InfoNodeParent { get; }

        public InfoHashPeerStore TorrentPeers { get; }

        public InfoHash Hash { get; }

        internal Torrent(InfoHash infoHash, TorrentFileInfoNodeParent parent, TorrentFile? torrentFile, LocalTorrentInfo info, TorrentSender torrentSender, InfoHashPeerStore infoHashPeerStore)
        {
            _localInfo = info;
            _torrentSender = torrentSender;

            TorrentFile = torrentFile;
            InfoNodeParent = parent;
            Hash = infoHash;
            TorrentPeers = infoHashPeerStore;
        }

        public void AnnounceAsync()
        {
            var trackerRequest = new TrackerRequest
            {
                Id = _localInfo.Id,
                Port = (ushort)_localInfo.Port,
                Uploaded = Uploaded,
                Downloaded = Downloaded,
                Left = 0,
                Compact = "1",
                NoPeerId = "1",
                Hash = TorrentFile.Hash,
                Event = (byte)AnnounceEventType.None,
            };


            _torrentSender.SendTrackerRequest(trackerRequest);
        }
    }
}
