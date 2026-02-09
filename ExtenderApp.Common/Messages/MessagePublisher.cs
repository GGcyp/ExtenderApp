using System.Collections.Concurrent;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Messages
{
    /// <summary>
    /// 消息发布者抽象基类，定义消息类型和取消订阅的基本操作。
    /// </summary>
    internal abstract class MessagePublisher
    {
        /// <summary>
        /// 获取当前发布者管理的消息类型。
        /// </summary>
        public abstract Type MessageType { get; }

        /// <summary>
        /// 通过目标对象取消订阅。
        /// </summary>
        /// <param name="target">订阅者对象</param>
        /// <returns>如果成功找到并移除了订阅，则返回 true；否则返回 false。</returns>
        public abstract bool Unsubscribe(object target);

        /// <summary>
        /// 通过订阅句柄取消订阅。
        /// </summary>
        /// <param name="handle">要取消的订阅句柄。</param>
        /// <returns>如果成功找到并移除了订阅，则返回 true；否则返回 false。</returns>
        public abstract bool Unsubscribe(Guid subscriptionId);
    }

    /// <summary>
    /// 泛型消息发布者，负责管理指定类型消息的订阅与发布。此类是线程安全的。
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    internal class MessagePublisher<TMessage> : MessagePublisher
    {
        /// <summary>
        /// 使用线程安全的字典来存储订阅者信息，键为订阅ID。
        /// </summary>
        private readonly ConcurrentDictionary<Guid, MessageConsumeInfo<TMessage>> _subscribers;

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
        /// 发布消息给所有存活的订阅者。已被垃圾回收的订阅者会自动移除。
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">消息内容</param>
        public void Publish(object sender, TMessage message)
        {
            // 遍历所有订阅者并调用其处理方法
            foreach (var subscriber in _subscribers.Values)
            {
                if (!subscriber.Invoke(sender, message))
                {
                    // 如果订阅者已经不再存活，则将其从字典中移除
                    _subscribers.TryRemove(subscriber.ConsumeId, out _);
                }
            }
        }

        /// <summary>
        /// 添加订阅者，订阅指定类型的消息。
        /// </summary>
        /// <param name="target">订阅者对象</param>
        /// <param name="eventHandler">消息处理委托</param>
        /// <returns>订阅句柄 <see cref="MessageHandle"/>，可用于取消订阅</returns>
        /// <exception cref="InvalidOperationException">如果目标对象已订阅相同处理程序则抛出异常</exception>
        public MessageHandle Subscribe(object target, EventHandler<TMessage> eventHandler)
        {
            // 检查是否已存在相同的订阅
            var existingSubscriber = _subscribers.Values.FirstOrDefault(s => s.Target == target && s.HandleMessage == eventHandler);
            if (existingSubscriber != null)
            {
                throw new InvalidOperationException($"该目标对象已经订阅了相同的事件处理程序。类型: {target.GetType().FullName}");
            }

            var consumeInfo = new MessageConsumeInfo<TMessage>(target, eventHandler);
            _subscribers.TryAdd(consumeInfo.ConsumeId, consumeInfo);
            return new MessageHandle(MessageType.FullName ?? MessageType.Name, consumeInfo.ConsumeId);
        }

        /// <summary>
        /// 通过目标对象取消其所有订阅。
        /// </summary>
        /// <param name="target">订阅者对象</param>
        /// <returns>如果成功找到并移除了至少一个订阅，则返回 true；否则返回 false。</returns>
        public override bool Unsubscribe(object target)
        {
            var subscribersToRemove = _subscribers.Values.Where(s => s.Target == target).ToList();
            if (!subscribersToRemove.Any())
            {
                return false;
            }

            foreach (var subscriber in subscribersToRemove)
            {
                _subscribers.TryRemove(subscriber.ConsumeId, out _);
            }
            return true;
        }

        /// <summary>
        /// 通过订阅唯一标识取消订阅。
        /// </summary>
        /// <param name="subscriptionId">订阅者唯一标识</param>
        /// <returns>如果成功找到并移除了订阅，则返回 true；否则返回 false。</returns>
        public override bool Unsubscribe(Guid subscriptionId)
        {
            return _subscribers.TryRemove(subscriptionId, out _);
        }

        /// <summary>
        /// 通过目标对象和处理委托取消订阅。
        /// </summary>
        /// <param name="target">订阅者对象</param>
        /// <param name="eventHandler">消息处理委托</param>
        /// <returns>如果成功找到并移除了订阅，则返回 true；否则返回 false。</returns>
        public bool Unsubscribe(object target, EventHandler<TMessage> eventHandler)
        {
            var subscriber = _subscribers.Values.FirstOrDefault(s => s.Target == target && s.HandleMessage == eventHandler);
            return subscriber != null && _subscribers.TryRemove(subscriber.ConsumeId, out _);
        }

        /// <summary>
        /// 通过处理委托取消订阅。
        /// </summary>
        /// <param name="eventHandler">消息处理委托</param>
        /// <returns>如果成功找到并移除了订阅，则返回 true；否则返回 false。</returns>
        public bool Unsubscribe(EventHandler<TMessage> eventHandler)
        {
            var subscriber = _subscribers.Values.FirstOrDefault(s => s.HandleMessage == eventHandler);
            return subscriber != null && _subscribers.TryRemove(subscriber.ConsumeId, out _);
        }
    }
}