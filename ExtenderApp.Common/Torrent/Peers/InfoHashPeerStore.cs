using System.Collections.Concurrent;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Torrent
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


        private readonly Action<PeerInfo> _removeCallback;

        /// <summary>
        /// 获取与当前 InfoHashPeerStore 实例关联的 InfoHash。
        /// </summary>
        public InfoHash Hash { get; }

        /// <summary>
        /// 获取 TorrentFileInfoNodeParent 的父节点。
        /// </summary>
        /// <value>TorrentFileInfoNodeParent 的父节点。</value>
        public TorrentFileInfoNodeParent Parent { get; }

        /// <summary>
        /// 当添加新的 TorrentPeer 时触发的事件。
        /// </summary>
        public event Action<TorrentPeer>? OnPeerAdded;

        /// <summary>
        /// 初始化 InfoHashPeerStore 实例。
        /// </summary>
        /// <param name="hash">与当前 InfoHashPeerStore 实例关联的 InfoHash。</param>
        public InfoHashPeerStore(InfoHash hash, TorrentPeerProvider provider, TorrentFileInfoNodeParent parent)
        {
            _peers = new();
            _provider = provider;
            Parent = parent;
            _removeCallback = Remove;

            Hash = hash;
        }

        /// <summary>
        /// 向 PeersSet 中添加一个 PeerAddress。
        /// </summary>
        /// <param name="peerAddress">要添加的 PeerAddress。</param>
        public void Add(PeerAddress peerAddress)
        {
            var peer = _provider.CreatePeer(peerAddress, Parent);
            if (peer == null)
                return;
            Add(peer);
        }

        /// <summary>
        /// 添加TorrentPeer到集合中。
        /// </summary>
        /// <param name="peer">要添加的TorrentPeer对象。</param>
        public void Add(TorrentPeer peer, bool needSendHandshake = true)
        {
            peer.RemoveCallback = _removeCallback;
            if (needSendHandshake)
            {
                peer.OnHandshake += PrivateOnHandshake;
                peer.SendHandshake(Hash);
                return;
            }


            if (!_peers.TryAdd(peer.RemotePeerInfo, peer))
                return;
            OnPeerAdded?.Invoke(peer);
        }

        /// <summary>
        /// 处理握手事件。
        /// </summary>
        /// <param name="info">Peer信息。</param>
        /// <param name="hash">握手信息中的InfoHash。</param>
        /// <param name="peer">发起握手的TorrentPeer对象。</param>
        private void PrivateOnHandshake(PeerInfo info, InfoHash hash, TorrentPeer peer)
        {
            peer.OnHandshake -= PrivateOnHandshake;

            if (hash != Hash)
            {
                peer.Dispose();
                throw new InvalidOperationException($"预期为 {Hash}，实际得到 {hash}");
            }

            if (!_peers.TryAdd(info, peer))
                return;

            OnPeerAdded?.Invoke(peer);
        }

        /// <summary>
        /// 从集合中移除指定的TorrentPeer。
        /// </summary>
        /// <param name="peerInfo">要移除的TorrentPeer的PeerInfo。</param>
        public void Remove(PeerInfo peerInfo)
        {
            if (_peers.TryRemove(peerInfo, out var peer))
            {
                peer.Dispose();
            }
        }
    }
}
