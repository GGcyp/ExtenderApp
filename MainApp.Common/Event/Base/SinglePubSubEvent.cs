

using MainApp.Common;
using MainApp.IRole;

namespace MainApp.Common.Event
{
    /// <summary>
    /// 唯一订阅者
    /// </summary>
    public class SinglePubSubEvent : SingletonEvent<Action>
    {
        /// <summary>
        /// 订阅消息事件
        /// </summary>
        /// <param name="subAction"></param>
        public void Subscription(Action subAction)
        {
            Subscription(subAction, ThreadOption.PublisherThread);
        }

        /// <summary>
        /// 订阅消息事件
        /// </summary>
        /// <param name="subAction"></param>
        /// <param name="eventThreadOption"></param>
        public void Subscription(Action subAction, ThreadOption eventThreadOption)
        {
            ProtectedSubscription(new ActionEventReferenceData<Action>(subAction, eventThreadOption));
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        public void Publish(ThreadOption forceOption = ThreadOption.None)
        {
            PublishModel model = GetMode(forceOption);
            ReferncePublish(model);
        }

        public override void Distribute(IPublishModel model)
        {
            Publish(model);
        }

        public void PublishNow(ThreadOption forceOption = ThreadOption.None)
        {
            PublishModel model = GetMode(forceOption);
            Distribute(model);
        }

        private PublishModel GetMode(ThreadOption forceOption)
        {
            PublishModel model = new PublishModel();

            model.forceOption = forceOption;
            return model;
        }
    }

    /// <summary>
    /// 唯一订阅者
    /// </summary>
    public class SinglePubSubEvent<T> : SingletonEvent<Action<T>>
    {
        public void Subscription(Action<T> action)
        {
            Subscription(action, ThreadOption.PublisherThread);
        }

        public void Subscription(Action<T> action, ThreadOption threadOption)
        {
            ProtectedSubscription(new ActionEventReferenceData<Action<T>>(action, threadOption));
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message"></param>
        public virtual void Publish(T message, ThreadOption forceOption = ThreadOption.None)
        {
            PublishModel<T> model = GetMode(message, forceOption);
            ReferncePublish(model);
        }

        public override void Distribute(IPublishModel model)
        {
            Publish(model);
        }

        public void PublishNow(T message, ThreadOption forceOption = ThreadOption.None)
        {
            PublishModel<T> model = GetMode(message, forceOption);
            Distribute(model);
        }

        private PublishModel<T> GetMode(T message, ThreadOption forceOption)
        {
            PublishModel<T> model = new PublishModel<T>();

            model.forceOption = forceOption;
            model.eventData = message;
            return model;
        }
    }

    public class SinglePubSubEvent<T1,T2> : SingletonEvent<Action<T1, T2>>
    {
        /// <summary>
        /// 订阅消息事件
        /// </summary>
        /// <param name="subAction"></param>
        /// <returns>返回订阅令牌</returns>
        public void Subscription(Action<T1, T2> subAction)
        {
            Subscription(subAction, ThreadOption.PublisherThread);
        }

        /// <summary>
        /// 订阅消息事件
        /// </summary>
        /// <param name="subAction"></param>
        /// <param name="eventThreadOption"></param>
        /// <returns>返回订阅令牌</returns>
        public void Subscription(Action<T1, T2> subAction, ThreadOption eventThreadOption)
        {
            ProtectedSubscription(new ActionEventReferenceData<Action<T1, T2>>(subAction, eventThreadOption));
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message1"></param>
        /// <param name="message2"></param>
        public virtual void Publish(T1 message1, T2 message2, ThreadOption forceOption = ThreadOption.None)
        {
            PublishModel<T1, T2> model = GetMode(message1, message2, forceOption);
            ReferncePublish(model);
        }

        public override void Distribute(IPublishModel model)
        {
            Publish(model);
        }

        public void PublishNow(T1 message1, T2 message2, ThreadOption forceOption = ThreadOption.None)
        {
            PublishModel<T1, T2> model = GetMode(message1, message2, forceOption);
            Distribute(model);
        }

        private PublishModel<T1, T2> GetMode(T1 message1, T2 message2, ThreadOption forceOption)
        {
            PublishModel<T1, T2> model = new PublishModel<T1, T2>();

            model.forceOption = forceOption;
            model.eventData1 = message1;
            model.eventData2 = message2;
            return model;
        }
    }

    public class SinglePubSubEvent<T1, T2, T3> : SingletonEvent<Action<T1, T2, T3>>
    {
        /// <summary>
        /// 订阅消息事件
        /// </summary>
        /// <param name="subAction"></param>
        /// <returns>返回订阅令牌</returns>
        public void Subscription(Action<T1, T2, T3> subAction)
        {
            Subscription(subAction, ThreadOption.PublisherThread);
        }

        /// <summary>
        /// 订阅消息事件
        /// </summary>
        /// <param name="subAction"></param>
        /// <param name="eventThreadOption"></param>
        /// <returns>返回订阅令牌</returns>
        public void Subscription(Action<T1, T2, T3> subAction, ThreadOption eventThreadOption)
        {
            ProtectedSubscription(new ActionEventReferenceData<Action<T1, T2, T3>>(subAction, eventThreadOption));
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message1"></param>
        /// <param name="message2"></param>
        public virtual void Publish(T1 message1, T2 message2, T3 message3, ThreadOption forceOption = ThreadOption.None)
        {
            PublishModel<T1, T2, T3> model = GetMode(message1, message2, message3, forceOption);
            ReferncePublish(model);
        }

        public override void Distribute(IPublishModel model)
        {
            Publish(model);
        }

        public void PublishNow(T1 message1, T2 message2, T3 message3, ThreadOption forceOption = ThreadOption.None)
        {
            PublishModel<T1, T2, T3> model = GetMode(message1, message2, message3, forceOption);
            Distribute(model);
        }

        private PublishModel<T1, T2, T3> GetMode(T1 message1, T2 message2, T3 message3, ThreadOption forceOption)
        {
            PublishModel<T1, T2, T3> model = new PublishModel<T1, T2, T3>();

            model.forceOption = forceOption;
            model.eventData1 = message1;
            model.eventData2 = message2;
            model.eventData3 = message3;
            return model;
        }
    }

    public class SinglePubSubEvent<T1, T2, T3, T4> : SingletonEvent<Action<T1, T2, T3, T4>>
    {
        /// <summary>
        /// 订阅消息事件
        /// </summary>
        /// <param name="subAction"></param>
        /// <returns>返回订阅令牌</returns>
        public void Subscription(Action<T1, T2, T3, T4> subAction)
        {
            Subscription(subAction, ThreadOption.PublisherThread);
        }

        /// <summary>
        /// 订阅消息事件
        /// </summary>
        /// <param name="subAction"></param>
        /// <param name="eventThreadOption"></param>
        /// <returns>返回订阅令牌</returns>
        public void Subscription(Action<T1, T2, T3, T4> subAction, ThreadOption eventThreadOption)
        {
            ProtectedSubscription(new ActionEventReferenceData<Action<T1, T2, T3, T4>>(subAction, eventThreadOption));
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message1"></param>
        /// <param name="message2"></param>
        public virtual void Publish(T1 message1, T2 message2, T3 message3, T4 message4, ThreadOption forceOption = ThreadOption.None)
        {
            PublishModel<T1, T2, T3, T4> model = GetMode(message1, message2, message3, message4, forceOption);
            ReferncePublish(model);
        }

        public override void Distribute(IPublishModel model)
        {
            Publish(model);
        }

        public void PublishNow(T1 message1, T2 message2, T3 message3, T4 message4, ThreadOption forceOption = ThreadOption.None)
        {
            PublishModel<T1, T2, T3, T4> model = GetMode(message1, message2, message3, message4, forceOption);

            Distribute(model);
        }

        private PublishModel<T1, T2, T3, T4> GetMode(T1 message1, T2 message2, T3 message3, T4 message4, ThreadOption forceOption)
        {
            PublishModel<T1, T2, T3, T4> model = new PublishModel<T1, T2, T3, T4>();

            model.forceOption = forceOption;
            model.eventData1 = message1;
            model.eventData2 = message2;
            model.eventData3 = message3;
            model.eventData4 = message4;
            return model;
        }
    }
}
