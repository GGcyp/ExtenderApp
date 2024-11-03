using MainApp.Common;
using MainApp.IRole;

namespace MainApp.Common.Event
{
    public class PubSubEvent : MulticastEvent<Action>
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
            PublishModel model = GetModel(forceOption);
            ReferncePublish(model);
        }

        public override void Distribute(IPublishModel model)
        {
            LoopReferenceList(model, GetSameModel);
        }

        public void PublishNow(ThreadOption forceOption = ThreadOption.None)
        {
            PublishModel model = GetModel(forceOption);
            Distribute(model);
        }

        private PublishModel GetModel(ThreadOption forceOption)
        {
            PublishModel model = new PublishModel();

            model.forceOption = forceOption;
            return model;
        }

        /// <summary>
        /// 获得一样的PublishModel
        /// </summary>
        /// <param name="baseModel"></param>
        /// <returns></returns>
        private IPublishModel GetSameModel(IPublishModel iBaseModel)
        {
            var baseModel = iBaseModel as PublishModel;
            var model = new PublishModel();

            model.forceOption = baseModel.forceOption;
            return model;
        }
    }

    public class PubSubEvent<T> : MulticastEvent<Action<T>>
    {
        /// <summary>
        /// 订阅消息事件
        /// </summary>
        /// <param name="subAction"></param>
        /// <returns>返回订阅令牌</returns>
        public void Subscription(Action<T> subAction)
        {
            Subscription(subAction, ThreadOption.PublisherThread);
        }

        /// <summary>
        /// 订阅消息事件
        /// </summary>
        /// <param name="subAction"></param>
        /// <param name="eventThreadOption"></param>
        /// <returns>返回订阅令牌</returns>
        public void Subscription(Action<T> subAction, ThreadOption eventThreadOption)
        {
            ProtectedSubscription(new ActionEventReferenceData<Action<T>>(subAction, eventThreadOption));
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
            LoopReferenceList(model, GetSameModel);
        }

        public void PublishNow(T message, ThreadOption forceOption = ThreadOption.None)
        {
            PublishModel<T> model = GetMode(message, forceOption);
            Distribute(model);
        }

        private PublishModel<T> GetMode( T message, ThreadOption forceOption)
        {
            PublishModel<T> model = new PublishModel<T>();

            model.forceOption = forceOption;
            model.eventData = message;
            return model;
        }

        /// <summary>
        /// 获得一样的PublishModel
        /// </summary>
        /// <param name="iBaseModel"></param>
        /// <returns></returns>
        private IPublishModel GetSameModel(IPublishModel iBaseModel)
        {
            var baseModel = iBaseModel as PublishModel<T>;
            var model = new PublishModel<T>();

            model.forceOption = baseModel.forceOption;
            model.eventData = baseModel.eventData;
            return model;
        }
    }

    public class PubSubEvent<T1, T2> : MulticastEvent<Action<T1, T2>>
    {
        /// <summary>
        /// 订阅消息事件
        /// </summary>
        /// <param name="subAction"></param>
        /// <returns>返回订阅令牌</returns>
        public void Subscription(Action<T1,T2> subAction)
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
            LoopReferenceList(model, GetSameModel);
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

        /// <summary>
        /// 获得一样的PublishModel
        /// </summary>
        /// <param name="iBaseModel"></param>
        /// <returns></returns>
        private PublishModel<T1, T2> GetSameModel(IPublishModel iBaseModel)
        {
            var baseModel = iBaseModel as PublishModel<T1, T2>;
            var model = new PublishModel<T1, T2>();

            model.forceOption = baseModel.forceOption;
            model.eventData1 = baseModel.eventData1;
            model.eventData2 = baseModel.eventData2;
            return model;
        }
    }

    public class PubSubEvent<T1, T2, T3> : MulticastEvent<Action<T1, T2, T3>>
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
            LoopReferenceList(model, GetSameModel);
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

        /// <summary>
        /// 获得一样的PublishModel
        /// </summary>
        /// <param name="iBaseModel"></param>
        /// <returns></returns>
        private IPublishModel GetSameModel(IPublishModel iBaseModel)
        {
            var baseModel = iBaseModel as PublishModel<T1, T2, T3>;
            var model = new PublishModel<T1, T2, T3>();

            model.forceOption = baseModel.forceOption;
            model.eventData1 = baseModel.eventData1;
            model.eventData2 = baseModel.eventData2;
            model.eventData3 = baseModel.eventData3;
            return model;
        }
    }

    public class PubSubEvent<T1, T2, T3, T4> : MulticastEvent<Action<T1, T2, T3, T4>>
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
            LoopReferenceList(model, GetSameModel);
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

        /// <summary>
        /// 获得一样的PublishModel
        /// </summary>
        /// <param name="iBaseModel"></param>
        /// <returns></returns>
        private IPublishModel GetSameModel(IPublishModel iBaseModel)
        {
            var baseModel = iBaseModel as PublishModel<T1, T2, T3, T4>;
            var model = new PublishModel<T1, T2, T3, T4>();

            model.forceOption = baseModel.forceOption;
            model.eventData1 = baseModel.eventData1;
            model.eventData2 = baseModel.eventData2;
            model.eventData3 = baseModel.eventData3;
            model.eventData4 = baseModel.eventData4;
            return model;
        }
    }
}
