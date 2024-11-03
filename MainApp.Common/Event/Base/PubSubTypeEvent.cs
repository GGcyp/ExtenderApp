using MainApp.Common;

namespace MainApp.Common.Event
{
    public class PubSubTypeEvent : PubSubEvent<Type>
    {
        public void Publish<T>(ThreadOption forceOption = ThreadOption.None)
        {
            Publish(typeof(T), forceOption);
        }

        public void PublishNow<T>(ThreadOption forceOption = ThreadOption.None)
        {
            PublishNow(typeof(T), forceOption);
        }
    }

    public class PubSubTypeEvent<T2> : PubSubEvent<Type, T2>
    {
        public void Publish<T>(T2 message, ThreadOption forceOption = ThreadOption.None)
        {
            Publish(typeof(T), message, forceOption);
        }

        public void PublishNow<T>(T2 message, ThreadOption forceOption = ThreadOption.None)
        {
            PublishNow(typeof(T), message, forceOption);
        }
    }
}
