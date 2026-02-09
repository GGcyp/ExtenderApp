using ExtenderApp.Contracts;

namespace ExtenderApp.Common.ScheduledTasks
{
    /// <summary>
    /// 操作控制器（可暂停/恢复/停止，带定时调度，线程安全）。
    /// </summary>
    public abstract class ExtenderOperationController : DisposableObject
    {
        #region 线程安全字段

        /// <summary>
        /// 同步锁对象，用于线程同步
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// 定时器对象，用于执行定时任务
        /// </summary>
        private Timer? _timer;

        #endregion 线程安全字段

        #region 状态属性（原子操作）

        /// <summary>
        /// 标识当前操作是否被暂停。
        /// </summary>
        private volatile bool _isPaused;

        /// <summary>
        /// 标识当前操作是否已停止。
        /// </summary>
        private volatile bool _isStopped;

        /// <summary>
        /// 标识当前操作是否可以进行。
        /// </summary>
        private volatile bool _canOperate = true;

        /// <summary>
        /// 获取当前操作是否被暂停。
        /// </summary>
        public bool IsPaused => _isPaused;

        /// <summary>
        /// 获取当前操作是否已停止。
        /// </summary>
        public bool IsStopped => _isStopped;

        /// <summary>
        /// 获取当前操作是否可以进行。
        /// </summary>
        public bool CanOperate => _canOperate;

        #endregion 状态属性（原子操作）

        #region 事件回调

        /// <summary>
        /// 定义一个私有事件，该事件触发时执行的委托。
        /// </summary>
        private event Action? Callback;

        /// <summary>
        /// 定义一个私有变量，用于存储计划执行的委托。
        /// </summary>
        private Action? _scheduledAction;

        #endregion 事件回调

        #region 定时器管理（线程安全）

        /// <summary>
        /// 获取一个安全的计时器对象。
        /// </summary>
        /// <returns>返回一个Timer对象，确保线程安全。</returns>
        private Timer SafeTimer
        {
            get
            {
                if (_timer == null)
                {
                    lock (_syncRoot)
                    {
                        _timer ??= new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite);
                    }
                }
                return _timer;
            }
        }

        #endregion 定时器管理（线程安全）

        #region 核心操作方法

        /// <summary>
        /// 在对象可操作的情况下执行给定的操作。
        /// </summary>
        /// <param name="action">要执行的操作。</param>
        /// <returns>如果对象可操作且操作成功执行，则返回true；否则返回false。</returns>
        protected bool ExecuteIfOperable(Action action)
        {
            ThrowIfDisposed();
            lock (_syncRoot)
            {
                if (!_canOperate) return false;

                action();
                return true;
            }
        }

        /// <summary>
        /// 在指定的延迟后计划执行给定的操作。
        /// </summary>
        /// <param name="delay">延迟时间。</param>
        /// <param name="operation">要执行的操作。</param>
        /// <exception cref="ArgumentOutOfRangeException">如果延迟时间小于TimeSpan.Zero，则抛出此异常。</exception>
        protected void ScheduleOperation(TimeSpan delay, Action operation)
        {
            if (delay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(delay));

            ExecuteIfOperable(() =>
            {
                _scheduledAction = operation;
                SafeTimer.Change(delay, Timeout.InfiniteTimeSpan);
            });
        }

        #endregion 核心操作方法

        #region 抽象方法

        /// <summary>
        /// 暂停核心操作。
        /// </summary>
        protected abstract void CorePause();

        /// <summary>
        /// 恢复核心操作。
        /// </summary>
        protected abstract void CoreResume();

        /// <summary>
        /// 停止核心操作。
        /// </summary>
        protected abstract void CoreStop();

        #endregion 抽象方法

        #region 公共方法（线程安全）

        /// <summary>
        /// 暂停当前操作。
        /// </summary>
        public void Pause() => ScheduleOperation(TimeSpan.Zero, () =>
        {
            ExecuteIfOperable(() =>
            {
                CorePause();
                _isPaused = true;
            });
        });

        /// <summary>
        /// 恢复当前操作。
        /// </summary>
        public void Resume() => ScheduleOperation(TimeSpan.Zero, () =>
        {
            ExecuteIfOperable(() =>
            {
                if (!_isPaused) return;

                CoreResume();
                _isPaused = false;
            });
        });

        /// <summary>
        /// 停止当前操作。
        /// </summary>
        public void Stop() => ScheduleOperation(TimeSpan.Zero, () =>
        {
            ExecuteIfOperable(() =>
            {
                CoreStop();
                _isStopped = true;
                _canOperate = false;
            });
        });

        /// <summary>
        /// 暂停操作，延迟指定的时间后执行。
        /// </summary>
        /// <param name="delay">延迟的时间间隔。</param>
        public void Pause(TimeSpan delay) => ScheduleOperation(delay, () =>
        {
            ExecuteIfOperable(() =>
            {
                CorePause();
                _isPaused = true;
            });
        });

        /// <summary>
        /// 恢复操作，延迟指定的时间后执行。
        /// </summary>
        /// <param name="delay">延迟的时间间隔。</param>
        public void Resume(TimeSpan delay) => ScheduleOperation(delay, () =>
        {
            ExecuteIfOperable(() =>
            {
                if (!_isPaused) return;

                CoreResume();
                _isPaused = false;
            });
        });

        /// <summary>
        /// 停止操作，延迟指定的时间后执行。
        /// </summary>
        /// <param name="delay">延迟的时间间隔。</param>
        public void Stop(TimeSpan delay) => ScheduleOperation(delay, () =>
        {
            ExecuteIfOperable(() =>
            {
                CoreStop();
                _isStopped = true;
                _canOperate = false;
            });
        });

        #endregion 公共方法（线程安全）

        #region 回调与资源释放

        /// <summary>
        /// 定时器回调函数，用于执行预定的操作并触发注册的回调。
        /// </summary>
        /// <param name="state">定时器状态对象。</param>
        private void TimerCallback(object? state)
        {
            Action? actionToExecute = null;
            lock (_syncRoot)
            {
                actionToExecute = _scheduledAction;
                _scheduledAction = null;
            }

            actionToExecute?.Invoke();
            Callback?.Invoke();
        }

        /// <summary>
        /// 注册回调函数。
        /// </summary>
        /// <param name="callback">要注册的回调方法。</param>
        /// <exception cref="ArgumentNullException">如果<paramref name="callback"/>为null，则抛出此异常。</exception>
        public void RegisterCallback(Action callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            lock (_syncRoot)
            {
                Callback += callback;
            }
        }

        protected override void DisposeManagedResources()
        {
            lock (_syncRoot)
            {
                _timer?.Dispose();
                Callback = null;
                _scheduledAction = null;
                _canOperate = false;
            }
        }

        #endregion 回调与资源释放
    }
}