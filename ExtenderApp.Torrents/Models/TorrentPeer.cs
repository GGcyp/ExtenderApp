using System.Collections.Specialized;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Connections;

namespace ExtenderApp.Torrents.Models
{
    public class TorrentPeer : INotifyCollectionChanged
    {
        public PeerId PeerId { get; private set; }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public Uri PeerUri { get; set; }
        public PeerId PeerID { get; set; }
        public EncryptionType EncryptionType { get; set; }

        public Direction ConnectionDirection { get; set; }

        public bool AmChoking { get; set; }
        public bool IsChoking { get; set; }
        public bool AmInterested { get; set; }
        public bool IsInterested { get; set; }
        public bool IsSeeder { get; set; }

        public TorrentPeer(PeerId peerId)
        {
            PeerId = peerId;
            PeerUri = peerId.Uri;
            PeerID = peerId;
            EncryptionType = peerId.EncryptionType;
            ConnectionDirection = peerId.ConnectionDirection;
            AmChoking = peerId.AmChoking;
            IsChoking = peerId.IsChoking;
            AmInterested = peerId.AmInterested;
            IsInterested = peerId.IsInterested;
            IsSeeder = peerId.IsSeeder;
        }

        public void Update()
        {
            EncryptionType = PeerId.EncryptionType;
            AmChoking = PeerId.AmChoking;
            IsChoking = PeerId.IsChoking;
            AmInterested = PeerId.AmInterested;
            IsInterested = PeerId.IsInterested;
        }
    }
}
