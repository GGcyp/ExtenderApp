using MainApp.Common;
using MainApp.Common.Event;

namespace MainApp.IRole
{
    public interface IEventSystem
    {
        TEvent GetEvent<TEvent>() where TEvent : class,IEvent, new();

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="delegateReference"></param>
        /// <param name="delegate"></param>
        /// <param name="forceOption">强制所有监听者使用某个线程</param>
        void Publish(ThreadOption threadOption, Action action);
        /// <summary>
        /// 注册将要分发事件
        /// </summary>
        /// <param name="data"></param>
        void ReferncePublish(IPublishModel data, BaseEvent baseEvent);
    }
}
