using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// PeerInfo扩展类，提供对ILinker接口的扩展方法。
    /// </summary>
    public static class PeerInfoExtensions
    {
        /// <summary>
        /// 连接到指定的PeerInfo对象所表示的Peer。
        /// </summary>
        /// <param name="linker">ILinker接口的实例。</param>
        /// <param name="peerInfo">要连接的Peer的PeerInfo对象。</param>
        /// <exception cref="ArgumentNullException">如果peerInfo为空，则抛出此异常。</exception>
        public static void Connect(this ILinker linker, PeerInfo peerInfo)
        {
            if (peerInfo.IsEmpty)
            {
                throw new ArgumentNullException(nameof(peerInfo));
            }
            //linker.Connect(peerInfo.IP, peerInfo.Port);
        }

        /// <summary>
        /// 异步连接到一个Peer。
        /// </summary>
        /// <param name="linker">ILinker接口实例。</param>
        /// <param name="peerInfo">包含Peer信息的PeerInfo对象。</param>
        /// <exception cref="ArgumentNullException">如果peerInfo为空，则抛出此异常。</exception>
        public static void ConnectAsync(this ILinker linker, PeerInfo peerInfo)
        {
            if (peerInfo.IsEmpty)
            {
                throw new ArgumentNullException(nameof(peerInfo));
            }
            //linker.ConnectAsync(peerInfo.IP, peerInfo.Port);
        }
    }
}
