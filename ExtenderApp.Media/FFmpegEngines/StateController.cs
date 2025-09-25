
namespace ExtenderApp.Media.FFmpegEngines
{
    /// <summary>
    /// 泛型状态控制器，用于管理状态的变更与同步。
    /// 支持等待状态达到目标值，并可通过委托动态更新状态。
    /// 适用于多线程环境下的状态同步场景。
    /// </summary>
    /// <typeparam name="T">状态类型，需支持比较。</typeparam>
    public class StateController<T>
    {
        /// <summary>
        /// 状态变更信号，用于线程间同步。
        /// </summary>
        private readonly AutoResetEvent _cacheAvailableEvent;

        /// <summary>
        /// 当前状态。
        /// </summary>
        public T? State { get; private set; }

        /// <summary>
        /// 目标状态。
        /// </summary>
        public T? TargetState { get; private set; }

        /// <summary>
        /// 状态更新委托，可用于自定义状态变更逻辑。
        /// </summary>
        public Func<T>? UpdateStateFunc { get; private set; }

        /// <summary>
        /// 当前状态是否已达到目标状态。
        /// </summary>
        public bool IsInTargetState => Equals(State, TargetState);

        /// <summary>
        /// 默认构造函数，状态和目标状态均为默认值。
        /// </summary>
        public StateController() : this(default, default, default)
        {
        }

        /// <summary>
        /// 构造函数，指定初始状态和目标状态。
        /// </summary>
        /// <param name="initialState">初始状态。</param>
        /// <param name="targetState">目标状态。</param>
        public StateController(T initialState, T targetState) : this(initialState, targetState, null)
        {
        }

        public StateController(Func<T> updateFunc) : this(default, default, updateFunc)
        {
        }

        public StateController(T? targetState, Func<T> updateFunc) : this(default, targetState, updateFunc)
        {
        }

        /// <summary>
        /// 构造函数，指定初始状态、目标状态和状态更新委托。
        /// </summary>
        /// <param name="initialState">初始状态。</param>
        /// <param name="targetState">目标状态。</param>
        /// <param name="updateFunc">状态更新委托。</param>
        public StateController(T? initialState, T? targetState, Func<T>? updateFunc)
        {
            State = initialState;
            TargetState = targetState;
            UpdateStateFunc = updateFunc;
            _cacheAvailableEvent = new(false);
        }

        /// <summary>
        /// 等待状态达到目标状态，支持超时和取消。
        /// 若提供了状态更新委托，则在等待期间会尝试更新状态。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <param name="timeoutMs">每次等待的超时时间（毫秒）。</param>
        /// <returns>是否成功达到目标状态。</returns>
        public bool WaitForTargetState(CancellationToken token, int timeoutMs = 50)
        {
            if (UpdateStateFunc != null)
            {
                State = UpdateStateFunc.Invoke();
            }

            while (!IsInTargetState && !token.IsCancellationRequested)
            {
                if (!_cacheAvailableEvent.WaitOne(timeoutMs, !token.IsCancellationRequested))
                {
                    if (UpdateStateFunc != null)
                    {
                        State = UpdateStateFunc.Invoke();
                    }
                    continue;
                }
            }
            return !token.IsCancellationRequested;
        }

        /// <summary>
        /// 更新当前状态为指定的新状态。
        /// 若新状态已达到目标状态，则触发状态变更信号。
        /// </summary>
        /// <param name="newState">新状态。</param>
        public void UpdateTargetState(T newState)
        {
            State = newState;
            if (IsInTargetState)
            {
                _cacheAvailableEvent.Set();
            }
        }
    }
}
