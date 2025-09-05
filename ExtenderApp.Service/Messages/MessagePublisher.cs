using ExtenderApp.Data;

namespace ExtenderApp.Services.Messages
{
    /// <summary>
    /// 消息发布者抽象基类，定义消息类型和取消订阅的基本操作。
    /// </summary>
    public abstract class MessagePublisher
    {
        /// <summary>
        /// 获取当前发布者管理的消息类型。
        /// </summary>
        public abstract Type MessageType { get; }

        /// <summary>
        /// 通过目标对象取消订阅。
        /// </summary>
        /// <param name="target">订阅者对象</param>
        /// <returns>是否成功取消订阅</returns>
        public abstract bool Unsubscribe(object target);

        /// <summary>
        /// 通过订阅唯一标识取消订阅。
        /// </summary>
        /// <param name="consumeId">订阅者唯一标识</param>
        /// <returns>是否成功取消订阅</returns>
        public abstract bool Unsubscribe(Guid consumeId);
    }

    /// <summary>
    /// 泛型消息发布者，负责管理指定类型消息的订阅与发布。
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    public class MessagePublisher<TMessage> : MessagePublisher
    {
        /// <summary>
        /// 当前类型所有订阅者集合。
        /// </summary>
        private readonly List<MessageConsumeInfo<TMessage>> _subscribers;

        /// <summary>
        /// 获取当前发布者管理的消息类型。
        /// </summary>
        public override Type MessageType { get; }

        /// <summary>
        /// 初始化消息发布者。
        /// </summary>
        public MessagePublisher()
        {
            _subscribers = new();
            MessageType = typeof(TMessage);
        }

        /// <summary>
        /// 发布消息给所有存活的订阅者。
        /// 已被回收的订阅者会自动移除。
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">消息内容</param>
        public void Publish(object sender, TMessage message)
        {
            // 遍历所有订阅者并调用其处理方法
            foreach (var subscriber in _subscribers.ToList())
            {
                if (!subscriber.Invoke(sender, message))
                {
                    // 如果订阅者已经不再存活，则将其移除
                    _subscribers.Remove(subscriber);
                }
            }
        }

        /// <summary>
        /// 添加订阅者，订阅指定类型的消息。
        /// </summary>
        /// <param name="target">订阅者对象</param>
        /// <param name="eventHandler">消息处理委托</param>
        /// <returns>订阅句柄 <see cref="MessageHandle"/>，可用于取消订阅</returns>
        /// <exception cref="Exception">如果目标对象已订阅相同处理程序则抛出异常</exception>
        public MessageHandle Subscribe(object target, EventHandler<TMessage> eventHandler)
        {
            foreach (MessageConsumeInfo<TMessage> subscriber in _subscribers)
            {
                if (subscriber.Target == target && subscriber.HandleMessage == eventHandler)
                {
                    throw new Exception($"该目标对象已经订阅了相同的事件处理程序。{target.GetType().FullName} : {eventHandler.GetType().Name}");
                }
            }

            var consumeInfo = new MessageConsumeInfo<TMessage>(target, eventHandler);
            _subscribers.Add(consumeInfo);
            return new MessageHandle(MessageType, consumeInfo.ConsumeId);
        }

        /// <summary>
        /// 通过目标对象取消订阅。
        /// </summary>
        /// <param name="target">订阅者对象</param>
        /// <returns>是否成功取消订阅</returns>
        public override bool Unsubscribe(object target)
        {
            var subscriber = _subscribers.FirstOrDefault(s => s.Target == target);
            return subscriber != null ? _subscribers.Remove(subscriber) : true;
        }

        /// <summary>
        /// 通过订阅唯一标识取消订阅。
        /// </summary>
        /// <param name="consumeId">订阅者唯一标识</param>
        /// <returns>是否成功取消订阅</returns>
        public override bool Unsubscribe(Guid consumeId)
        {
            var subscriber = _subscribers.FirstOrDefault(s => s.ConsumeId == consumeId);
            return subscriber != null ? _subscribers.Remove(subscriber) : true;
        }

        /// <summary>
        /// 通过目标对象和处理委托取消订阅。
        /// </summary>
        /// <param name="target">订阅者对象</param>
        /// <param name="eventHandler">消息处理委托</param>
        /// <returns>是否成功取消订阅</returns>
        public bool Unsubscribe(object target, EventHandler<TMessage> eventHandler)
        {
            var subscriber = _subscribers.FirstOrDefault(s => s.Target == target && s.HandleMessage == eventHandler);
            return subscriber != null ? _subscribers.Remove(subscriber) : true;
        }

        /// <summary>
        /// 通过处理委托取消订阅。
        /// </summary>
        /// <param name="eventHandler">消息处理委托</param>
        /// <returns>是否成功取消订阅</returns>
        public bool Unsubscribe(EventHandler<TMessage> eventHandler)
        {
            var subscriber = _subscribers.FirstOrDefault(s => s.HandleMessage == eventHandler);
            return subscriber != null ? _subscribers.Remove(subscriber) : true;
        }
    }
}
