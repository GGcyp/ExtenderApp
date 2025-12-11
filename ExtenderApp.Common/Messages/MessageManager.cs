using System.Collections.Concurrent;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Messages
{
    /// <summary>
    /// 负责管理消息的订阅、发布和取消订阅。此类是线程安全的。
    /// </summary>
    public class MessageManager
    {
        /// <summary>
        /// 存储所有消息类型对应的发布者实例，保证线程安全。
        /// </summary>
        private readonly ConcurrentDictionary<string, MessagePublisher> _messagePublishers;

        /// <summary>
        /// 初始化 MessageManager 类的新实例。
        /// </summary>
        public MessageManager()
        {
            _messagePublishers = new();
        }

        /// <summary>
        /// 发布消息给所有订阅者，并指定消息发送者。
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">消息实例</param>
        public void Publish<TMessage>(object sender, TMessage message)
        {
            if (TryGetMessagePublisher<TMessage>(out var publisher))
            {
                publisher!.Publish(sender, message);
            }
        }

        /// <summary>
        /// 订阅指定类型的消息。
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="target">订阅者对象（用于弱引用跟踪）</param>
        /// <param name="eventHandler">消息处理委托</param>
        /// <returns>订阅句柄 <see cref="MessageHandle"/>，可用于取消订阅</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="target"/> 或 <paramref name="eventHandler"/> 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当同一消息名称被注册为不同类型时触发。</exception>
        public MessageHandle Subscribe<TMessage>(object target, EventHandler<TMessage> eventHandler)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (eventHandler == null)
                throw new ArgumentNullException(nameof(eventHandler));

            var messageName = GetMessageName<TMessage>();
            var publisher = _messagePublishers.GetOrAdd(messageName, _ => new MessagePublisher<TMessage>());

            if (publisher is MessagePublisher<TMessage> typedPublisher)
            {
                return typedPublisher.Subscribe(target, eventHandler);
            }

            throw new InvalidOperationException($"消息 '{messageName}' 已被注册为类型 '{publisher.MessageType.FullName}'，无法再注册为 '{typeof(TMessage).FullName}'。");
        }

        /// <summary>
        /// 通过消息名称订阅指定类型的消息。
        /// </summary>
        /// <param name="messageName">消息名称</param>
        /// <param name="target">目标实例</param>
        /// <param name="eventHandler">消息委托</param>
        /// <returns>消息句柄</returns>
        /// <exception cref="ArgumentNullException">当消息名称、目标实例及消息回调为空时触发</exception>
        /// <exception cref="InvalidOperationException">当同一消息名称被注册为不同类型时触发</exception>
        public MessageHandle Subscribe(string messageName, object target, EventHandler<object> eventHandler)
        {
            if (string.IsNullOrEmpty(messageName))
                throw new ArgumentNullException(nameof(messageName));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (eventHandler == null)
                throw new ArgumentNullException(nameof(eventHandler));

            var publisher = _messagePublishers.GetOrAdd(messageName, _ => new MessagePublisher<object>());

            if (publisher is MessagePublisher<object> typedPublisher)
            {
                return typedPublisher.Subscribe(target, eventHandler);
            }

            throw new InvalidOperationException($"消息 '{messageName}' 已被注册为类型 '{publisher.MessageType.FullName}'，无法再注册为 '{typeof(object).FullName}'。");
        }

        /// <summary>
        /// 通过订阅句柄取消消息订阅。
        /// </summary>
        /// <param name="handle">订阅句柄</param>
        /// <returns>如果订阅不再存在（已被移除或从未订阅），则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        public bool Unsubscribe(MessageHandle handle)
        {
            if (handle.IsEmpty)
                return false;

            if (!_messagePublishers.TryGetValue(handle.MessageName, out var publisher))
                return true;

            return publisher.Unsubscribe(handle.SubscriptionId);
        }

        /// <summary>
        /// 通过订阅者对象取消指定类型消息的订阅。
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="target">订阅者对象</param>
        /// <returns>如果订阅不再存在（已被移除或从未订阅），则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        public bool Unsubscribe<TMessage>(object target)
        {
            if (target == null)
                return true;

            if (!TryGetMessagePublisher<TMessage>(out var publisher))
                return true;

            return publisher.Unsubscribe(target);
        }

        /// <summary>
        /// 通过消息处理委托取消指定类型消息的订阅。
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="eventHandler">消息处理委托</param>
        /// <returns>如果订阅不再存在（已被移除或从未订阅），则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        public bool Unsubscribe<TMessage>(EventHandler<TMessage> eventHandler)
        {
            if (eventHandler == null)
                return true;
            if (!TryGetMessagePublisher<TMessage>(out var publisher))
                return true;
            return publisher.Unsubscribe(eventHandler);
        }

        /// <summary>
        /// 通过消息类型和订阅者对象取消订阅。
        /// </summary>
        /// <param name="messageType">消息类型</param>
        /// <param name="target">订阅者对象</param>
        /// <returns>如果订阅不再存在（已被移除或从未订阅），则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        public bool Unsubscribe(Type messageType, object target)
        {
            if (messageType == null)
                throw new ArgumentNullException(nameof(messageType));
            if (target == null)
                return true;
            if (!TryGetMessagePublisher(messageType, out var publisher))
                return true;
            return publisher.Unsubscribe(target);
        }

        /// <summary>
        /// 通过订阅者对象和消息处理委托取消指定类型消息的订阅。
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="target">订阅者对象</param>
        /// <param name="eventHandler">消息处理委托</param>
        /// <returns>如果订阅不再存在（已被移除或从未订阅），则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        public bool Unsubscribe<TMessage>(object target, EventHandler<TMessage> eventHandler)
        {
            if (target == null)
                return true;
            if (eventHandler == null)
                return true;
            if (!TryGetMessagePublisher<TMessage>(out var publisher))
                return true;

            return publisher.Unsubscribe(target, eventHandler);
        }

        /// <summary>
        /// 取消指定对象的所有消息订阅（所有类型）。
        /// </summary>
        /// <param name="target">订阅者对象</param>
        /// <returns>如果订阅不再存在（已被移除或从未订阅），则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        public bool UnsubscribeAll(object target)
        {
            if (target == null)
                return true;

            foreach (var publisher in _messagePublishers.Values)
            {
                publisher.Unsubscribe(target);
            }
            return true;
        }

        /// <summary>
        /// 获取指定消息类型的名称，用作字典键。
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <returns>消息类型的全名或名称。</returns>
        private static string GetMessageName<TMessage>()
        {
            var messageType = typeof(TMessage);
            return messageType.FullName ?? messageType.Name;
        }

        /// <summary>
        /// 尝试获取指定类型的消息发布者实例。
        /// </summary>
        /// <typeparam name="TMessage">指定类型</typeparam>
        /// <param name="publisher">如果找到，则返回消息发布者实例；否则返回 null。</param>
        /// <returns>如果成功找到并转换了发布者，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        private bool TryGetMessagePublisher<TMessage>(out MessagePublisher<TMessage> publisher)
        {
            if (_messagePublishers.TryGetValue(GetMessageName<TMessage>(), out var basePublisher))
            {
                publisher = (basePublisher as MessagePublisher<TMessage>)!;
                return publisher is not null;
            }
            publisher = default!;
            return false;
        }

        /// <summary>
        /// 尝试根据消息类型获取对应的消息发布器
        /// </summary>
        /// <param name="messageType">消息类型</param>
        /// <param name="publisher">输出参数，返回找到的消息发布器</param>
        /// <returns>如果找到对应的消息发布器则返回true，否则返回false</returns>
        private bool TryGetMessagePublisher(Type messageType, out MessagePublisher publisher)
        {
            // 使用类型的全名作为键查找发布器，如果全名为空则使用类型名
            return _messagePublishers.TryGetValue(messageType.FullName ?? messageType.Name, out publisher!);
        }
    }
}