
namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 流量记录器类，用于记录每秒发送和接收的字节数。
    /// </summary>
    public class FlowRecorder
    {
        /// <summary>
        /// 定时器实例
        /// </summary>
        private readonly Timer _timer;

        /// <summary>
        /// 发送的字节数
        /// </summary>
        private long sendByteCount;

        /// <summary>
        /// 接收的字节数
        /// </summary>
        private long receiveByteCount;

        /// <summary>
        /// 每秒发送的字节数
        /// </summary>
        private long sendBytesPerSecond;

        /// <summary>
        /// 每秒接收的字节数
        /// </summary>
        private long receiveBytesPerSecond;

        /// <summary>
        /// 发送的字节数
        /// </summary>
        public long SendByteCount => sendByteCount;

        /// <summary>
        /// 接收的字节数
        /// </summary>
        public long ReceiveByteCount => receiveByteCount;

        /// <summary>
        /// 每秒发送的字节数
        /// </summary>
        public long SendBytesPerSecond => sendBytesPerSecond;

        /// <summary>
        /// 每秒接收的字节数
        /// </summary>
        public long ReceiveBytesPerSecond => receiveBytesPerSecond;

        /// <summary>
        /// 定义一个事件，当<see cref="FlowRecorder"/>对象发生变化时触发。
        /// </summary>
        /// <remarks>
        /// 事件处理器将接收一个<see cref="FlowRecorder"/>类型的参数。
        /// </remarks>
        public event Action<FlowRecorder>? OnFlowRecorder;

        /// <summary>
        /// 初始化流量记录器，设置每秒触发一次定时器。
        /// </summary>
        public FlowRecorder()
        {
            _timer = new(OnTimerElapsed, null, 0, 1000); // 每秒触发一次
        }

        /// <summary>
        /// 记录发送的字节数。
        /// </summary>
        /// <param name="byteCount">发送的字节数。</param>
        public void RecordSend(int byteCount)
        {
            if (byteCount < 0)
                throw new ArgumentOutOfRangeException(nameof(byteCount), "字节数不能为负数。");

            Interlocked.Add(ref sendByteCount, byteCount);
            Interlocked.Add(ref sendBytesPerSecond, byteCount);
        }

        /// <summary>
        /// 记录接收的字节数。
        /// </summary>
        /// <param name="byteCount">接收的字节数。</param>
        public void RecordReceive(long byteCount)
        {
            if (byteCount < 0)
                throw new ArgumentOutOfRangeException(nameof(byteCount), "字节数不能为负数。");

            Interlocked.Add(ref receiveByteCount, byteCount);
            Interlocked.Add(ref receiveBytesPerSecond, byteCount);
        }

        /// <summary>
        /// 定时器触发时调用的方法，重置每秒发送和接收的字节数为0。
        /// </summary>
        /// <param name="sender">事件触发者。</param>
        private void OnTimerElapsed(object? sender)
        {
            OnFlowRecorder?.Invoke(this);
            Interlocked.Exchange(ref sendBytesPerSecond, 0);
            Interlocked.Exchange(ref receiveBytesPerSecond, 0);
        }

        public void Start()
        {
            _timer.Change(0, 1000);
        }

        public void Release()
        {
            _timer.Change(0, 0);
            Interlocked.Exchange(ref sendBytesPerSecond, 0);
            Interlocked.Exchange(ref sendByteCount, 0);
            Interlocked.Exchange(ref receiveBytesPerSecond, 0);
            Interlocked.Exchange(ref receiveByteCount, 0);
        }
    }
}
