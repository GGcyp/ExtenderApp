using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 观察-消费（发布-订阅）消息服务接口。
    /// 支持消息订阅、取消订阅和消息发布。
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// 订阅指定类型的消息。
        /// 当消息发布时，eventHandler 会被调用。
        /// target 用于追踪订阅者对象的生命周期（弱引用），防止内存泄漏。
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="target">订阅者对象（用于弱引用跟踪）</param>
        /// <param name="eventHandler">消息处理委托</param>
        /// <returns>订阅句柄 <see cref="MessageHandle"/>，可用于取消订阅</returns>
        MessageHandle Subscribe<TMessage>(object target, EventHandler<TMessage> eventHandler);

        /// <summary>
        /// 订阅指定名称的消息（字符串方式）
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="messageName">消息名称标识</param>
        /// <param name="target">订阅者对象（用于弱引用跟踪）</param>
        /// <param name="eventHandler">消息处理委托</param>
        /// <returns>订阅句柄，可用于取消订阅</returns>
        MessageHandle Subscribe<TMessage>(string messageName, object target, EventHandler<TMessage> eventHandler);

        /// <summary>
        /// 通过订阅句柄取消指定类型消息的订阅。
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="handle">订阅句柄 <see cref="MessageHandle"/></param>
        /// <returns>是否成功取消订阅</returns>
        bool Unsubscribe(MessageHandle handle);

        /// <summary>
        /// 通过订阅者对象取消指定类型消息的所有订阅。
        /// </summary>
        /// <param name="messageType">消息类型</param>
        /// <param name="target">订阅者对象</param>
        /// <returns>是否成功取消订阅</returns>
        bool Unsubscribe(Type messageType, object target);

        /// <summary>
        /// 通过订阅者对象取消指定类型消息的所有订阅。
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="target">订阅者对象</param>
        /// <returns>是否成功取消订阅</returns>
        bool Unsubscribe<TMessage>(object target);

        /// <summary>
        /// 通过消息处理委托取消指定类型消息的订阅。
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="eventHandler">消息处理委托</param>
        /// <returns>是否成功取消订阅</returns>
        bool Unsubscribe<TMessage>(EventHandler<TMessage> eventHandler);

        /// <summary>
        /// 通过订阅者对象和消息处理委托取消指定类型消息的订阅。
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="target">订阅者对象</param>
        /// <param name="eventHandler">指定委托</param>
        /// <returns></returns>
        bool Unsubscribe<TMessage>(object target, EventHandler<TMessage> eventHandler);

        /// <summary>
        /// 取消指定对象的所有消息订阅（所有类型）。
        /// </summary>
        /// <param name="target">订阅者对象</param>
        /// <returns>是否成功取消所有订阅</returns>
        bool UnsubscribeAll(object target);

        /// <summary>
        /// 发布消息给所有订阅者。
        /// 所有已订阅该类型消息的处理委托都会被调用。
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="message">消息实例</param>
        void Publish<TMessage>(TMessage message);

        /// <summary>
        /// 发布消息给所有订阅者。
        /// 所有已订阅该类型消息的处理委托都会被调用。
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">消息实例</param>
        void Publish<TMessage>(object sender, TMessage message);
    }
}
