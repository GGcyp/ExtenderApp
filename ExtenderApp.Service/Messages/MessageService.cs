using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Services.Messages;
using Microsoft.Extensions.Logging;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 消息服务实现类，负责管理消息的发布、订阅和取消订阅。 支持多类型消息的线程安全发布-订阅机制。
    /// </summary>
    internal class MessageService : IMessageService
    {
        /// <summary>
        /// 存储所有消息类型对应的发布者实例，保证线程安全。
        /// </summary>
        private readonly ConcurrentDictionary<string, MessagePublisher> _messagePublishers;

        /// <summary>
        /// 日志服务实例，用于记录消息相关日志。
        /// </summary>
        private readonly ILogger<IMessageService> _logger;

        /// <summary>
        /// 初始化消息服务实例。
        /// </summary>
        /// <param name="logingService">日志服务实例</param>
        public MessageService(ILogger<IMessageService> logger)
        {
            _messagePublishers = new();
            _logger = logger;
        }

        /// <summary>
        /// 发布消息给所有订阅者（无发送者信息）。
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="message">消息实例</param>
        public void Publish<TMessage>(TMessage message)
        {
            Publish(null, message);
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
        public MessageHandle Subscribe<TMessage>(object target, EventHandler<TMessage> eventHandler)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (eventHandler == null)
                throw new ArgumentNullException(nameof(eventHandler));
            if (!TryGetMessagePublisher<TMessage>(out var publisher))
            {
                return MessageHandle.Empty;
            }
            return publisher.Subscribe(target, eventHandler);
        }

        public MessageHandle Subscribe<TMessage>(string messageName, object target, EventHandler<TMessage> eventHandler)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (eventHandler == null)
                throw new ArgumentNullException(nameof(eventHandler));
            if (!TryGetMessagePublisher<TMessage>(out var publisher))
            {
                return MessageHandle.Empty;
            }
            return publisher.Subscribe(target, eventHandler);
        }

        /// <summary>
        /// 通过订阅句柄取消消息订阅。
        /// </summary>
        /// <param name="handle">订阅句柄</param>
        /// <returns>是否成功取消订阅</returns>
        public bool Unsubscribe(MessageHandle handle)
        {
            if (handle.IsEmpty)
                return false;

            if (!_messagePublishers.TryGetValue(handle.MessageName, out var publisher))
                return true;

            return publisher.Unsubscribe(handle);
        }

        /// <summary>
        /// 通过订阅者对象取消指定类型消息的订阅。
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="target">订阅者对象</param>
        /// <returns>是否成功取消订阅</returns>
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
        /// <returns>是否成功取消订阅</returns>
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
        /// <returns>是否成功取消订阅</returns>
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
        /// <returns>是否成功取消所有订阅</returns>
        public bool UnsubscribeAll(object target)
        {
            foreach (var publisher in _messagePublishers.Values)
            {
                publisher.Unsubscribe(target);
            }
            return true;
        }

        /// <summary>
        /// 尝试获取指定类型的消息发布者实例。
        /// </summary>
        /// <typeparam name="TMessage">指定类型</typeparam>
        /// <param name="publisher">消息发布者实例</param>
        /// <returns>是否获取成功</returns>
        private bool TryGetMessagePublisher<TMessage>(out MessagePublisher<TMessage> publisher)
        {
            Type messageType = typeof(TMessage);
            return TryGetMessagePublisher(messageType.FullName ?? messageType.Name, out publisher);
        }

        /// <summary>
        /// 尝试获取指定类型的消息发布者实例
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="messageName"></param>
        /// <param name="publisher"></param>
        /// <returns></returns>
        private bool TryGetMessagePublisher<TMessage>(string messageName, out MessagePublisher<TMessage> publisher)
        {
            MessagePublisher item = null;
            try
            {
                item = _messagePublishers.GetOrAdd(messageName, _ => new MessagePublisher<TMessage>());
                publisher = (MessagePublisher<TMessage>)item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"指定类型已被注册，需要类型{messageName}，已注册类型{item?.MessageType.FullName}", messageName, item?.MessageType.FullName);
                publisher = null;
                return false;
            }
            return publisher != null;
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
            return TryGetMessagePublisher(messageType.FullName ?? messageType.Name, out publisher);
        }

        /// <summary>
        /// 尝试根据消息名称获取对应的消息发布器
        /// </summary>
        /// <param name="messageName">消息名称</param>
        /// <param name="publisher">输出参数，返回找到的消息发布器</param>
        /// <returns>如果找到对应的消息发布器则返回true，否则返回false</returns>
        private bool TryGetMessagePublisher(string messageName, out MessagePublisher publisher)
        {
            // 从字典中查找指定名称的消息发布器
            return _messagePublishers.TryGetValue(messageName, out publisher);
        }
    }
}