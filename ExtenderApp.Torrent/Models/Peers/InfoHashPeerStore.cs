using System.Collections.Concurrent;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// InfoHashPeerStore 类表示与特定 InfoHash 关联的 Peer 存储。
    /// </summary>
    public class InfoHashPeerStore
    {
        /// <summary>
        /// 一个并发字典，用于存储 PeerInfo 与 TorrentPeer 的映射关系。
        /// </summary>
        private readonly ConcurrentDictionary<PeerInfo, TorrentPeer> _peers;

        /// <summary>
        /// TorrentPeerProvider 实例，用于提供Torrent对端信息
        /// </summary>
        private readonly TorrentPeerProvider _provider;

        /// <summary>
        /// 获取与当前 InfoHashPeerStore 实例关联的 InfoHash。
        /// </summary>
        public InfoHash Hash { get; }

        /// <summary>
        /// 初始化 InfoHashPeerStore 实例。
        /// </summary>
        /// <param name="hash">与当前 InfoHashPeerStore 实例关联的 InfoHash。</param>
        public InfoHashPeerStore(InfoHash hash,TorrentPeerProvider provider)
        {
            _peers = new();
            _provider = provider;

            Hash = hash;
        }

        /// <summary>
        /// 向 PeersSet 中添加一个 PeerAddress。
        /// </summary>
        /// <param name="peerAddress">要添加的 PeerAddress。</param>
        public void AddPeerInfo(PeerAddress peerAddress)
        {
            
        }
    }
}
