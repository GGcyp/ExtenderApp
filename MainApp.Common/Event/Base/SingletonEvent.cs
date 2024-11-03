
using MainApp.Common;
using MainApp.IRole;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MainApp.Common.Event
{
    /// <summary>
    /// 只能有单个监听者的事件
    /// </summary>
    public abstract class SingletonEvent<T> : BaseEvent where T : Delegate
    {
        private ActionEventReferenceData<T> m_RefernceAction;

        public override bool hasSub => !m_RefernceAction.isEmpty;

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="referenceData"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected void ProtectedSubscription(ActionEventReferenceData<T> referenceData)
        {
            if (referenceData.isEmpty) throw new ArgumentNullException("事件订阅函数不能为空");

            if (!m_RefernceAction.isEmpty) 
                throw new ArgumentNullException($"{GetType()}事件已经被订阅，不能重复订阅SingletonEvent");

            m_RefernceAction = referenceData;
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
            m_RefernceAction = default;
        }

        protected void Publish<T1>(T1 mode) where T1 : IPublishModel
        {
            lock (m_RefernceAction.lockObject)
            {
                ThreadOption threadOption = mode.GetPublishAction(m_RefernceAction, out Action action);
                eventSystem.Publish(threadOption, action);
            }
        }
    }
}
