
namespace ExtenderApp.Data
{
    /// <summary>
    /// 支持内部定时“按周期结算并输出”的线程安全值计数器。
    /// </summary>
    /// <remarks>
    /// - 使用内部 <see cref="Timer"/> 按 <see cref="Period"/> 周期结算当前累计值，并通过 <see cref="OnPeriod"/> 输出快照。<br/>
    /// - 即使没有调用 <see cref="Increment(long)"/>，在运行状态下也会按时结算。<br/>
    /// - 线程安全：所有公开方法/属性均可被并发调用；内部通过 <see cref="Interlocked"/> 保证原子性。<br/>
    /// - 事件线程：<see cref="OnPeriod"/> 在线程池计时器回调线程上触发，请避免在回调中做耗时/阻塞操作。<br/>
    /// - 生命周期：请在不再使用时调用 <see cref="Dispose"/> 或配合 using 释放，避免定时器泄漏。
    /// </remarks>
    public sealed class ValueCounter : IDisposable
    {
        /// <summary>
        /// 当前周期累计值（本周期内通过 <see cref="Increment(long)"/> 累计的总和）。
        /// </summary>
        /// <remarks>仅通过 <see cref="Interlocked.Add(ref long, long)"/> 与 <see cref="Interlocked.Exchange(ref long, long)"/> 进行并发访问；在结算时清零。</remarks>
        private long count;

        /// <summary>
        /// 当前周期调用次数（<see cref="Increment(long)"/> 被调用的次数）。
        /// </summary>
        /// <remarks>用于统计请求频次与计算周期内平均速率。</remarks>
        private long incrementCount;

        /// <summary>
        /// 上个周期累计值（上一次结算得到的周期值）。
        /// </summary>
        /// <remarks>为对外的 <see cref="LastPeriodValue"/> 提供数据来源，便于回放/对比。</remarks>
        private long lastPeriodValue;

        /// <summary>
        /// 上个周期调用次数（上一次结算时的调用次数）。
        /// </summary>
        /// <remarks>为对外的 <see cref="LastPeriodIncrements"/> 提供数据来源。</remarks>
        private long lastPeriodIncrementCount;

        /// <summary>
        /// 自上次 <see cref="Reset"/> 以来跨周期的累计总值。
        /// </summary>
        /// <remarks>只增不减；用于 <see cref="Total"/> 与整体速率统计。</remarks>
        private long total;

        /// <summary>
        /// 周期时长的后备字段（对应 <see cref="Period"/>）。
        /// </summary>
        /// <remarks>变更 <see cref="Period"/> 时会通过 <see cref="RestartTimer"/> 重启定时器。</remarks>
        private TimeSpan _period;

        /// <summary>
        /// 内部计时器，用于按固定周期触发结算。
        /// </summary>
        /// <remarks>为 <see langword="null"/> 表示未运行或已停止。</remarks>
        private Timer? _timer;

        /// <summary>
        /// 结算回调重入保护标志（0/1）。
        /// </summary>
        /// <remarks>通过 <see cref="Interlocked.Exchange(ref int, int)"/> 防止并发执行 <c>OnTimer/RollPeriod</c>。</remarks>
        private int _isTicking;

        /// <summary>
        /// 定时器创建/销毁与状态切换的互斥锁。
        /// </summary>
        /// <remarks>保护 <see cref="_timer"/> 与 <see cref="_running"/> 的一致性。</remarks>
        private readonly object _timerLock = new();

        /// <summary>
        /// 定时器是否处于运行状态的标志位。
        /// </summary>
        /// <remarks>使用 <see langword="volatile"/> 确保跨线程可见性。</remarks>
        private volatile bool _running;

        /// <summary>
        /// 总体统计的时间基准（UTC），用于计算总平均速率。
        /// </summary>
        /// <remarks>在首次 <see cref="Start"/> 或首次 <see cref="Increment(long)"/> 时初始化。</remarks>
        private DateTimeOffset _trackingStartUtc;

        /// <summary>
        /// 当前周期累计值。
        /// </summary>
        /// <remarks>通过原子读取，适用于并发场景。</remarks>
        public long Count => Interlocked.Read(ref count);

        /// <summary>
        /// 上个周期累计值。
        /// </summary>
        public long LastPeriodValue => Interlocked.Read(ref lastPeriodValue);

        /// <summary>
        /// 当前周期调用次数（<see cref="Increment(long)"/> 被调用的次数）。
        /// </summary>
        public long CurrentPeriodIncrements => Interlocked.Read(ref incrementCount);

        /// <summary>
        /// 上个周期调用次数。
        /// </summary>
        public long LastPeriodIncrements => Interlocked.Read(ref lastPeriodIncrementCount);

        /// <summary>
        /// 总累计值（自 <see cref="Reset"/> 以来，跨周期）。
        /// </summary>
        public long Total => Interlocked.Read(ref total);

        /// <summary>
        /// 最后一次 <see cref="Increment(long)"/> 的时间（UTC）。
        /// </summary>
        public DateTimeOffset LastIncrement { get; private set; }

        /// <summary>
        /// 当前周期起始时间（UTC）。
        /// </summary>
        public DateTimeOffset PeriodStartUtc { get; private set; }

        /// <summary>
        /// 上个周期起始时间（UTC）。
        /// </summary>
        public DateTimeOffset LastPeriodStartUtc { get; private set; }

        /// <summary>
        /// 上个周期结束时间（UTC）。
        /// </summary>
        public DateTimeOffset LastPeriodEndUtc { get; private set; }

        /// <summary>
        /// 当前周期已用时。
        /// </summary>
        public TimeSpan CurrentPeriodElapsed => PeriodStartUtc == default ? TimeSpan.Zero : (DateTimeOffset.UtcNow - PeriodStartUtc);

        /// <summary>
        /// 上个周期持续时长。
        /// </summary>
        public TimeSpan LastPeriodDuration => (LastPeriodStartUtc == default || LastPeriodEndUtc == default) ? TimeSpan.Zero : (LastPeriodEndUtc - LastPeriodStartUtc);

        /// <summary>
        /// 当前周期平均速率（单位/秒）。
        /// </summary>
        public double CurrentRatePerSecond
        {
            get
            {
                var elapsed = CurrentPeriodElapsed.TotalSeconds;
                return elapsed > 0 ? Count / elapsed : 0d;
            }
        }

        /// <summary>
        /// 上个周期平均速率（单位/秒）。
        /// </summary>
        public double LastPeriodRatePerSecond
        {
            get
            {
                var dur = LastPeriodDuration.TotalSeconds;
                var val = LastPeriodValue;
                return dur > 0 ? val / dur : 0d;
            }
        }

        /// <summary>
        /// 周期到达时回调（传递该周期的快照）。
        /// </summary>
        /// <remarks>
        /// - 在回调中收到的是“刚刚结算完成”的周期数据快照（通过对 <see cref="ValueCounter"/> 的隐式转换构造）。<br/>
        /// - 回调在线程池线程执行，建议快速返回；如需和 UI 交互，请自行封送到 UI 线程。
        /// </remarks>
        public event Action<Value>? OnPeriod;

        /// <summary>
        /// 统计周期。小于等于 0 表示不启用定时输出。修改该值会自动重启内部定时器。
        /// </summary>
        /// <remarks>
        /// 变更周期会重置 <see cref="PeriodStartUtc"/> 到当前时间并从新周期开始计数。
        /// </remarks>
        public TimeSpan Period
        {
            get => _period;
            set
            {
                _period = value;
                if (_running)
                {
                    RestartTimer();
                }
            }
        }

        /// <summary>
        /// 定时器是否在运行。
        /// </summary>
        public bool IsRunning => _running;

        /// <summary>
        /// 使用 1 秒为周期创建计数器，并在构造后自动启动（当周期大于 0 时）。
        /// </summary>
        public ValueCounter() : this(TimeSpan.FromSeconds(1))
        {
        }

        /// <summary>
        /// 使用指定周期创建计数器，并在构造后自动启动（当周期大于 0 时）。
        /// </summary>
        /// <param name="period">统计周期，≤0 表示不开启定时结算。</param>
        public ValueCounter(TimeSpan period)
        {
            count = 0;
            incrementCount = 0;
            lastPeriodValue = 0;
            lastPeriodIncrementCount = 0;
            total = 0;
            LastIncrement = DateTimeOffset.MinValue;
            PeriodStartUtc = default;
            LastPeriodStartUtc = default;
            LastPeriodEndUtc = default;
            _trackingStartUtc = default;
            _period = period;
            OnPeriod = null;

            if (_period > TimeSpan.Zero)
            {
                Start();
            }
        }

        /// <summary>
        /// 按照传入的值增加计数器的值（线程安全，非阻塞）。
        /// </summary>
        /// <param name="value">要增加的值。</param>
        /// <remarks>
        /// 本方法不依赖定时器，停止状态下同样可累计；仅不会自动结算输出。
        /// </remarks>
        public void Increment(long value)
        {
            var now = DateTimeOffset.UtcNow;

            // 原子性地增加计数器的值
            Interlocked.Add(ref count, value);
            Interlocked.Add(ref total, value);
            Interlocked.Increment(ref incrementCount);

            LastIncrement = now;

            if (_trackingStartUtc == default)
            {
                _trackingStartUtc = now;
            }
        }

        /// <summary>
        /// 手动立即结算并输出当前周期，然后进入下个周期。
        /// </summary>
        /// <remarks>
        /// - 当定时器处于停止或周期不合法（≤0）时，本方法不执行任何操作。<br/>
        /// - 回调在调用线程中触发（通过内部逻辑与定时回调同路径执行）。
        /// </remarks>
        public void FlushNow()
        {
            RollPeriod(DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// 启动内部定时器（幂等）。
        /// </summary>
        /// <remarks>若 <see cref="Period"/> ≤ 0 则不启动。</remarks>
        public void Start()
        {
            lock (_timerLock)
            {
                if (_running || _period <= TimeSpan.Zero) return;

                _running = true;
                PeriodStartUtc = DateTimeOffset.UtcNow;
                if (_trackingStartUtc == default)
                    _trackingStartUtc = PeriodStartUtc;

                _timer = new Timer(OnTimer, null, _period, _period);
            }
        }

        /// <summary>
        /// 停止内部定时器（幂等）。
        /// </summary>
        public void Stop()
        {
            lock (_timerLock)
            {
                _running = false;
                _timer?.Dispose();
                _timer = null;
            }
        }

        /// <summary>
        /// 重启内部定时器并以当前时间作为新的周期起点。
        /// </summary>
        private void RestartTimer()
        {
            lock (_timerLock)
            {
                _timer?.Dispose();
                _timer = null;

                if (_period <= TimeSpan.Zero)
                {
                    _running = false;
                    return;
                }

                _running = true;
                PeriodStartUtc = DateTimeOffset.UtcNow;
                if (_trackingStartUtc == default)
                    _trackingStartUtc = PeriodStartUtc;

                _timer = new Timer(OnTimer, null, _period, _period);
            }
        }

        private void OnTimer(object? obj)
        {
            OnTimer();
        }

        private void OnTimer()
        {
            // 防止回调重入
            if (Interlocked.Exchange(ref _isTicking, 1) == 1) return;
            try
            {
                RollPeriod(DateTimeOffset.UtcNow);
            }
            finally
            {
                Volatile.Write(ref _isTicking, 0);
            }
        }

        /// <summary>
        /// 结算一个周期，触发事件并切换到下个周期。
        /// </summary>
        /// <param name="now">当前时间（UTC）。</param>
        private void RollPeriod(DateTimeOffset now)
        {
            if (!_running || _period <= TimeSpan.Zero)
                return;

            var periodStart = PeriodStartUtc == default ? now - _period : PeriodStartUtc;
            var duration = now - periodStart;

            // 原子交换，拿到周期内的快照值，并清零进入下个周期
            var periodValue = Interlocked.Exchange(ref count, 0);
            var periodIncrements = Interlocked.Exchange(ref incrementCount, 0);

            // 更新“上个周期”属性
            Interlocked.Exchange(ref lastPeriodValue, periodValue);
            Interlocked.Exchange(ref lastPeriodIncrementCount, periodIncrements);
            LastPeriodStartUtc = periodStart;
            LastPeriodEndUtc = now;

            // 准备下个周期
            PeriodStartUtc = now;

            // 计算总速率（如有需要可在订阅侧自行计算，这里保留时间基准）
            var totalNow = Interlocked.Read(ref total);
            var totalSeconds = _trackingStartUtc == default ? 0 : (now - _trackingStartUtc).TotalSeconds;
            var totalRate = totalSeconds > 0 ? totalNow / totalSeconds : 0d;

            // 通过隐式转换为 Value 快照传递当前对象
            OnPeriod?.Invoke(this);
        }

        /// <summary>
        /// 重置所有统计（Count/Total/周期信息等）。
        /// </summary>
        /// <remarks>
        /// - 若定时器正在运行，重置后新的周期起点为当前时间。<br/>
        /// - 不会触发 <see cref="OnPeriod"/>。
        /// </remarks>
        public void Reset()
        {
            Interlocked.Exchange(ref count, 0);
            Interlocked.Exchange(ref incrementCount, 0);
            Interlocked.Exchange(ref lastPeriodValue, 0);
            Interlocked.Exchange(ref lastPeriodIncrementCount, 0);
            Interlocked.Exchange(ref total, 0);

            LastIncrement = DateTimeOffset.MinValue;

            var now = DateTimeOffset.UtcNow;
            PeriodStartUtc = now;
            LastPeriodStartUtc = default;
            LastPeriodEndUtc = default;
            _trackingStartUtc = now;
        }

        /// <summary>
        /// 释放内部定时器资源。等同于调用 <see cref="Stop"/>。
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// 表示一个周期结算时的快照值。
        /// </summary>
        public struct Value
        {
            /// <summary>
            /// 当前周期累计值。
            /// </summary>
            public long Count { get; init; }

            /// <summary>
            /// 上个周期累计值。
            /// </summary>
            public long LastPeriodValue { get; init; }

            /// <summary>
            /// 当前周期调用次数。
            /// </summary>
            public long CurrentPeriodIncrements { get; init; }

            /// <summary>
            /// 上个周期调用次数。
            /// </summary>
            public long LastPeriodIncrements { get; init; }

            /// <summary>
            /// 总累计值（自上次重置以来）。
            /// </summary>
            public long Total { get; init; }

            /// <summary>
            /// 最后一次递增时间（UTC）。
            /// </summary>
            public DateTimeOffset LastIncrement { get; init; }

            /// <summary>
            /// 当前周期起始时间（UTC）。
            /// </summary>
            public DateTimeOffset PeriodStartUtc { get; init; }

            /// <summary>
            /// 上个周期起始时间（UTC）。
            /// </summary>
            public DateTimeOffset LastPeriodStartUtc { get; init; }

            /// <summary>
            /// 上个周期结束时间（UTC）。
            /// </summary>
            public DateTimeOffset LastPeriodEndUtc { get; init; }

            /// <summary>
            /// 当前周期已用时。
            /// </summary>
            public TimeSpan CurrentPeriodElapsed { get; init; }

            /// <summary>
            /// 上个周期持续时长。
            /// </summary>
            public TimeSpan LastPeriodDuration { get; init; }

            /// <summary>
            /// 当前周期平均速率（单位/秒）。
            /// </summary>
            public double CurrentRatePerSecond { get; init; }

            /// <summary>
            /// 上个周期平均速率（单位/秒）。
            /// </summary>
            public double LastPeriodRatePerSecond { get; init; }

            /// <summary>
            /// 总平均速率（单位/秒）。
            /// </summary>
            public double TotalRatePerSecond { get; init; }

            /// <summary>
            /// 从计数器当前状态构建一个快照。
            /// </summary>
            /// <param name="counter">源计数器。</param>
            public Value(ValueCounter counter)
            {
                Count = counter.Count;
                LastPeriodValue = counter.LastPeriodValue;
                CurrentPeriodIncrements = counter.CurrentPeriodIncrements;
                LastPeriodIncrements = counter.LastPeriodIncrements;
                Total = counter.Total;
                LastIncrement = counter.LastIncrement;
                PeriodStartUtc = counter.PeriodStartUtc;
                LastPeriodStartUtc = counter.LastPeriodStartUtc;
                LastPeriodEndUtc = counter.LastPeriodEndUtc;
                CurrentPeriodElapsed = counter.CurrentPeriodElapsed;
                LastPeriodDuration = counter.LastPeriodDuration;
                CurrentRatePerSecond = counter.CurrentRatePerSecond;
                LastPeriodRatePerSecond = counter.LastPeriodRatePerSecond;

                var trackingStart = counter._trackingStartUtc;
                var now = DateTimeOffset.UtcNow;
                var totalSeconds = trackingStart == default ? 0 : (now - trackingStart).TotalSeconds;
                TotalRatePerSecond = totalSeconds > 0 ? counter.Total / totalSeconds : 0d;
            }

            /// <summary>
            /// 从 <see cref="ValueCounter"/> 隐式生成快照。
            /// </summary>
            public static implicit operator Value(ValueCounter counter) => new(counter);
        }
    }
}