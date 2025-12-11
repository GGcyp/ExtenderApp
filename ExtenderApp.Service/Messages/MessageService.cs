using System.Runtime.CompilerServices;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Messages;
using ExtenderApp.Data;
using Microsoft.Extensions.Logging;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 消息服务实现类，负责管理消息的发布、订阅和取消订阅。 支持多类型消息的线程安全发布-订阅机制。
    /// </summary>
    internal class MessageService : IMessageService
    {
        private readonly ILogger<IMessageService> _logger;
        private readonly MessageManager _messageManager;

        /// <summary>
        /// 初始化消息服务实例。
        /// </summary>
        /// <param name="logger">日志服务实例</param>
        public MessageService(ILogger<IMessageService> logger)
        {
            _messageManager = new();
            _logger = logger;
        }

        public void Publish<TMessage>(object sender, TMessage message)
        {
            Execute(() => _messageManager.Publish(sender, message));
        }

        public MessageHandle Subscribe<TMessage>(object target, EventHandler<TMessage> eventHandler)
        {
            return Execute(() => _messageManager.Subscribe(target, eventHandler), MessageHandle.Empty);
        }

        public MessageHandle Subscribe(string messageName, object target, EventHandler<object> eventHandler)
        {
            return Execute(() => _messageManager.Subscribe(messageName, target, eventHandler), MessageHandle.Empty);
        }

        public bool Unsubscribe(MessageHandle handle)
        {
            return Execute(() => _messageManager.Unsubscribe(handle));
        }

        public bool Unsubscribe(Type messageType, object target)
        {
            return Execute(() => _messageManager.Unsubscribe(messageType, target));
        }

        public bool Unsubscribe<TMessage>(EventHandler<TMessage> eventHandler)
        {
            return Execute(() => _messageManager.Unsubscribe(eventHandler));
        }

        public bool Unsubscribe<TMessage>(object target, EventHandler<TMessage> eventHandler)
        {
            return Execute(() => _messageManager.Unsubscribe(target, eventHandler));
        }

        public bool UnsubscribeAll(object target)
        {
            return Execute(() => _messageManager.UnsubscribeAll(target));
        }

        private void Execute(Action action, [CallerMemberName] string? operation = null)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "在执行 {Operation} 操作时发生错误。", operation);
            }
        }

        private T Execute<T>(Func<T> func, T defaultValue = default!, [CallerMemberName] string? operation = null)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "在执行 {Operation} 操作时发生错误。", operation);
                return defaultValue;
            }
        }
    }
}