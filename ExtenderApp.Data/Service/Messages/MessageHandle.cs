

namespace ExtenderApp.Data
{
    /// <summary>
    /// 消息订阅句柄，用于标识一次消息订阅。
    /// 包含消息类型和订阅者对象，可用于取消订阅等操作。
    /// </summary>
    public struct MessageHandle
    {
        /// <summary>
        /// 订阅的消息类型。
        /// </summary>
        public Type MessageType { get; }

        /// <summary>
        /// 订阅Id,用于唯一标识一次订阅。
        /// </summary>
        public Guid SubscriptionId { get; }

        /// <summary>
        /// 初始化 <see cref="MessageHandle"/> 实例。
        /// </summary>
        /// <param name="messageType">消息类型</param>
        /// <param name="subscriptionId">订阅Id</param>
        public MessageHandle(Type messageType, Guid subscriptionId)
        {
            MessageType = messageType;
            SubscriptionId = subscriptionId;
        }
    }
}
