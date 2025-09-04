

namespace ExtenderApp.Services.Messages
{
    public abstract class MessagePublisher
    {
        public void Publish(object message)
        {
            var messageType = message.GetType();
            var method = GetType().GetMethod("Publish", new Type[] { messageType });
            if (method != null)
            {
                method.Invoke(this, new object[] { message });
            }
            else
            {
                throw new InvalidOperationException($"No Publish method found for message type {messageType}");
            }
        }
    }

    public class MessagePublisher<TMessage> : MessagePublisher
    {
        private readonly List<MessageConsumeInfo<TMessage>> _subscribers;

        public MessagePublisher()
        {
            _subscribers = new();
        }
    }
}
