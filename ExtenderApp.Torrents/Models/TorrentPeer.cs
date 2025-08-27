using System.Collections.Specialized;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Connections;

namespace ExtenderApp.Torrents.Models
{
    /// <summary>
    /// 表示一个种子文件的对等节点。
    /// </summary>
    public class TorrentPeer : INotifyCollectionChanged
    {
        /// <summary>
        /// 当集合更改时触发的事件。
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        /// 获取或设置对等节点的 URI。
        /// </summary>
        public Uri PeerUri { get; set; }

        /// <summary>
        /// 获取或设置对等节点的 ID。
        /// </summary>
        public PeerId? Id { get; set; }

        /// <summary>
        /// 获取或设置对等节点的客户端应用程序。
        /// </summary>
        public Software ClientApp { get; set; }

        /// <summary>
        /// 获取或设置对等节点的加密类型。
        /// </summary>
        public EncryptionType EncryptionType { get; set; }

        /// <summary>
        /// 获取或设置对等节点的连接方向。
        /// </summary>
        public Direction ConnectionDirection { get; set; }

        /// <summary>
        /// 获取或设置对等节点是否正在扼流（choking）。
        /// </summary>
        public bool AmChoking { get; set; }

        /// <summary>
        /// 获取或设置对等节点是否被扼流（choked）。
        /// </summary>
        public bool IsChoking { get; set; }

        /// <summary>
        /// 获取或设置对等节点是否感兴趣（interested）。
        /// </summary>
        public bool AmInterested { get; set; }

        /// <summary>
        /// 获取或设置对等节点是否对其他节点感兴趣（interested）。
        /// </summary>
        public bool IsInterested { get; set; }

        /// <summary>
        /// 获取或设置对等节点是否为种子节点（seeder）。
        /// </summary>
        public bool IsSeeder { get; set; }

        /// <summary>
        /// 获取或设置对等节点是否已连接。
        /// </summary>
        public bool IsConnect { get; set; }

        /// <summary>
        /// 获取或设置对等节点发送的片段数量。
        /// </summary>
        public long PiecesSent { get; set; }

        /// <summary>
        /// 获取或设置对等节点发送片段的速率。
        /// </summary>
        public long PiecesSentRate { get; set; }

        /// <summary>
        /// 获取或设置对等节点接收的片段数量。
        /// </summary>
        public long PiecesReceived { get; set; }

        /// <summary>
        /// 获取或设置对等节点接收片段的速率。
        /// </summary>
        public long PiecesReceivedRate { get; set; }

        /// <summary>
        /// 获取或设置对等节点发送的协议字节数。
        /// </summary>
        public long ProtocolSent { get; set; }

        /// <summary>
        /// 获取或设置对等节点接收的协议字节数。
        /// </summary>
        public long ProtocolReceived { get; set; }

        /// <summary>
        /// 获取或设置对等节点发送的内容字节数。
        /// </summary>
        public long ContentSent { get; set; }

        /// <summary>
        /// 获取或设置对等节点接收的内容字节数。
        /// </summary>
        public long ContentReceived { get; set; }

        /// <summary>
        /// 获取或设置对等节点的开始时间。
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 获取或设置对等节点已连接的时间。
        /// </summary>
        public TimeSpan ElapsedTime { get; set; }

        /// <summary>
        /// 初始化 <see cref="TorrentPeer"/> 类的新实例。
        /// </summary>
        /// <param name="peerUri">对等节点的 URI。</param>
        public TorrentPeer(Uri peerUri)
        {
            PeerUri = peerUri ?? throw new ArgumentNullException(nameof(peerUri));
            IsConnect = false;
        }

        /// <summary>
        /// 初始化 <see cref="TorrentPeer"/> 类的新实例。
        /// </summary>
        /// <param name="peerId">对等节点的 ID。</param>
        public TorrentPeer(PeerId peerId)
        {
            SetPeerId(peerId);
        }

        /// <summary>
        /// 设置对等节点的 ID。
        /// </summary>
        /// <param name="peerId">对等节点的 ID。</param>
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

        /// <summary>
        /// 更新对等节点的状态。
        /// </summary>
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
