using ExtenderApp.Abstract;

namespace ExtenderApp.Common.NetWorks
{
    /// <summary>
    /// 表示心跳检测结果的结构体
    /// </summary>
    public struct HearbeatResult
    {
        /// <summary>
        /// 获取链接操作接口
        /// </summary>
        public ILinker Linker { get; }

        /// <summary>
        /// 获取最后一次心跳检测的时间
        /// </summary>
        public DateTime LastReceiveTime { get; }

        /// <summary>
        /// 获取心跳类型
        /// </summary>
        public HeartbeatType HeartbeatType { get; }

        /// <summary>
        /// 初始化 HearbeatResult 结构体
        /// </summary>
        /// <param name="linkOperate">链接操作接口</param>
        /// <param name="lastDateTime">最后一次心跳检测的时间</param>
        /// <param name="heartbeatType">心跳类型</param>
        internal HearbeatResult(ILinker linkOperate, DateTime lastReceiveTime, HeartbeatType heartbeatType)
        {
            Linker = linkOperate;
            LastReceiveTime = lastReceiveTime;
            HeartbeatType = heartbeatType;
        }
    }

    /// <summary>
    /// 心跳管理类，用于管理和维护网络连接的心跳机制。
    /// </summary>
    public class Heartbeat : DisposableObject
    {
        /// <summary>
        /// 心跳配置参数，发送心跳包的间隔时间，以毫秒为单位。
        /// </summary>
        private const int HeartbeatInterval = 30000; // 30秒发送间隔

        /// <summary>
        /// 心跳配置参数，超时的阈值时间，以毫秒为单位。
        /// </summary>
        private const int TimeoutThreshold = 90000;  // 90秒超时阈值

        /// <summary>
        /// 网络连接接口。
        /// </summary>
        private readonly ILinker _linker;

        #region Timer

        /// <summary>
        /// 定时任务，用于定时发送心跳包。
        /// </summary>
        private Timer _heartbeatTimer;

        /// <summary>
        /// 定时任务，用于检测超时。
        /// </summary>
        private Timer _timeoutTimer;

        #endregion

        #region Event

        /// <summary>
        /// 超时事件，当检测到超时时会触发该事件。
        /// </summary>
        public event Action<HearbeatResult>? TimeoutActionEvent;

        /// <summary>
        /// 发送心跳包事件，当发送心跳包时会触发该事件。
        /// </summary>
        public event Action<HearbeatResult>? SendHeartbeatActionEvent;

        /// <summary>
        /// 接收心跳包事件，当接收到心跳包时会触发该事件。
        /// </summary>
        public event Action<HearbeatResult>? ReceiveHeartbeatEvent;

        #endregion

        #region 内部数据

        /// <summary>
        /// 上次接收到心跳包的时间。
        /// </summary>
        private DateTime lastReceiveTime;

        /// <summary>
        /// 心跳间隔，单位为毫秒。
        /// </summary>
        private int heartbeatInterval;

        /// <summary>
        /// 超时阈值，单位为毫秒。
        /// </summary>
        private int timeoutThreshold;

        #endregion

        /// <summary>
        /// 初始化Heartbeat类的新实例。
        /// </summary>
        /// <param name="link">网络连接接口。</param>
        public Heartbeat(ILinker link)
        {
            _linker = link;
            lastReceiveTime = DateTime.UtcNow;

            // 初始化定时器
            //_heartbeatTimer = new Timer(SendHeartbeat, null, HeartbeatInterval, HeartbeatInterval);
            //_timeoutTimer = new Timer(CheckTimeout, null, TimeoutThreshold, 1000);
            _heartbeatTimer = new(SendHeartbeat, null, HeartbeatInterval, HeartbeatInterval);
            _timeoutTimer = new(CheckTimeout, null, TimeoutThreshold, 1000);
            timeoutThreshold = TimeoutThreshold;
            heartbeatInterval = HeartbeatInterval;

            _linker.Register<HeartbeatType>(ProcessReceivedData);
        }

        #region Send

        /// <summary>
        /// 发送心跳包。
        /// </summary>
        public void SendHeartbeat()
        {
            SendHeartbeat(null);
        }

        /// <summary>
        /// 发送心跳包。
        /// </summary>
        /// <param name="state">状态对象，此处未使用。</param>
        private void SendHeartbeat(object? state)
        {
            ThrowIfDisposed();
            if (!_linker.Connected || (DateTime.UtcNow - lastReceiveTime).TotalMilliseconds < heartbeatInterval)
                return;

            _linker.Send(HeartbeatType.Ping);
            SendHeartbeatActionEvent?.Invoke(new HearbeatResult(_linker, lastReceiveTime, HeartbeatType.Ping));
        }

        #endregion

        #region Received

        /// <summary>
        /// 处理接收到的心跳包数据。
        /// </summary>
        /// <param name="heartbeatType">接收到的心跳包类型。</param>
        private void ProcessReceivedData(HeartbeatType heartbeatType)
        {
            if (heartbeatType == HeartbeatType.Ping)
                _linker.Send(HeartbeatType.Pong);

            ReceiveHeartbeatEvent?.Invoke(new HearbeatResult(_linker, lastReceiveTime, heartbeatType));
            lastReceiveTime = DateTime.UtcNow;
        }

        #endregion

        #region Change

        /// <summary>
        /// 更改发送心跳包的间隔时间
        /// </summary>
        /// <param name="dueTime">新的发送心跳包的间隔时间（毫秒），默认为HeartbeatInterval</param>
        public void ChangeSendHearbeatInterval(int dueTime = HeartbeatInterval)
        {
            _heartbeatTimer.Change(dueTime, dueTime);
            heartbeatInterval = dueTime;
        }

        /// <summary>
        /// 更改超时阈值
        /// </summary>
        /// <param name="dueTime">新的超时阈值（毫秒），默认为TimeoutThreshold</param>
        /// <param name="period">定时器周期（毫秒），默认为1000毫秒</param>
        public void ChangeTimeoutThreshold(int dueTime = TimeoutThreshold, int period = 1000)
        {
            _timeoutTimer.Change(dueTime, period);
            timeoutThreshold = dueTime;
        }

        #endregion

        /// <summary>
        /// 超时检测。
        /// </summary>
        /// <param name="state">状态对象，此处未使用。</param>
        private void CheckTimeout(object? state)
        {
            if ((DateTime.UtcNow - lastReceiveTime).TotalMilliseconds > timeoutThreshold)
            {
                TimeoutActionEvent?.Invoke(new HearbeatResult(_linker, lastReceiveTime, HeartbeatType.Timeout));
            }
        }

        /// <summary>
        /// 释放Heartbeat类占用的资源。
        /// </summary>
        /// <param name="disposing">指示是否应释放托管资源。</param>
        protected override void Dispose(bool disposing)
        {
            _heartbeatTimer?.Dispose();
            _timeoutTimer?.Dispose();
            base.Dispose(disposing);
        }
    }
}
