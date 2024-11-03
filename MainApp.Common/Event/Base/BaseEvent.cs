using MainApp.IRole;

namespace MainApp.Common.Event
{
    public abstract class BaseEvent : IEvent
    {
        private string m_EventName;
        protected string eventName
        {
            get
            {
                if(m_EventName == null)
                {
                    m_EventName = GetType().Name;
                }
                return m_EventName;
            }
        }

        private IEventSystem m_EventSystem;
        public IEventSystem eventSystem
        {
            protected get => m_EventSystem;
            set => m_EventSystem = value;
        }

        /// <summary>
        /// 判断有没有订阅者
        /// </summary>
        public abstract bool hasSub { get; }

        public virtual void Init()
        {
        }

        /// <summary>
        /// 分发
        /// </summary>
        /// <param name="data"></param>
        public abstract void Distribute(IPublishModel data);

        /// <summary>
        /// 向EventSystem登记将要分发
        /// </summary>
        /// <param name="forceOption"></param>
        protected void ReferncePublish(IPublishModel data)
        {
            eventSystem.ReferncePublish(data, this);
        }
    }
}
