using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.Error;
using ExtenderApp.Common.Networks.LinkOperates;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Common.ObjectPools.Policy;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 抽象基类 Linker，继承自 ConcurrentOperate 类，实现了 ILinker 接口。
    /// </summary>
    /// <typeparam name="TPolicy">操作策略类型，需要继承自 LinkOperatePolicy 并约束 TData 类型。</typeparam>
    /// <typeparam name="TData">数据类型，需要继承自 LinkerData。</typeparam>
    public abstract class Linker : ConcurrentOperate<LinkOperateData>, ILinker
    {
        /// <summary>
        /// 对象池，用于创建和重用 LinkerOperation 对象。
        /// </summary>
        private static ObjectPool<LinkerOperation> _operationPool =
                ObjectPool.Create(new SelfResetPooledObjectPolicy<LinkerOperation>());

        /// <summary>
        /// 接收回调。
        /// </summary>
        private readonly AsyncCallback _receiveCallbcak;

        /// <summary>
        /// 连接回调。
        /// </summary>
        private readonly AsyncCallback _connectCallback;

        /// <summary>
        /// 发送流量时的回调函数，参数为发送的流量大小
        /// </summary>
        private readonly Action<int>? _onSendedTraffic;

        /// <summary>
        /// 接收队列字节。
        /// </summary>
        private readonly ConcurrentQueue<(byte[], int)> _receiveQueueBytes;

        #region Event

        /// <summary>
        /// 接收数据事件。
        /// </summary>
        public event Action<byte[], int>? OnReceive;

        /// <summary>
        /// 发送流量事件，当发送流量时触发。
        /// </summary>
        /// <remarks>
        /// 参数表示发送的流量大小（以字节为单位）。
        /// </remarks>
        public event Action<int>? OnSendingTraffic;

        /// <summary>
        /// 已发送流量事件，当流量发送完成时触发。
        /// </summary>
        /// <remarks>
        /// 参数表示已发送的流量大小（以字节为单位）。
        /// </remarks>
        public event Action<int>? OnSendedTraffic;

        /// <summary>
        /// 接收流量事件，当开始接收流量时触发。
        /// </summary>
        /// <remarks>
        /// 参数表示开始接收的流量大小（以字节为单位）。
        /// </remarks>
        public event Action<int>? OnReceiveingTraffic;

        /// <summary>
        /// 已接收流量事件，当流量接收处理完成时触发。
        /// </summary>
        /// <remarks>
        /// 参数表示已处理完成的流量大小（以字节为单位）。
        /// </remarks>
        public event Action<int>? OnReceivedTraffic;

        /// <summary>
        /// 连接事件。
        /// </summary>
        public event Action<ILinker>? OnConnect;

        /// <summary>
        /// 关闭事件。
        /// </summary>
        public event Action<ILinker>? OnClose;

        /// <summary>
        /// 当发生错误时触发的事件
        /// </summary>
        /// <remarks>
        /// 事件处理函数接受一个字符串参数，表示错误信息
        /// </remarks>
        public event Action<string>? OnErrored;

        #endregion

        #region 内部属性

        /// <summary>
        /// 缓存字节数组。
        /// </summary>
        private byte[] cacheBytes;

        /// <summary>
        /// 是否正在接收数据。
        /// </summary>
        private volatile int isReceiveing;

        /// <summary>
        /// 是否正在连接。
        /// </summary>
        private volatile int isConnecting;

        /// <summary>
        /// 是否正在关闭。
        /// </summary>
        private volatile int isClosing;

        /// <summary>
        /// 发送包的数量，使用volatile关键字确保多线程访问时变量的可见性和有序性
        /// </summary>
        private volatile int sendPackCount;

        #endregion

        /// <summary>
        /// 是否已连接。
        /// </summary>
        public bool Connected => Data.Socket.Connected;

        /// <summary>
        /// 获取数据包长度
        /// </summary>
        /// <returns>返回数据包长度</returns>
        protected abstract int PacketLength { get; }

        /// <summary>
        /// 初始化 Linker 类的新实例。
        /// </summary>
        /// <param name="binaryParser">二进制解析器。</param>
        /// <param name="sequencePool">序列号池。</param>
        public Linker() : this(null)
        {

        }

        public Linker(Socket? socket)
        {
            _receiveQueueBytes = new();

            _receiveCallbcak = new AsyncCallback(ReceiveCallbcak);
            _connectCallback = new AsyncCallback(ConnectCallbcak);
            _onSendedTraffic = PrivateOnSendedTraffic;


            cacheBytes = ArrayPool<byte>.Shared.Rent(PacketLength * 2);

            Start(CreateLinkOperateData(socket));
        }

        #region Connect

        /// <summary>
        /// 连接到指定的主机和端口。
        /// </summary>
        /// <param name="host">主机名或IP地址。</param>
        /// <param name="port">端口号。</param>
        /// <exception cref="Exception">当前连接已经关闭</exception>
        /// <exception cref="Exception">当前连接正在连接中</exception>
        /// <exception cref="Exception">当前连接正在关闭中</exception>
        public void Connect(string host, int port)
        {
            if (!CanOperate)
                throw new Exception("当前连接已经关闭");

            if (Interlocked.CompareExchange(ref isConnecting, 1, 0) == 1)
                throw new Exception("当前连接正在连接中");

            if (Interlocked.CompareExchange(ref isClosing, 0, 1) == 1)
                throw new Exception("当前连接正在关闭中");

            lock (Data)
            {
                Data.Socket.Connect(host, port);
                StartReceive();
                OnConnect?.Invoke(this);
            }
        }

        /// <summary>
        /// 连接到指定的IP地址和端口。
        /// </summary>
        /// <param name="address">IP地址。</param>
        /// <param name="port">端口号。</param>
        /// <exception cref="Exception">当前连接已经关闭</exception>
        /// <exception cref="Exception">当前连接正在连接中</exception>
        /// <exception cref="Exception">当前连接正在关闭中</exception>
        public void Connect(IPAddress address, int port)
        {
            if (!CanOperate)
                throw new Exception("当前连接已经关闭");

            if (Interlocked.CompareExchange(ref isConnecting, 1, 0) == 1)
                throw new Exception("当前连接正在连接中");

            if (Interlocked.CompareExchange(ref isClosing, 0, 1) == 1)
                throw new Exception("当前连接正在关闭中");

            lock (Data)
            {
                Data.Socket.Connect(address, port);
                StartReceive();
                OnConnect?.Invoke(this);
            }
        }

        /// <summary>
        /// 连接到指定的终结点。
        /// </summary>
        /// <param name="point">终结点对象。</param>
        /// <exception cref="Exception">当前连接已经关闭</exception>
        /// <exception cref="Exception">当前连接正在连接中</exception>
        /// <exception cref="Exception">当前连接正在关闭中</exception>
        public void Connect(EndPoint point)
        {
            if (!CanOperate)
                throw new Exception("当前连接已经关闭");

            if (Interlocked.CompareExchange(ref isConnecting, 1, 0) == 1)
                throw new Exception("当前连接正在连接中");

            if (Interlocked.CompareExchange(ref isClosing, 0, 1) == 1)
                throw new Exception("当前连接正在关闭中");

            lock (Data)
            {
                Data.Socket.Connect(point);
                StartReceive();
                OnConnect?.Invoke(this);
            }
        }

        /// <summary>
        /// 异步连接到指定的主机和端口。
        /// </summary>
        /// <param name="host">主机名或IP地址。</param>
        /// <param name="port">端口号。</param>
        /// <exception cref="Exception">当前连接已经关闭</exception>
        /// <exception cref="Exception">当前连接正在连接中</exception>
        /// <exception cref="Exception">当前连接正在关闭中</exception>
        public void ConnectAsync(string host, int port)
        {
            if (!CanOperate)
                throw new Exception("当前连接已经关闭");

            if (Interlocked.CompareExchange(ref isConnecting, 1, 0) == 1)
                throw new Exception("当前连接正在连接中");

            if (Interlocked.CompareExchange(ref isClosing, 0, 1) == 1)
                throw new Exception("当前连接正在关闭中");

            Interlocked.Increment(ref isConnecting);
            Data.Socket.BeginConnect(host, port, _connectCallback, null);
        }

        /// <summary>
        /// 异步连接到指定的IP地址和端口。
        /// </summary>
        /// <param name="address">IP地址。</param>
        /// <param name="port">端口号。</param>
        /// <exception cref="Exception">当前连接已经关闭</exception>
        /// <exception cref="Exception">当前连接正在连接中</exception>
        /// <exception cref="Exception">当前连接正在关闭中</exception>
        public void ConnectAsync(IPAddress address, int port)
        {
            if (!CanOperate)
                throw new Exception("当前连接已经关闭");

            if (Interlocked.CompareExchange(ref isConnecting, 1, 0) == 1)
                throw new Exception("当前连接正在连接中");

            if (Interlocked.CompareExchange(ref isClosing, 0, 1) == 1)
                throw new Exception("当前连接正在关闭中");

            Interlocked.Increment(ref isConnecting);
            Data.Socket.BeginConnect(address, port, _connectCallback, null);
        }

        /// <summary>
        /// 异步连接到指定的终结点。
        /// </summary>
        /// <param name="point">终结点对象。</param>
        /// <exception cref="Exception">当前连接已经关闭</exception>
        /// <exception cref="Exception">当前连接正在连接中</exception>
        /// <exception cref="Exception">当前连接正在关闭中</exception>
        public void ConnectAsync(EndPoint point)
        {
            if (!CanOperate)
                throw new Exception("当前连接已经关闭");

            if (Interlocked.CompareExchange(ref isConnecting, 1, 0) == 1)
                throw new Exception("当前连接正在连接中");

            if (Interlocked.CompareExchange(ref isClosing, 0, 1) == 1)
                throw new Exception("当前连接正在关闭中");

            Interlocked.Increment(ref isConnecting);
            Data.Socket.BeginConnect(point, _connectCallback, null);
        }

        /// <summary>
        /// 连接回调处理。
        /// </summary>
        /// <param name="ar">异步操作结果。</param>
        private void ConnectCallbcak(IAsyncResult ar)
        {
            StartReceive();
            Interlocked.Decrement(ref isConnecting);
            OnConnect?.Invoke(this);
        }

        #endregion

        #region Send

        public void Send(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            CheckState();

            PrivateSend(data, 0, data.Length);
        }

        public void Send(byte[] data, int start)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            CheckState();
            if (start < 0 || start > data.Length)
                throw new ArgumentOutOfRangeException(nameof(start), "start is out of range");
            PrivateSend(data, start, data.Length - start);
        }

        public void Send(byte[] data, int start, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (start < 0 || length < 0 || start + length > data.Length)
                throw new ArgumentOutOfRangeException("start or length is out of range");
            CheckState();

            PrivateSend(data, start, length);
        }

        public void SendAsync(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            CheckState();

            PrivateSendAsync(data, 0, data.Length);
        }

        public void SendAsync(byte[] data, int start, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (start < 0 || length < 0 || start + length > data.Length)
                throw new ArgumentOutOfRangeException("start or length is out of range");
            CheckState();

            PrivateSendAsync(data, start, length);
        }

        private void PrivateSend(byte[] data, int start, int length)
        {
            var operation = _operationPool.Get();
            operation.Set(data, start, length, _onSendedTraffic);
            OnSendingTraffic?.Invoke(length);
            Interlocked.Increment(ref sendPackCount);
            Execute(operation);
        }

        private void PrivateSendAsync(byte[] data, int start, int length)
        {
            var operation = _operationPool.Get();
            operation.Set(data, start, length, _onSendedTraffic);
            OnSendingTraffic?.Invoke(length);
            Interlocked.Increment(ref sendPackCount);
            ExecuteAsync(operation);
        }

        public void Send(Memory<byte> memory)
        {
            var operation = _operationPool.Get();
            operation.Set(memory, _onSendedTraffic);
            OnSendingTraffic?.Invoke(memory.Length);
            Interlocked.Increment(ref sendPackCount);
            Execute(operation);
        }

        public void SendAsync(Memory<byte> memory)
        {
            var operation = _operationPool.Get();
            operation.Set(memory, _onSendedTraffic);
            OnSendingTraffic?.Invoke(memory.Length);
            Interlocked.Increment(ref sendPackCount);
            ExecuteAsync(operation);
        }

        private void PrivateOnSendedTraffic(int length)
        {
            Interlocked.Decrement(ref sendPackCount);
            OnSendedTraffic?.Invoke(length);
        }

        #endregion

        #region Receive

        /// <summary>
        /// 开始接收数据
        /// </summary>
        private void StartReceive()
        {
            //byte[] newReceiveQueueBytes = ArrayPool<byte>.Shared.Rent(DEFALUT_RECEIVE_LENGTH);
            byte[] newReceiveQueueBytes = ArrayPool<byte>.Shared.Rent(PacketLength);
            // 继续接收数据
            Data.Socket.BeginReceive(newReceiveQueueBytes, 0, newReceiveQueueBytes.Length, SocketFlags.None, _receiveCallbcak, newReceiveQueueBytes);
        }

        /// <summary>
        /// 异步接收回调方法
        /// </summary>
        /// <param name="ar">异步操作结果</param>
        private void ReceiveCallbcak(IAsyncResult ar)
        {
            try
            {
                byte[] receiveBuffer = (byte[])ar.AsyncState!;
                int bytesRead = Data.Socket.EndReceive(ar);
                Interlocked.Decrement(ref isReceiveing);
                if (bytesRead == 0)
                {
                    ArrayPool<byte>.Shared.Return(receiveBuffer);
                    OnErrored?.Invoke("连接已经断开");
                    return;
                    //throw new Exception("连接已经断开");
                }

                //记录接收数据长度
                OnReceiveingTraffic?.Invoke(bytesRead);

                _receiveQueueBytes.Enqueue((receiveBuffer, bytesRead));

                if (Interlocked.CompareExchange(ref isReceiveing, 1, 0) == 0)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(_ => ExecuteReceiveQueueBytes(), null);
                }

                StartReceive();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 执行接收队列中的字节数据
        /// </summary>
        private void ExecuteReceiveQueueBytes()
        {
            //如果前后两个数据包的数据都不是我自定义的数据包，则直接调用回调函数
            while (true)
            {
                if (!_receiveQueueBytes.TryDequeue(out var item))
                {
                    break;
                }

                OnReceive?.Invoke(item.Item1, item.Item2);

                OnReceivedTraffic?.Invoke(item.Item2);

                ArrayPool<byte>.Shared.Return(item.Item1);
            }

            Interlocked.Decrement(ref isReceiveing);

            //如果接收队列中还有数据，则继续处理
            if (_receiveQueueBytes.Count > 0 && Interlocked.CompareExchange(ref isReceiveing, 1, 0) == 0)
            {
                ThreadPool.UnsafeQueueUserWorkItem(_ => ExecuteReceiveQueueBytes(), null);
                return;
            }
        }

        #endregion

        #region Close
        public void Close(bool requireFullTransmission = false, bool requireFullDataProcessing = false)
        {
            //if (IsExecuting)
            //{
            //    CanOperate = false;
            //    return;
            //}

            if (Interlocked.CompareExchange(ref isClosing, 1, 0) == 1)
                return;

            if (requireFullTransmission && sendPackCount > 0)
            {
                // 等待所有发送操作完成
                while (sendPackCount > 0)
                {
                    Task.Delay(10).Wait();
                }
            }
            if (requireFullDataProcessing && _receiveQueueBytes.Count > 0)
            {
                // 等待所有接收操作完成
                while (_receiveQueueBytes.Count > 0)
                {
                    Task.Delay(10).Wait();
                }
            }
            PrivateClose();
        }

        private void PrivateClose()
        {
            //Data.Socket.
            Data.Socket.Close();
            OnClose?.Invoke(this);
            //Data.Socket.Dispose();
            //Release();
        }

        #endregion

        private void CheckState()
        {
            if (!CanOperate)
                throw new Exception("当前连接已经关闭");
            if (Interlocked.CompareExchange(ref isClosing, 0, 1) == 1)
                throw new Exception("当前连接正在关闭中");
            if (!Connected)
                throw new Exception("当前连接还未连接");
        }

        public override bool TryReset()
        {
            //foreach (var buffer in _registerDicts.Values)
            //{
            //    buffer.Release();
            //}
            //_registerDicts.Clear();

            //if (_heartbeatLazy.IsValueCreated)
            //{
            //    Heartbeat.ChangeSendHearbeatInterval(0);
            //    Heartbeat.ChangeTimeoutThreshold(0, 0);
            //}

            isConnecting = 0;
            isReceiveing = 0;
            isClosing = 0;

            ArrayPool<byte>.Shared.Return(cacheBytes);

            _receiveQueueBytes.Clear();

            OnReceive = null;
            OnSendingTraffic = null;
            OnSendedTraffic = null;
            OnReceiveingTraffic = null;
            OnReceivedTraffic = null;
            OnErrored = null;
            OnConnect = null;
            OnClose = null;

            return base.TryReset();
        }

        /// <summary>
        /// 创建一个LinkOperateData对象
        /// </summary>
        /// <param name="socket">Socket对象</param>
        /// <returns>返回创建的LinkOperateData对象</returns>
        protected abstract LinkOperateData CreateLinkOperateData(Socket? socket);
    }
}
