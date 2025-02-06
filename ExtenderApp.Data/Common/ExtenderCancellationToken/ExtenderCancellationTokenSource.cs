namespace ExtenderApp.Data
{
    /// <summary>
    /// 扩展的取消令牌源类
    /// </summary>
    public abstract class ExtenderCancellationTokenSource : IDisposable
    {
        private Timer? _timer;
        private Timer Timer
        {
            get
            {
                if (_timer == null)
                {
                    _timer = new Timer(TimerCallback);
                }
                return _timer;
            }
        }

        /// <summary>
        /// 已经暂停
        /// </summary>
        public bool IsPause { get; private set; }

        /// <summary>
        /// 已经停止
        /// </summary>
        public bool IsStop { get; private set; }

        /// <summary>
        /// 获取或设置是否可以操作。
        /// </summary>
        /// <value>
        /// 如果可以操作，则为 true；否则为 false。
        /// </value>
        public bool CanOperate { get; protected set; }

        /// <summary>
        /// 获取或设置操作行为。
        /// </summary>
        /// <value>
        /// 操作行为。如果为 null，则表示没有设置操作行为。
        /// </value>
        private Action? operateAction;

        /// <summary>
        /// 停止操作的回调方法。
        /// </summary>
        private Action? callback;

        /// <summary>
        /// 获取当前实例的扩展取消令牌。
        /// </summary>
        /// <returns>扩展取消令牌。</returns>
        public ExtenderCancellationToken Token => new ExtenderCancellationToken(this);

        /// <summary>
        /// ExtenderCancellationTokenSource 的构造函数。
        /// </summary>
        public ExtenderCancellationTokenSource()
        {
        }

        protected virtual void Reset()
        {
            IsPause = false;
            IsStop = false;
            CanOperate = true;
            operateAction = null;
            callback = null;
        }

        #region Pause

        /// <summary>
        /// 暂停操作，指定暂停时间为长整型毫秒数。
        /// </summary>
        /// <param name="millisecondsDelay">暂停时间（毫秒）。</param>
        public void Pause(long millisecondsDelay)
        {
            if (!CanPause()) return;

            Timer.Change(millisecondsDelay, 0);
            operateAction = ProtectedPause;
            IsPause = true;
        }

        /// <summary>
        /// 暂停操作，指定暂停时间为 TimeSpan。
        /// </summary>
        /// <param name="delay">暂停时间。</param>
        public void Pause(TimeSpan delay)
        {
            if (!CanPause()) return;

            Timer.Change(delay, TimeSpan.Zero);
            operateAction = ProtectedPause;
            IsPause = true;
        }

        /// <summary>
        /// 暂停操作的抽象方法。
        /// </summary>
        public void Pause()
        {
            if (!CanPause()) return;

            ProtectedPause();
            IsPause = true;
        }

        /// <summary>
        /// 子类继承的暂停方法。
        /// </summary>
        protected abstract void ProtectedPause();

        protected bool CanPause()
        {
            return CanOperate && !IsPause;
        }

        #endregion

        #region Resume

        /// <summary>
        /// 恢复操作，指定恢复时间为长整型毫秒数。
        /// </summary>
        /// <param name="millisecondsDelay">恢复时间（毫秒）。</param>
        public void Resume(long millisecondsDelay)
        {
            if (!CanResume()) return;

            Timer.Change(millisecondsDelay, 0);
            operateAction = ProtectedResume;
            IsPause = false;
        }

        /// <summary>
        /// 恢复操作，指定恢复时间为 TimeSpan。
        /// </summary>
        /// <param name="delay">恢复时间。</param>
        public void Resume(TimeSpan delay)
        {
            if (!CanResume()) return;

            Timer.Change(delay, TimeSpan.Zero);
            operateAction = ProtectedResume;
            IsPause = false;
        }

        /// <summary>
        /// 恢复操作的抽象方法。
        /// </summary>
        public void Resume()
        {
            if (!CanResume()) return;

            ProtectedResume();
            IsPause = false;
        }

        /// <summary>
        /// 子类继承的恢复方法。
        /// </summary>
        protected abstract void ProtectedResume();

        protected bool CanResume()
        {
            return CanOperate && IsPause && !IsStop;
        }

        #endregion

        #region Stop

        /// <summary>
        /// 停止操作，指定停止时间为长整型毫秒数。
        /// </summary>
        /// <param name="millisecondsDelay">停止时间（毫秒）。</param>
        public void Stop(long millisecondsDelay)
        {
            if (!CanStop()) return;

            Timer.Change(millisecondsDelay, 0);
            operateAction = ProtectedStop;
            IsStop = true;
        }

        /// <summary>
        /// 停止操作，指定停止时间为 TimeSpan。
        /// </summary>
        /// <param name="delay">停止时间。</param>
        public void Stop(TimeSpan delay)
        {
            if (!CanStop()) return;

            Timer.Change(delay, TimeSpan.Zero);
            operateAction = ProtectedStop;
            IsStop = true;
        }

        /// <summary>
        /// 停止操作的抽象方法。
        /// </summary>
        public void Stop()
        {
            if (!CanStop()) return;

            ProtectedStop();
            IsStop = true;
        }

        /// <summary>
        /// 子类继承的停止方法。
        /// </summary>
        protected abstract void ProtectedStop();

        protected bool CanStop()
        {
            return CanOperate && !IsStop;
        }

        #endregion

        #region Register

        /// <summary>
        /// 注册一个回调函数，以便在请求取消时执行。
        /// </summary>
        /// <param name="callback">回调函数</param>
        public void Register(Action callback)
        {
            this.callback += callback;
        }

        #endregion

        /// <summary>
        /// 定时器回调方法，调用用户指定的回调方法并传递状态参数。
        /// </summary>
        private void TimerCallback(object? obj)
        {
            operateAction?.Invoke();
            callback?.Invoke();
            operateAction = null;
        }

        public virtual void Dispose()
        {
            operateAction = null;
            callback = null;
        }
    }
}
