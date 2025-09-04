

using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Services
{
    internal class MessageService : IMessageService
    {
        private readonly ConcurrentDictionary<Type, MessagePublisher> _messagePublishers;
        private readonly ILogingService _logingService;

        public MessageService(ILogingService logingService)
        {
            _messagePublishers = new();
            _logingService = logingService;
        }

        public void Publish<TMessage>(TMessage message)
        {
            throw new NotImplementedException();
        }

        public MessageHandle Subscribe<TMessage>(object target, EventHandler<TMessage> eventHandler)
        {
            throw new NotImplementedException();
        }

        public bool Unsubscribe(MessageHandle handle)
        {
            throw new NotImplementedException();
        }

        public bool Unsubscribe<TMessage>(object target)
        {
            throw new NotImplementedException();
        }

        public bool Unsubscribe<TMessage>(EventHandler<TMessage> eventHandler)
        {
            throw new NotImplementedException();
        }

        public bool UnsubscribeAll(object target)
        {
            throw new NotImplementedException();
        }
    }
}
