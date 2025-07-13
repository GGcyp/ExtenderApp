using System.Collections.Concurrent;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    public class TorrentPeerProvider
    {
        private readonly LocalTorrentInfo _localTorrentInfo;
        private readonly ConcurrentDictionary<PeerInfo, TorrentPeer> _peerDict;
        private readonly ConcurrentDictionary<PeerAddress, PeerInfo> _peerIdDict;
        private readonly ConcurrentDictionary<InfoHash, InfoHashPeerStore> _infoHashPeerDict;
        private readonly LinkerClientFactory _linkerClientFactory;
        private readonly ConcurrentQueue<LinkClient<ITcpLinker, BTMessageParser>> _linkerClientPool;

        public TorrentPeerProvider(LinkerClientFactory linkerClientFactory, LocalTorrentInfo localTorrentInfo)
        {
            _linkerClientFactory = linkerClientFactory;
            _localTorrentInfo = localTorrentInfo;

            _peerDict = new();
            _peerIdDict = new();
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

                store = new(infoHash, this);
                _infoHashPeerDict.TryAdd(infoHash, store);
            }
            return store;
        }

        public TorrentPeer? GetPeer(PeerAddress address)
        {
            if (_peerIdDict.TryGetValue(address, out var peerInfo))
                return GetPeer(peerInfo);

            lock (_peerIdDict)
            {
                if (_peerIdDict.TryGetValue(address, out peerInfo))
                    return GetPeer(peerInfo);

                var linkClient = ConnectPeer(address);
                //var newPeer = new TorrentPeer(linkClient, _linkerClientFactory, _localTorrentInfo.Id);

                //_peerIdDict.TryAdd(address, peerInfo);
                //_peerDict.TryAdd(peerInfo, newPeer);
                //return newPeer;
                return null;
            }
        }

        public TorrentPeer? GetPeer(PeerInfo peerInfo)
        {
            if (peerInfo.IsEmpty)
                return null;

            if (_peerDict.TryGetValue(peerInfo, out var peer))
            {
                return peer;
            }

            lock (_peerDict)
            {
                if (_peerDict.TryGetValue(peerInfo, out peer))
                    return peer;

                // 如果没有找到，则创建一个新的 TorrentPeer 实例
                var linkClient = ConnectPeer(peerInfo.PeerAddress);
                //var newPeer = new TorrentPeer(linkClient, _linkerClientFactory, _localTorrentInfo.Id);
                //_peerDict.TryAdd(peerInfo, newPeer);
                //return newPeer;
                return null;
            }
        }

        private LinkClient<ITcpLinker, BTMessageParser> ConnectPeer(PeerAddress address)
        {
            LinkClient<ITcpLinker, BTMessageParser> linkClient;
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
                return null;
            }
            catch (Exception ex)
            {
                // 处理其他异常
                throw new InvalidOperationException($"连接到 {address} 失败: {ex.Message}", ex);
            }
            return linkClient;
        }
    }
}
