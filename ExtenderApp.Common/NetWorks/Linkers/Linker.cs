using System.Buffers;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.Networks.LinkOperates;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

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
        protected static ObjectPool<LinkerOperation> _operationPool =
                ObjectPool.Create(new SelfResetPooledObjectPolicy<LinkerOperation>());

        /// <summary>
        /// 获取资源限制器对象。
        /// </summary>
        /// <value>
        /// 资源限制器对象。
        /// </value>
        protected readonly ResourceLimiter _resourceLimit;

        /// <summary>
        /// 接收回调。
        /// </summary>
        protected AsyncCallback OnReceiveCallbcak { get; }

        /// <summary>
        /// 连接回调。
        /// </summary>
        private readonly AsyncCallback _connectCallback;

        public EndPoint RemoteEndPoint => Data.Socket.RemoteEndPoint;

        #region Event

        /// <summary>
        /// 接收数据事件。
        /// </summary>
        public event Action<byte[], int>? OnReceive;
        protected Action<byte[], int>? OnReceiveCallback => OnReceive;

        /// <summary>
        /// 连接事件。
        /// </summary>
        public event Action<ILinker>? OnConnect;
        protected Action<ILinker>? OnConnectCallback => OnConnect;

        /// <summary>
        /// 关闭事件。
        /// </summary>
        public event Action<ILinker>? OnClose;
        protected Action<ILinker>? OnCloseCallback => OnClose;

        /// <summary>
        /// 当发生错误时触发的事件
        /// </summary>
        /// <remarks>
        /// 事件处理函数接受一个字符串参数，表示错误信息
        /// </remarks>
        public event Action<Exception>? OnErrored;
        protected Action<Exception>? OnErroredCallback => OnErrored;

        #endregion

        #region 内部属性

        ///// <summary>
        ///// 是否正在接收数据。
        ///// </summary>
        //private volatile int isReceiveing;

        /// <summary>
        /// 是否正在连接。
        /// </summary>
        private volatile int isConnecting;

        /// <summary>
        /// 是否正在关闭。
        /// </summary>
        private volatile int isClosing;

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

        public Linker(ResourceLimiter resourceLimit) : this(AddressFamily.InterNetwork, resourceLimit)
        {

        }

        public Linker(Socket socket, ResourceLimiter resourceLimit)
        {
            OnReceiveCallbcak = new AsyncCallback(ReceiveCallbcak);
            _connectCallback = new AsyncCallback(ConnectCallbcak);
            _resourceLimit = resourceLimit;

            Start(CreateLinkOperateData(socket));
        }

        public Linker(AddressFamily addressFamily, ResourceLimiter resourceLimit)
        {
            OnReceiveCallbcak = new AsyncCallback(ReceiveCallbcak);
            _connectCallback = new AsyncCallback(ConnectCallbcak);
            _resourceLimit = resourceLimit;

            Start(CreateLinkOperateData(addressFamily));
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
            CheckStateForConnected();

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
        public virtual void Connect(IPAddress address, int port)
        {
            CheckStateForConnected();

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
        public virtual void Connect(EndPoint point)
        {
            CheckStateForConnected();

            lock (Data)
            {
                Data.Socket.Connect(point);
                StartReceive();
                OnConnect?.Invoke(this);
            }
        }

        public virtual void Connect(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (uri.Scheme == Uri.UriSchemeHttps)
                throw new ArgumentException("URI方案暂时处理不了https", nameof(uri));
            CheckStateForConnected();

            int port = uri.Port;
            if (port == -1)
            {
                port = uri.Scheme == "https" ? 443 : 80;
            }
            string host = uri.Host;

            lock (Data)
            {
                Data.Socket.Connect(host, port);
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
            CheckStateForConnected();

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
            CheckStateForConnected();

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
        public virtual void ConnectAsync(EndPoint point)
        {
            CheckStateForConnected();

            Interlocked.Increment(ref isConnecting);
            Data.Socket.BeginConnect(point, _connectCallback, null);
        }

        public virtual void ConnectAsync(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (uri.Scheme != Uri.UriSchemeHttp)
                throw new ArgumentException("URI方案暂时只能处理http", nameof(uri));
            CheckStateForConnected();

            int port = uri.Port;
            if (port == -1)
            {
                port = uri.Scheme == "https" ? 443 : 80;
            }
            string host = uri.Host;

            Interlocked.Increment(ref isConnecting);
            Data.Socket.BeginConnect(host, port, _connectCallback, null);
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

        public virtual void Send(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            PrivateSend(data, 0, data.Length);
        }

        public virtual void Send(byte[] data, int start)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (start < 0 || start > data.Length)
                throw new ArgumentOutOfRangeException(nameof(start), "start is out of range");
            PrivateSend(data, start, data.Length - start);
        }

        public virtual void Send(byte[] data, int start, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (start < 0 || length < 0 || start + length > data.Length)
                throw new ArgumentOutOfRangeException("start or length is out of range");
            PrivateSend(data, start, length);
        }

        public virtual void Send(ExtenderBinaryWriter writer)
        {
            CheckState();

            _resourceLimit.WaitForSendPermissionAsync(writer.BytesCommitted);

            var operation = _operationPool.Get();
            operation.Set(writer, _resourceLimit.ReleaseMemoryAction);

            Execute(operation);
        }

        public virtual void SendAsync(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            PrivateSendAsync(data, 0, data.Length);
        }

        public virtual void SendAsync(byte[] data, int start, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (start < 0 || length < 0 || start + length > data.Length)
                throw new ArgumentOutOfRangeException("start or length is out of range");

            PrivateSendAsync(data, start, length);
        }

        public virtual void SendAsync(ExtenderBinaryWriter writer)
        {
            CheckState();

            _resourceLimit.WaitForSendPermissionAsync(writer.BytesCommitted);

            var operation = _operationPool.Get();
            operation.Set(writer, _resourceLimit.ReleaseMemoryAction);

            ExecuteAsync(operation);
        }

        private void PrivateSend(byte[] data, int start, int length)
        {
            CheckState();

            _resourceLimit.WaitForMemoryAsync(length);
            _resourceLimit.WaitForSendPermissionAsync(length);

            var operation = _operationPool.Get();
            operation.Set(data, start, length, _resourceLimit.ReleaseMemoryAction);

            Execute(operation);
        }

        private void PrivateSendAsync(byte[] data, int start, int length)
        {
            _resourceLimit.WaitForMemoryAsync(length);
            _resourceLimit.WaitForSendPermissionAsync(length);

            var operation = _operationPool.Get();
            operation.Set(data, start, length, _resourceLimit.ReleaseMemoryAction);

            ExecuteAsync(operation);
        }

        public virtual void Send(Memory<byte> memory)
        {
            _resourceLimit.WaitForMemoryAsync(memory.Length);
            _resourceLimit.WaitForSendPermissionAsync(memory.Length);

            var operation = _operationPool.Get();
            operation.Set(memory, _resourceLimit.ReleaseMemoryAction);

            Execute(operation);
        }

        public virtual void SendAsync(Memory<byte> memory)
        {
            _resourceLimit.WaitForMemoryAsync(memory.Length);
            _resourceLimit.WaitForSendPermissionAsync(memory.Length);

            var operation = _operationPool.Get();
            operation.Set(memory, _resourceLimit.ReleaseMemoryAction);
            ExecuteAsync(operation);
        }

        #endregion

        #region Receive

        /// <summary>
        /// 开始接收数据
        /// </summary>
        protected virtual void StartReceive()
        {
            //byte[] newReceiveQueueBytes = ArrayPool<byte>.Shared.Rent(DEFALUT_RECEIVE_LENGTH);
            byte[] newReceiveQueueBytes = ArrayPool<byte>.Shared.Rent(PacketLength);
            // 继续接收数据
            Data.Socket.BeginReceive(newReceiveQueueBytes, 0, newReceiveQueueBytes.Length, SocketFlags.None, ReceiveCallbcak, newReceiveQueueBytes);
        }

        protected virtual int SocketEndReceive(IAsyncResult ar)
        {
            return Data.Socket.EndReceive(ar);
        }

        /// <summary>
        /// 异步接收回调方法
        /// </summary>
        /// <param name="ar">异步操作结果</param>
        protected void ReceiveCallbcak(IAsyncResult ar)
        {
            try
            {
                int bytesRead = SocketEndReceive(ar);
                byte[] receiveBuffer = (byte[])ar.AsyncState!;
                if (bytesRead == 0)
                {
                    ArrayPool<byte>.Shared.Return(receiveBuffer);
                    OnErrored?.Invoke(new Exception("连接已经断开"));
                    return;
                    //throw new Exception("连接已经断开");
                }

                _resourceLimit.WaitForMemoryAsync(bytesRead);
                _resourceLimit.WaitForReceivePermissionAsync(bytesRead);


                if (OnReceive == null)
                {
                    ArrayPool<byte>.Shared.Return(receiveBuffer);
                }
                else
                {
                    OnReceive.Invoke(receiveBuffer, bytesRead);
                }

                _resourceLimit.ReleaseMemory(bytesRead);
                StartReceive();
            }
            catch (SocketException ex)
            {
                OnErroredCallback?.Invoke(ex);
            }
        }

        #endregion

        #region Close

        public void Close(bool requireFullTransmission = false)
        {
 
            if (Interlocked.CompareExchange(ref isClosing, 1, 0) == 1)
                return;

            if (requireFullTransmission && _concurrentQueue.Count > 0)
            {
                // 等待所有发送操作完成
                while (_concurrentQueue.Count > 0)
                {
                    Task.Delay(10);
                }
            }
            PrivateClose();
        }

        private void PrivateClose()
        {
            Data.Socket.Close();
            OnClose?.Invoke(this);
        }

        #endregion

        /// <summary>
        /// 检查连接状态。
        /// </summary>
        /// <exception cref="Exception">如果连接已经关闭，则抛出异常。</exception>
        /// <exception cref="Exception">如果连接正在关闭中，则抛出异常。</exception>
        /// <exception cref="Exception">如果连接还未建立，则抛出异常。</exception>
        private void CheckState()
        {
            if (!CanOperate)
                throw new Exception("当前连接已经关闭");
            if (Interlocked.CompareExchange(ref isClosing, 0, 1) == 1)
                throw new Exception("当前连接正在关闭中");
            if (!Connected && Data.ProtocolType != ProtocolType.Udp)
                throw new Exception("当前连接还未连接");
        }

        private void CheckStateForConnected()
        {
            if (!CanOperate)
                throw new Exception("当前连接已经关闭");
            if (Interlocked.CompareExchange(ref isClosing, 0, 1) == 1)
                throw new Exception("当前连接正在关闭中");
        }

        public override bool TryReset()
        {
            isConnecting = 0;
            isClosing = 0;

            OnReceive = null;
            OnErrored = null;
            OnConnect = null;
            OnClose = null;

            return base.TryReset();
        }

        protected override void Dispose(bool disposing)
        {
            Close();
            Data.Socket.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// 创建一个LinkOperateData对象
        /// </summary>
        /// <param name="socket">Socket对象</param>
        /// <returns>返回创建的LinkOperateData对象</returns>
        protected abstract LinkOperateData CreateLinkOperateData(Socket socket);

        /// <summary>
        /// 创建一个 LinkOperateData 对象
        /// </summary>
        /// <param name="addressFamily">地址族</param>
        /// <returns>创建的 LinkOperateData 对象</returns>
        protected abstract LinkOperateData CreateLinkOperateData(AddressFamily addressFamily);
    }
}
