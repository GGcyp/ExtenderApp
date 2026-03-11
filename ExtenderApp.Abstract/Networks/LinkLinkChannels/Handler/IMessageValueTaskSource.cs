using System.Threading.Tasks;

namespace ExtenderApp.Abstract.Networks
{
    /// <summary>
    /// 表示一个消息相关的可等待源的标记接口。
    /// 该接口本身不包含成员，主要用于在代码中统一标识所有基于 ValueTask/ValueTask&lt;T&gt; 的消息等待源类型。
    /// </summary>
    public interface IMessageValueTaskSource
    {
    }

    /// <summary>
    /// 表示一个可以异步获取消息结果的可等待源。
    /// 实现者应返回一个 <see cref="ValueTask{TResult}"/>，用于在异步/事件驱动场景中等待消息处理结果。
    /// </summary>
    /// <typeparam name="T">异步操作返回的结果类型。</typeparam>
    public interface IMessageValueTaskSource<T> : IMessageValueTaskSource
    {
        /// <summary>
        /// 获取一个表示异步结果的 <see cref="ValueTask{TResult}"/>。
        /// 当结果可用时该任务应完成并返回对应的值；在等待期间可由生产者在适当时机触发完成。
        /// </summary>
        /// <returns>一个将在结果可用时完成并返回 <typeparamref name="T"/> 的 <see cref="ValueTask{TResult}"/>。</returns>
        public ValueTask<T> GetValueAsync();
    }
}
