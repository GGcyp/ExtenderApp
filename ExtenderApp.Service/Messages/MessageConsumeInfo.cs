namespace ExtenderApp.Services.Messages
{
    /// <summary>
    /// 泛型消息信息类，用于存储对象的弱引用
    /// </summary>
    /// <typeparam name="TValue">引用的对象类型，必须是引用类型</typeparam>
    public class MessageConsumeInfo<TMessage>
    {
        /// <summary>
        /// 获取存储的弱引用对象
        /// </summary>
        private readonly WeakReference<object> _weakReference;

        /// <summary>
        /// 获取消息处理委托
        /// </summary>
        public EventHandler<TMessage> HandleMessage { get; }

        /// <summary>
        /// 消费者唯一标识
        /// </summary>
        public Guid ConsumeId { get; }

        /// <summary>
        /// 检查对象是否还存活（未被垃圾回收）
        /// </summary>
        public bool IsAlive => _weakReference.TryGetTarget(out var target) && target != null;

        /// <summary>
        /// 目标对象的强引用
        /// </summary>
        public object Target { get; }

        /// <summary>
        /// 初始化 MessageInfo 实例
        /// </summary>
        /// <param name="target">要引用的对象</param>
        /// <param name="handleMessage">消息处理委托</param>
        public MessageConsumeInfo(object target, EventHandler<TMessage> handleMessage)
        {
            HandleMessage = handleMessage;
            // 创建对目标对象的弱引用
            _weakReference = new(target);
            Target = target;
            ConsumeId = Guid.NewGuid();
        }

        public bool Invoke(object sender, TMessage message)
        {
            if (!IsAlive)
                return false;

            HandleMessage?.Invoke(sender, message);
            return true;
        }
    }
}