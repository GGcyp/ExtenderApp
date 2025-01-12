using ExtenderApp.Data;

namespace ExtenderApp.Common.ScheduledTasks
{
    /// <summary>
    /// 计划任务类，用于管理定时器的启动和回调。
    /// </summary>
    public class ScheduledTask : ExtenderCancellationTokenSource
    {
        /// <summary>
        /// 定时器实例。
        /// </summary>
        private readonly Timer _timer;

        /// <summary>
        /// 获取或设置释放任务时执行的操作。
        /// </summary>
        public Action<ScheduledTask>? ReleaseAction { get; set; }

        /// <summary>
        /// 回调方法。
        /// </summary>
        private Action<object>? callback;

        /// <summary>
        /// 回调方法的状态参数。
        /// </summary>
        private object? state;

        /// <summary>
        /// 时间周期
        /// </summary>
        private TimeSpan period;

        /// <summary>
        /// 剩余时间
        /// </summary>
        private TimeSpan remainingTime;

        /// <summary>
        /// 暂停时间
        /// </summary>
        private DateTime pauseTime;

        /// <summary>
        /// 初始化TimerData实例，并创建一个新的定时器实例。
        /// </summary>
        public ScheduledTask()
        {
            _timer = new Timer(TimerCallback);
        }

        /// <summary>
        /// 启动定时器，使用指定的回调方法、状态参数、延迟时间和周期时间（以毫秒为单位）。
        /// </summary>
        /// <param name="callback">回调方法。</param>
        /// <param name="state">回调方法的状态参数。</param>
        /// <param name="dueTime">启动定时器前的延迟时间。</param>
        /// <param name="period">定时器周期时间。</param>
        public void Start(Action<object> callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            this.callback = callback;
            this.state = state;
            this.period = period;
            remainingTime = dueTime;

            _timer.Change(dueTime, period);

            IsStop = true;
            CanOperate = true;
        }

        protected override void ProtectedPause()
        {
            if (IsStop || IsPause)
                return;

            pauseTime = DateTime.Now;
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        protected override void ProtectedResume()
        {
            if (IsStop || !IsPause)
                return;

            var elapsedTime = remainingTime - (DateTime.Now - pauseTime);
            remainingTime = elapsedTime <= TimeSpan.Zero ? TimeSpan.Zero : elapsedTime;
            _timer.Change(remainingTime, period);
        }

        protected override void ProtectedStop()
        {
            if (IsStop) return;

            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            ReleaseAction?.Invoke(this);
        }

        /// <summary>
        /// 定时器回调方法，调用用户指定的回调方法并传递状态参数。
        /// </summary>
        /// <param name="obj">定时器对象。</param>
        /// <remarks>并且回收计时任务</remarks>
        private void TimerCallback(object? obj)
        {
            IsStop = false;
            CanOperate = false;
            callback?.Invoke(state);
            remainingTime = period;
            ReleaseAction?.Invoke(this);
        }

        public override void Dispose()
        {
            callback = null;
            state = null;
        }

    }
}