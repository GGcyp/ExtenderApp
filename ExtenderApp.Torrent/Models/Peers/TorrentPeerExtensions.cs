using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// TorrentPeer扩展类。
    /// </summary>
    public static class TorrentPeerExtensions
    {
        /// <summary>
        /// 使用指定的<see cref="LinkerClientFactory"/>和<see cref="PeerInfo"/>对象创建一个<see cref="TorrentPeer"/>对象。
        /// </summary>
        /// <param name="factory"><see cref="LinkerClientFactory"/>实例。</param>
        /// <param name="peerInfo"><see cref="PeerInfo"/>实例。</param>
        /// <returns>返回创建的<see cref="TorrentPeer"/>对象。</returns>
        /// <exception cref="ArgumentNullException">如果<paramref name="factory"/>或<paramref name="peerInfo"/>为null，则抛出此异常。</exception>
        /// <exception cref="ArgumentNullException">如果<paramref name="peerInfo"/>为空，则抛出此异常。</exception>
        public static TorrentPeer CreateTorrentPeer(this LinkerClientFactory factory, PeerInfo peerInfo)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            if (peerInfo.IsEmpty)
                throw new ArgumentNullException(nameof(peerInfo));

            var linker = factory.Create<ITcpLinker, BTMessageParser>();
            return new TorrentPeer(linker, peerInfo);
        }
    }
}
