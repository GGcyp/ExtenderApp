namespace ExtenderApp.Data
{
    /// <summary>
    /// 消息订阅句柄，用于标识一次消息订阅。
    /// 包含消息类型和订阅者对象，可用于取消订阅等操作。
    /// </summary>
    public readonly struct MessageHandle : IEquatable<MessageHandle>
    {
        /// <summary>
        /// 为空订阅句柄实例。
        /// </summary>
        public static MessageHandle Empty => new(string.Empty, Guid.Empty);

        /// <summary>
        /// 订阅的消息类型。
        /// </summary>
        public string MessageName { get; }

        /// <summary>
        /// 订阅Id,用于唯一标识一次订阅。
        /// </summary>
        public Guid SubscriptionId { get; }

        /// <summary>
        /// 判断当前实例是否为空订阅句柄。
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(MessageName) && SubscriptionId == Guid.Empty;

        /// <summary>
        /// 初始化 <see cref="MessageHandle"/> 实例。
        /// </summary>
        /// <param name="messageType">消息类型</param>
        /// <param name="subscriptionId">订阅Id</param>
        public MessageHandle(string messageName, Guid subscriptionId)
        {
            MessageName = messageName;
            SubscriptionId = subscriptionId;
        }

        public bool Equals(MessageHandle other)
        {
            return MessageName == other.MessageName && SubscriptionId == other.SubscriptionId;
        }

        public static bool operator ==(MessageHandle left, MessageHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MessageHandle left, MessageHandle right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            return obj is MessageHandle handle && Equals(handle);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MessageName, SubscriptionId);
        }
    }
}