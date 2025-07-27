using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Torrent
{
    public class TorrentPeerProvider
    {
        private readonly LocalTorrentInfo _localTorrentInfo;
        private readonly IFileOperateProvider _fileOperateProvider;
        private readonly ConcurrentDictionary<InfoHash, InfoHashPeerStore> _infoHashPeerDict;
        private readonly LinkClientFactory _linkerClientFactory;
        private readonly ConcurrentQueue<LinkClient<ITcpLinker, BTMessageParser>> _linkerClientPool;

        public TorrentPeerProvider(LinkClientFactory linkerClientFactory, LocalTorrentInfo localTorrentInfo, IFileOperateProvider fileOperateProvider)
        {
            _linkerClientFactory = linkerClientFactory;
            _localTorrentInfo = localTorrentInfo;
            _fileOperateProvider = fileOperateProvider;

            _infoHashPeerDict = new();
            _linkerClientPool = new();
        }

        public InfoHashPeerStore GetInfoHashPeerStore(InfoHash infoHash)
        {
            if (_infoHashPeerDict.TryGetValue(infoHash, out var store))
                return store;

            lock (_infoHashPeerDict)
            {
                if (_infoHashPeerDict.TryGetValue(infoHash, out store))
                    return store;

            }
            throw new InvalidOperationException($"未找到指定种子哈希值的peer库{infoHash},请先创建");
        }

        public InfoHashPeerStore CreateInfoHashPeerStore(InfoHash infoHash, TorrentFileInfoNodeParent parent)
        {
            if (_infoHashPeerDict.TryGetValue(infoHash, out var store))
                return store;

            lock (_infoHashPeerDict)
            {
                if (_infoHashPeerDict.TryGetValue(infoHash, out store))
                    return store;

                store = new(infoHash, this, parent);
                _infoHashPeerDict.TryAdd(infoHash, store);
            }
            return store;
        }

        public TorrentPeer? CreatePeer(PeerInfo peerInfo, TorrentFileInfoNodeParent parent)
        {
            if (peerInfo.IsEmpty)
                return null;

            return CreatePeer(peerInfo.PeerAddress, parent);
        }

        public TorrentPeer? CreatePeer(PeerAddress address, TorrentFileInfoNodeParent parent)
        {
            if (!TryConnectPeer(address, out var linkClient))
                return null;

            var peer = new TorrentPeer(linkClient, address, _localTorrentInfo.Id, parent, _fileOperateProvider);
            return peer;
        }

        public TorrentPeer? CreatePeer(ITcpLinker linker, TorrentFileInfoNodeParent parent)
        {
            var linkClient = _linkerClientFactory.Create<ITcpLinker, BTMessageParser>(linker);

            var peer = new TorrentPeer(linkClient, new PeerAddress((IPEndPoint)linker.RemoteEndPoint), _localTorrentInfo.Id, parent, _fileOperateProvider);
            return peer;
        }

        public void AddPeerToStore(ITcpLinker linker)
        {
            var linkClient = _linkerClientFactory.Create<ITcpLinker, BTMessageParser>(linker);
            linkClient.Parser.OnHandshake += (h, i) =>
            {
                if (!_infoHashPeerDict.TryGetValue(h, out var store))
                {
                    linkClient.Close();
                    linkClient.Dispose();
                    return;
                }

                var ipEndPoint = linker.RemoteEndPoint as IPEndPoint;
                PeerAddress address = new PeerAddress();
                if (ipEndPoint != null)
                {
                    address = new PeerAddress(ipEndPoint);
                }
                var peer = new TorrentPeer(linkClient, new PeerAddress((IPEndPoint)linker.RemoteEndPoint), _localTorrentInfo.Id, store.Parent, _fileOperateProvider);
                store.Add(peer, false);
            };
        }

        private bool TryConnectPeer(PeerAddress address, out LinkClient<ITcpLinker, BTMessageParser> linkClient)
        {
            if (!_linkerClientPool.TryDequeue(out linkClient))
            {
                linkClient = _linkerClientFactory.Create<ITcpLinker, BTMessageParser>();
            }

            try
            {
                linkClient.Connect(address.IP, address.Port);
            }
            catch (SocketException ex)
            {
                //处理连接异常
                //throw new InvalidOperationException($"连接到 {address} 失败: {ex.Message}", ex);
                _linkerClientPool.Enqueue(linkClient);
                linkClient = null;
                return false;
            }
            catch (Exception ex)
            {
                // 处理其他异常
                throw new InvalidOperationException($"连接到 {address} 失败: {ex.Message}", ex);
            }
            return true;
        }
    }
}
