using ExtenderApp.Common.ScheduledTasks;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 增强型计划任务（支持精确时间补偿）
    /// </summary>
    public sealed class ScheduledTask : ExtenderOperationController
    {
        #region 内部状态

        /// <summary>
        /// 执行定时器
        /// </summary>
        private readonly Timer _executionTimer;

        /// <summary>
        /// 周期时间
        /// </summary>
        private TimeSpan _period;

        /// <summary>
        /// 剩余等待时间
        /// </summary>
        private TimeSpan _remainingDueTime;

        /// <summary>
        /// 上次暂停时间
        /// </summary>
        private DateTime _lastPauseTime;

        /// <summary>
        /// 用户回调
        /// </summary>
        private Action<object?>? userCallback;

        /// <summary>
        /// 状态对象
        /// </summary>
        private object? _state;
        #endregion

        public ScheduledTask()
        {
            _executionTimer = new Timer(ExecuteCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        #region 任务控制

        /// <summary>
        /// 启动一个带有延迟和周期的定时器，当定时器触发时执行指定的回调方法。
        /// </summary>
        /// <param name="callback">当定时器触发时要执行的回调方法。</param>
        /// <param name="dueTime">定时器触发前的延迟时间（毫秒）。</param>
        /// <param name="period">定时器触发的周期时间（毫秒）。</param>
        public void Start(Action callback, int dueTime, int period)
        {
            Start(o => callback?.Invoke(), null, TimeSpan.FromMilliseconds(dueTime), TimeSpan.FromMilliseconds(period));
        }

        /// <summary>
        /// 启动一个定时任务
        /// </summary>
        /// <param name="callback">定时任务执行完毕后调用的回调函数</param>
        /// <param name="dueTime">定时任务首次执行的延迟时间</param>
        /// <param name="period">定时任务后续执行的间隔时间</param>
        public void Start(Action callback, TimeSpan dueTime, TimeSpan period)
        {
            Start(o => callback?.Invoke(), null, dueTime, period);
        }

        /// <summary>
        /// 启动任务，设置任务首次执行延迟时间和执行周期
        /// </summary>
        /// <param name="dueTime">首次执行延迟时间</param>
        /// <param name="period">执行周期</param>
        /// <exception cref="ArgumentOutOfRangeException">当dueTime或period小于TimeSpan.Zero时抛出</exception>
        public void Start(Action<object?> callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            if (dueTime < TimeSpan.Zero || period < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException();

            userCallback = callback ?? throw new ArgumentNullException(nameof(callback));
            _state = state;

            ExecuteIfOperable(() =>
            {
                _period = period;
                _remainingDueTime = dueTime;
                ScheduleNextExecution(dueTime);
            });
        }

        /// <summary>
        /// 安排下一次执行
        /// </summary>
        /// <param name="delay">延迟时间</param>
        private void ScheduleNextExecution(TimeSpan delay)
        {
            _executionTimer.Change(delay, _period == TimeSpan.Zero ?
                Timeout.InfiniteTimeSpan : _period);
        }

        #endregion

        #region 核心操作实现

        /// <summary>
        /// 暂停任务
        /// </summary>
        protected override void CorePause()
        {
            _executionTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _lastPauseTime = DateTime.UtcNow;
        }

        /// <summary>
        /// 恢复任务
        /// </summary>
        protected override void CoreResume()
        {
            var pausedDuration = DateTime.UtcNow - _lastPauseTime;
            var newDueTime = _remainingDueTime - pausedDuration;

            if (newDueTime < TimeSpan.Zero)
                newDueTime = TimeSpan.Zero;

            ScheduleNextExecution(newDueTime);
        }

        /// <summary>
        /// 停止任务
        /// </summary>
        protected override void CoreStop()
        {
            _executionTimer.Dispose();
            _state = null;
        }

        #endregion

        #region 回调执行

        /// <summary>
        /// 执行回调
        /// </summary>
        /// <param name="state">回调状态</param>
        private void ExecuteCallback(object? state)
        {
            try
            {
                userCallback?.Invoke(_state);
            }
            finally
            {
                if (_period != TimeSpan.Zero)
                {
                    _remainingDueTime = _period;
                }
            }
        }

        #endregion

        /// <summary>
        /// 释放资源
        /// </summary>
        public new void Dispose()
        {
            base.Dispose();
            _executionTimer.Dispose();
        }
    }
}