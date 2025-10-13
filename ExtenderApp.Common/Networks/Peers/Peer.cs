using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 表示对等体（Peer）的类。
    /// </summary>
    public class Peer : DisposableObject
    {
        /// <summary>
        /// 获取对等体的信息。
        /// </summary>
        public PeerInfo PeerInfo { get; protected set; }

        /// <summary>
        /// 获取对等体的连接状态。
        /// </summary>
        public LinkState Status { get; protected set; }

        public Peer()
        {
            Status = LinkState.Unknown;
        }
    }


    /// <summary>
    /// 表示一个对等节点（Peer），用于在网络中与其他节点进行通信。
    /// </summary>
    /// <typeparam name="TLinker">链路接口类型。</typeparam>
    /// <typeparam name="TParser">解析器类型。</typeparam>
    /// <typeparam name="TMessage">消息类型。</typeparam>
    public class Peer<TLinker, TParser, TMessage> : Peer
        where TLinker : ILinker
        where TParser : LinkParser
    {
        /// <summary>
        /// 链路客户端。
        /// </summary>
        protected readonly LinkClient<TLinker, TParser> _linkClient;

        /// <summary>
        /// 初始化 Peer 类的新实例。
        /// </summary>
        /// <param name="linkClient">链路客户端。</param>
        /// <param name="peerInfo">对等节点信息。</param>
        public Peer(LinkClient<TLinker, TParser> linkClient)
        {
            _linkClient = linkClient;
        }

        #region Send

        /// <summary>
        /// 发送消息到对等节点。
        /// </summary>
        /// <param name="message">要发送的消息。</param>
        public void Send(TMessage message)
        {
            
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            _linkClient?.Dispose();
        }
    }
}
