using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定时任务服务接口。
    /// </summary>
    public interface IScheduledTaskService
    {
        /// <summary>
        /// 启动定时任务
        /// </summary>
        /// <param name="callback">回调方法</param>
        /// <param name="state">回调方法的参数</param>
        /// <param name="dueTime">延迟时间</param>
        /// <param name="period">周期时间</param>
        /// <returns>取消令牌</returns>
        ExtenderCancellationToken Start(Action<object> callback, object state, TimeSpan dueTime, TimeSpan period);
    }
}
