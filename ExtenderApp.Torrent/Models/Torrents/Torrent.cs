using ExtenderApp.Abstract;

namespace ExtenderApp.Torrent
{
    public class Torrent
    {
        private readonly LocalTorrentInfo _localInfo;

        private readonly TorrentSender _torrentSender;

        public TorrentFile? TorrentFile { get; }

        public long Uploaded
        {
            get
            {
                long uploaded = 0;
                InfoNodeParent.ParentNode.LoopAllChildNodes((t, l) =>
                {
                    if (t.IsFile && t.IsDownload)
                        l += t.Uploaded;
                }, ref uploaded);
                return uploaded;
            }
        }

        public long Downloaded
        {
            get
            {
                long downloaded = 0;
                InfoNodeParent.ParentNode.LoopAllChildNodes((t, l) =>
                {
                    if (t.IsFile && t.IsDownload)
                        l += t.Downloaded;
                }, ref downloaded);
                return downloaded;
            }
        }

        public long Left
        {
            get
            {
                long left = 0;
                InfoNodeParent.ParentNode.LoopAllChildNodes((t, l) =>
                {
                    if (t.IsFile && t.IsDownload)
                        l += t.Length;
                }, ref left);
                return left;
            }
        }

        public TorrentFileInfoNodeParent InfoNodeParent { get; }

        public List<TorrentPeer> TorrentPeers { get; }

        public InfoHash Hash { get; }

        internal Torrent(InfoHash infoHash, TorrentFileInfoNodeParent parent, TorrentFile? torrentFile, LocalTorrentInfo info, TorrentSender torrentSender)
        {
            _localInfo = info;
            _torrentSender = torrentSender;

            TorrentFile = torrentFile;
            InfoNodeParent = parent;
            Hash = infoHash;
            TorrentPeers = new List<TorrentPeer>();
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

            var trackers = TorrentFile.AnnounceList;
            for (int i = 0; i < trackers.Count; i++)
            {
                _torrentSender.SendTrackerRequest(trackerRequest);
            }
        }
    }
}
