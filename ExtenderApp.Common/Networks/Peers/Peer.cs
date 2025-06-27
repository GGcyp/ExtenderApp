using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 表示对等体（Peer）的类。
    /// </summary>
    public class Peer
    {
        /// <summary>
        /// 获取对等体的信息。
        /// </summary>
        public PeerInfo PeerInfo { get; protected set; }

        /// <summary>
        /// 获取对等体的连接状态。
        /// </summary>
        public LinkState Status { get; protected set; }
    }


    /// <summary>
    /// 表示一个对等节点（Peer），用于在网络中与其他节点进行通信。
    /// </summary>
    /// <typeparam name="TLinker">链路接口类型。</typeparam>
    /// <typeparam name="TParser">解析器类型。</typeparam>
    /// <typeparam name="TMessage">消息类型。</typeparam>
    public class Peer<TLinker, TParser, TMessage> : Peer
        where TLinker : ILinker
        where TParser : LinkParser<TMessage>
    {
        /// <summary>
        /// 链路客户端。
        /// </summary>
        protected readonly LinkClient<TLinker, TParser> _linkClient;

        /// <summary>
        /// 当对等节点连接成功时触发的事件。
        /// </summary>
        public event Action<Peer>? OnConnected;

        /// <summary>
        /// 当对等节点接收到消息时触发的事件。
        /// </summary>
        public event Action<Peer, TMessage>? OnMessageReceived;

        /// <summary>
        /// 初始化 Peer 类的新实例。
        /// </summary>
        /// <param name="linkClient">链路客户端。</param>
        /// <param name="peerInfo">对等节点信息。</param>
        public Peer(LinkClient<TLinker, TParser> linkClient, PeerInfo peerInfo)
        {
            _linkClient = linkClient;
            PeerInfo = peerInfo;
            Status = LinkState.Unknown;
            _linkClient.Parser.OnMessageReceived += ReceivedMessage;
        }

        #region Connect

        /// <summary>
        /// 同步连接对等节点。
        /// </summary>
        public void Connect()
        {
            if (_linkClient.Connected)
            {
                return;
            }

            _linkClient.OnConnect += Connected;

            _linkClient.Connect(PeerInfo);
            Status = LinkState.Connecting;
        }

        /// <summary>
        /// 异步连接对等节点。
        /// </summary>
        public void ConnectAsync()
        {
            if (_linkClient.Connected)
            {
                return;
            }
            _linkClient.OnConnect += Connected;
            _linkClient.ConnectAsync(PeerInfo);
            Status = LinkState.Connecting;
        }

        /// <summary>
        /// 处理连接成功的事件。
        /// </summary>
        /// <param name="linker">链路接口。</param>
        private void Connected(ILinker linker)
        {
            Status = LinkState.Ok;
            OnConnected?.Invoke(this);
            _linkClient.OnConnect -= Connected;
        }

        #endregion

        #region Send

        /// <summary>
        /// 发送消息到对等节点。
        /// </summary>
        /// <param name="message">要发送的消息。</param>
        public void Send(TMessage message)
        {
            _linkClient.Send(message);
        }

        #endregion

        #region Received

        /// <summary>
        /// 处理接收到的消息。
        /// </summary>
        /// <param name="message">接收到的消息。</param>
        protected void ReceivedMessage(TMessage message)
        {
            OnMessageReceived?.Invoke(this, message);
        }

        #endregion
    }
}
