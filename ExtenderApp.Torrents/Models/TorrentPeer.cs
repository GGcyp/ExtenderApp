using System.Collections.Specialized;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Connections;

namespace ExtenderApp.Torrents.Models
{
    public class TorrentPeer : INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public Uri PeerUri { get; set; }

        public PeerId? Id { get; set; }

        public Software ClientApp { get; set; }

        public EncryptionType EncryptionType { get; set; }

        public Direction ConnectionDirection { get; set; }

        public bool AmChoking { get; set; }

        public bool IsChoking { get; set; }

        public bool AmInterested { get; set; }

        public bool IsInterested { get; set; }

        public bool IsSeeder { get; set; }

        public bool IsConnect { get; set; }

        public long PiecesSent { get; set; }

        public long PiecesSentRate { get; set; }

        public long PiecesReceived { get; set; }

        public long PiecesReceivedRate { get; set; }

        public long ProtocolSent { get; set; }

        public long ProtocolReceived { get; set; }

        public long ContentSent { get; set; }

        public long ContentReceived { get; set; }

        public DateTime StartTime { get; set; }

        public TimeSpan ElapsedTime { get; set; }

        public TorrentPeer(Uri peerUri)
        {
            PeerUri = peerUri ?? throw new ArgumentNullException(nameof(peerUri));
            IsConnect = false;
        }

        public TorrentPeer(PeerId peerId)
        {
            SetPeerId(peerId);
        }

        public void SetPeerId(PeerId peerId)
        {
            Id = peerId;
            ClientApp = Id.ClientApp;
            EncryptionType = peerId.EncryptionType;
            ConnectionDirection = peerId.ConnectionDirection;
            AmChoking = peerId.AmChoking;
            IsChoking = peerId.IsChoking;
            AmInterested = peerId.AmInterested;
            IsInterested = peerId.IsInterested;
            IsSeeder = peerId.IsSeeder;
            StartTime = DateTime.Now;
            IsConnect = true;
        }

        public void Update()
        {
            if (Id == null)
                return;

            EncryptionType = Id.EncryptionType;
            AmChoking = Id.AmChoking;
            IsChoking = Id.IsChoking;
            AmInterested = Id.AmInterested;
            IsInterested = Id.IsInterested;

            PiecesSent = Id.Monitor.DataBytesSent;
            PiecesSentRate = Id.Monitor.UploadRate;

            PiecesReceived = Id.Monitor.DataBytesReceived;
            PiecesReceivedRate = Id.Monitor.DownloadRate;

            ProtocolReceived = Id.Monitor.ProtocolBytesReceived;
            ProtocolSent = Id.Monitor.ProtocolBytesSent;

            ContentSent = PiecesSent - ProtocolSent;
            ContentReceived = PiecesReceived - ProtocolReceived;

            ElapsedTime = DateTime.Now - StartTime;
        }
    }
}
