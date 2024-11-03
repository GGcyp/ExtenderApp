namespace MainApp.Common
{
    /// <summary>
    /// 消息事件订阅函数索引
    /// </summary>
    public struct ActionEventReferenceData<T> where T : Delegate
    {
        private T m_Action;
        public T target => m_Action;

        private ThreadOption m_EventThreadOption;
        public ThreadOption eventThreadOption => m_EventThreadOption;

        public bool isEmpty => target == null;
        public object lockObject => target;

        public ActionEventReferenceData(T action) : this(action, ThreadOption.PublisherThread)
        {

        }

        public ActionEventReferenceData(T action, ThreadOption eventThreadOption)
        {
            if (action == null)
                throw new ArgumentNullException("不能绑定空的消息值");

            m_EventThreadOption = eventThreadOption;
            m_Action = action;
        }

        public bool Equals(ActionEventReferenceData<T> data)
        {
            return data.target.Equals(target);
        }

        public bool Equals(Action<T> action)
        {
            return action.Equals(target);
        }
    }
}
