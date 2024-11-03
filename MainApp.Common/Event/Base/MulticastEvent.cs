using MainApp.Common.Data;
using MainApp.IRole;

namespace MainApp.Common.Event
{
    /// <summary>
    /// 多播委托
    /// </summary>
    public abstract class MulticastEvent<T> : BaseEvent where T : Delegate
    {
        /// <summary>
        /// 注册列表
        /// </summary>
        private ValueList<ActionEventReferenceData<T>> m_RefernceList;

        public override bool hasSub => m_RefernceList.Count > 0;

        public MulticastEvent()
        {
            m_RefernceList =new ValueList<ActionEventReferenceData<T>>();
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="referenceData"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected void ProtectedSubscription(ActionEventReferenceData<T> referenceData)
        {
            if (referenceData.isEmpty) throw new ArgumentNullException("事件订阅函数不能为空");

            lock (m_RefernceList.LockObject)
            {
                m_RefernceList.Add(referenceData);
            }
        }

        /// <summary>
        /// 退订事件
        /// </summary>
        /// <param name="subscriptionToken"></param>
        public void Unsubscribe(T action)
        {
            ProtectedUnsubscribe(new ActionEventReferenceData<T>(action));
        }

        /// <summary>
        /// 退订事件
        /// </summary>
        /// <param name="data"></param>
        protected void ProtectedUnsubscribe(ActionEventReferenceData<T> data)
        {
            lock (m_RefernceList.LockObject)
            {
                m_RefernceList.Remove(data);
            }
        }

        public bool Contains(T subscriber)
        {
            return Contains(new ActionEventReferenceData<T>(subscriber));
        }

        public bool Contains(ActionEventReferenceData<T> data)
        {
            lock (m_RefernceList.LockObject)
            {
                data = m_RefernceList.FirstOrDefault((refernce, data) => refernce.Equals(data), data);
            }
            return !data.isEmpty;
        }

        protected void LoopReferenceList(IPublishModel model, Func<IPublishModel, IPublishModel> func)
        {
            try
            {
                lock (m_RefernceList.LockObject)
                {
                    for (int i = 0; i < m_RefernceList.Count; i++)
                    {
                        var tempModel = func(model);
                        ThreadOption threadOption = tempModel.GetPublishAction(m_RefernceList[i], out Action action);
                        eventSystem.Publish(threadOption, action);
                    }

                    //看最后一个是否
                    if (m_RefernceList.Count - 1 > 0)
                    {
                        ThreadOption threadOption = model.GetPublishAction(m_RefernceList[m_RefernceList.Count - 1], out Action action);
                        eventSystem.Publish(threadOption, action);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageLogger.Print(ex);
            }
        }
    }
}
