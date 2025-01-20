using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

namespace ExtenderApp.Service
{
    /// <summary>
    /// 定时任务服务类
    /// </summary>
    public class ScheduledTaskService : IScheduledTaskService
    {
        /// <summary>
        /// 定时任务对象池
        /// </summary>
        private readonly ObjectPool<ScheduledTask> _pool;

        /// <summary>
        /// 初始化ScheduledTaskService实例
        /// </summary>
        /// <param name="pool">定时任务对象池</param>
        public ScheduledTaskService(ObjectPool<ScheduledTask> pool)
        {
            _pool = pool;
        }

        public ExtenderCancellationToken Start(Action<object> callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            var task = GetScheduledTask();
            task.Start(callback, state, dueTime, period);
            return task.Token;
        }

        /// <summary>
        /// 获取一个定时任务实例
        /// </summary>
        /// <returns>定时任务实例</returns>
        private ScheduledTask GetScheduledTask()
        {
            var result = _pool.Get();
            result.ReleaseAction = _pool.Release;
            return result;
        }
    }
}
