﻿using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Error;
using ExtenderApp.Common.IO;
using ExtenderApp.Common.Networks.LinkOperates;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Common.ObjectPools.Policy;
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
        ///// <summary>
        ///// 默认发送长度，单位为字节
        ///// </summary>
        //private const int DEFALUT_SEND_LENGTH = 4 * 1024;

        ///// <summary>
        ///// 默认接收长度，单位为字节
        ///// </summary>
        //private const int DEFALUT_RECEIVE_LENGTH = 4 * 1024;

        /// <summary>
        /// 对象池，用于创建和重用 LinkerOperation 对象。
        /// </summary>
        private static ObjectPool<LinkerOperation> _pool =
                ObjectPool.Create(new SelfResetPooledObjectPolicy<LinkerOperation>());

        /// <summary>
        /// 序列号池，用于生成和管理序列号。
        /// </summary>
        private readonly SequencePool<byte> _sequencePool;

        /// <summary>
        /// 注册字典，用于存储和检索格式化器和委托。
        /// </summary>
        private readonly ConcurrentDictionary<int, DataBuffer<IBinaryFormatter, Delegate, ILinker>> _registerDicts;

        /// <summary>
        /// 数据包分段器
        /// </summary>
        private readonly PacketSegmenter _packetSegmenter;

        /// <summary>
        /// 二进制解析器
        /// </summary>
        protected readonly IBinaryParser _binaryParser;

        /// <summary>
        /// 发送头格式化器
        /// </summary>
        protected readonly IBinaryFormatter<SendHead> _sendHeadFormatter;

        /// <summary>
        /// 记录发送动作委托
        /// </summary>
        /// <remarks>
        /// 该委托用于记录发送动作，接受一个整数参数。
        /// </remarks>
        private readonly Action<int> _recordSendAction;

        /// <summary>
        /// 流量记录器。
        /// </summary>
        public FlowRecorder Recorder { get; }

        #region AsyncCallback

        /// <summary>
        /// 接收回调。
        /// </summary>
        private readonly AsyncCallback _receiveCallbcak;

        /// <summary>
        /// 连接回调。
        /// </summary>
        private readonly AsyncCallback _connectCallback;

        #endregion

        #region Lazy

        /// <summary>
        /// 心跳延迟初始化对象。
        /// </summary>
        private readonly Lazy<Heartbeat> _heartbeatLazy;

        /// <summary>
        /// 心跳对象。
        /// </summary>
        public Heartbeat Heartbeat => _heartbeatLazy.Value;

        /// <summary>
        /// 接收队列字节延迟初始化对象。
        /// </summary>
        private readonly Lazy<ConcurrentQueue<(byte[], int)>> _receiveQueueBytesLazy;

        /// <summary>
        /// 接收队列字节。
        /// </summary>
        private ConcurrentQueue<(byte[], int)> receiveQueueBytes => _receiveQueueBytesLazy.Value;

        #endregion

        #region Event

        /// <summary>
        /// 接收数据事件。
        /// </summary>
        public event Action<ReadOnlyMemory<byte>>? OnReceive;

        /// <summary>
        /// 接收 LinkerDto 事件。
        /// </summary>
        public event Action<ILinker, LinkerDto>? OnReceiveLinkerDto;

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
        /// 此事件包含一个委托，该委托有两个参数：第一个参数是整型，表示错误代码；第二个参数是字符串，表示错误信息。
        /// </remarks>
        public event Action<int, string> OnErrored;

        #endregion

        #region 内部属性

        /// <summary>
        /// LinkerDto 对象。
        /// </summary>
        private LinkerDto linkerDto;

        /// <summary>
        /// 剩余长度。
        /// </summary>
        private int remaingLength;

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

        #endregion

        /// <summary>
        /// 是否需要心跳。
        /// </summary>
        public bool NeedHeartBeat => linkerDto.NeedHeartbeat;

        /// <summary>
        /// 是否已连接。
        /// </summary>
        public bool Connected => Data.Socket.Connected;

        /// <summary>
        /// 获取数据长度
        /// </summary>
        /// <returns>返回数据长度</returns>
        protected abstract int DataLength { get; }

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
        public Linker(IBinaryParser binaryParser, SequencePool<byte> sequencePool)
        {
            _registerDicts = new();
            _binaryParser = binaryParser;
            _sendHeadFormatter = _binaryParser.GetFormatter<SendHead>();
            _sequencePool = sequencePool;
            //_packetSegmenter = new PacketSegmenter(this, DEFALUT_SEND_LENGTH - _sendHeadFormatter.Length - (int)_binaryParser.GetDefaulLength<PacketSegmentDto>(), RegisterTypeCallback);
            _packetSegmenter = new PacketSegmenter(this, () => DataLength, RegisterTypeCallback);

            _heartbeatLazy = new(() => new Heartbeat(this), true);
            _receiveQueueBytesLazy = new(() => new ConcurrentQueue<(byte[], int)>(), true);

            Recorder = new();
            _recordSendAction = Recorder.RecordSend;

            _receiveCallbcak = new AsyncCallback(ReceiveCallbcak);
            _connectCallback = new AsyncCallback(ConnectCallbcak);

            //cacheBytes = ArrayPool<byte>.Shared.Rent(DEFALUT_RECEIVE_LENGTH * 2);
            //cacheBytes = ArrayPool<byte>.Shared.Rent(DataLength * 2);
            cacheBytes = Array.Empty<byte>();
        }

        /// <summary>
        /// 受保护的重写方法，用于启动 Linker。
        /// </summary>
        protected override void ProtectedStart()
        {
            cacheBytes = ArrayPool<byte>.Shared.Rent(PacketLength * 2);

            Register<LinkerDto>(ReceiveLinkerDto);
            Register<ErrorDto>(ReceiveErrorDto);

            if (_heartbeatLazy.IsValueCreated)
            {
                Heartbeat.Start();
            }
            _packetSegmenter.Start();
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
                ConnectProcess();
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
                ConnectProcess();
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
                ConnectProcess();
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
            ConnectProcess();
            Interlocked.Decrement(ref isConnecting);
        }

        /// <summary>
        /// 连接处理过程。
        /// </summary>
        /// <remarks>
        /// 这是一个受保护的虚方法，可以在派生类中重写以提供自定义的连接处理逻辑。
        /// </remarks>
        protected virtual void ConnectProcess()
        {

        }

        #endregion

        #region Register

        /// <summary>
        /// 注册一个类型为T的回调函数
        /// </summary>
        /// <typeparam name="T">回调函数的参数类型</typeparam>
        /// <param name="callback">回调函数</param>
        /// <exception cref="Exception">当当前连接已经关闭时抛出异常</exception>
        /// <exception cref="Exception">当当前连接正在关闭中时抛出异常</exception>
        public void Register<T>(Action<T> callback)
        {
            Register<T>((v, l) => callback.Invoke(v));
        }

        /// <summary>
        /// 注册一个类型为T和ILinker的回调函数
        /// </summary>
        /// <typeparam name="T">回调函数的参数类型</typeparam>
        /// <param name="callback">回调函数</param>
        /// <exception cref="Exception">当当前连接已经关闭时抛出异常</exception>
        /// <exception cref="Exception">当当前连接正在关闭中时抛出异常</exception>
        public void Register<T>(Action<T, ILinker> callback)
        {
            if (!CanOperate)
                throw new Exception("当前连接已经关闭");
            if (Interlocked.CompareExchange(ref isClosing, 0, 1) == 1)
                throw new Exception("当前连接正在关闭中");

            Type type = typeof(T);
            callback.ArgumentNull(type.Name);

            string typeName = type.FullName ?? type.Name;
            int typeCode = Utility.GetSimpleConsistentHash(typeName);
            if (_registerDicts.TryGetValue(typeCode, out var buffer))
            {
                var action = buffer.Item2 as Action<T, ILinker>;
                action += callback;
                buffer.Item2 = action;
                return;

                //之前的代码
                // 只接受最后一个回调函数
                //buffer.Item2 = callback;
                //return;
                //throw new Exception(string.Format("不允许重复注册:{0}", typeName));
            }

            buffer = DataBuffer<IBinaryFormatter, Delegate, ILinker>.GetDataBuffer();
            buffer.Item1 = _binaryParser.GetFormatter<T>();
            buffer.Item2 = callback;
            buffer.Item3 = this;
            buffer.GetProcessAction<ReadOnlyMemory<byte>>((d, a) =>
            {
                var formatter = d.Item1 as IBinaryFormatter<T>;
                var reader = new ExtenderBinaryReader(a);
                T result = formatter.Deserialize(ref reader);
                var callback = d.Item2 as Action<T, ILinker>;
                callback?.Invoke(result, d.Item3);
            });

            _registerDicts.TryAdd(typeCode, buffer);
        }

        #endregion

        #region Send

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <typeparam name="T">发送的数据类型</typeparam>
        /// <param name="value">要发送的数据</param>
        /// <exception cref="Exception">如果当前连接已经关闭、正在关闭中或还未连接，则抛出异常</exception>
        public void Send<T>(T value)
        {
            if (!CanOperate)
                throw new Exception("当前连接已经关闭");
            if (Interlocked.CompareExchange(ref isClosing, 0, 1) == 1)
                throw new Exception("当前连接正在关闭中");
            if (!Connected)
                throw new Exception("当前连接还未连接");
            if (_packetSegmenter.IsSending)
                throw new Exception("无法同时发送大数据包");

            var valueBytes = _binaryParser.SerializeForArrayPool(value, out int valueLength);
            int typeCode = Utility.GetSimpleConsistentHash<T>();
            int headLength = _sendHeadFormatter.Length;

            //检测发送数据大于数据段的长度，需要分包发送
            if (valueLength + headLength > PacketLength)
            {
                _packetSegmenter.SendBigPacket(typeCode, valueBytes, valueLength);
                ArrayPool<byte>.Shared.Return(valueBytes);
                return;
            }

            GetSendBytes(valueBytes, valueLength, typeCode, out var sendBytes, out var totalLength);

            var operation = _pool.Get();
            operation.Set(sendBytes, 0, totalLength, _recordSendAction);
            ExecuteOperation(operation);
            ArrayPool<byte>.Shared.Return(sendBytes);
            operation.Release();
        }

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <typeparam name="T">发送的数据类型</typeparam>
        /// <param name="value">要发送的数据</param>
        /// <exception cref="Exception">如果当前连接已经关闭、正在关闭中或还未连接，则抛出异常</exception>
        public void SendAsync<T>(T value)
        {
            if (!CanOperate)
                throw new Exception("当前连接已经关闭");
            if (Interlocked.CompareExchange(ref isClosing, 0, 1) == 1)
                throw new Exception("当前连接正在关闭中");
            if (!Connected)
                throw new Exception("当前连接还未连接");
            if (_packetSegmenter.IsSending)
                throw new Exception("无法异步发送大数据包");

            var valueBytes = _binaryParser.SerializeForArrayPool(value, out int valueLength);
            int typeCode = Utility.GetSimpleConsistentHash<T>();
            int headLength = _sendHeadFormatter.Length;

            //检测发送数据大于数据段的长度，需要分包发送
            if (valueLength + headLength > PacketLength)
            {
                _packetSegmenter.SendBigPacketAsync(typeCode, valueBytes, valueLength);
                ArrayPool<byte>.Shared.Return(valueBytes);
                return;
            }

            GetSendBytes(valueBytes, valueLength, typeCode, out var sendBytes, out var totalLength);

            var operation = _pool.Get();
            operation.Set(sendBytes, 0, totalLength, _recordSendAction, b => ArrayPool<byte>.Shared.Return(b));
            QueueOperation(operation);
        }

        /// <summary>
        /// 获取发送字节数组
        /// </summary>
        /// <param name="valueBytes">输入值字节数组</param>
        /// <param name="valueLength">输入值字节数组长度</param>
        /// <param name="typeCode">类型码</param>
        /// <param name="sendBytes">输出发送字节数组</param>
        /// <param name="totalLength">总长度</param>
        private void GetSendBytes(byte[] valueBytes, int valueLength, int typeCode, out byte[] sendBytes, out int totalLength)
        {
            int startIndex = _sendHeadFormatter.Length;
            totalLength = startIndex + valueLength;

            sendBytes = ArrayPool<byte>.Shared.Rent(totalLength);

            SendHead sendHead = new SendHead(true, typeCode, valueLength);
            ExtenderBinaryWriter writer = new ExtenderBinaryWriter(_sequencePool, sendBytes);
            _sendHeadFormatter.Serialize(ref writer, sendHead);
            for (int i = 0; i < valueLength; i++)
            {
                sendBytes[i + startIndex] = valueBytes[i];
            }

            ArrayPool<byte>.Shared.Return(valueBytes);
        }

        /// <summary>
        /// 发送原始字节数据
        /// </summary>
        /// <param name="bytes">要发送的字节数据</param>
        public void SendSource(byte[] bytes)
        {
            SendSource(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 发送原始字节数据
        /// </summary>
        /// <param name="bytes">要发送的字节数据</param>
        /// <param name="offset">要发送的数据的起始位置</param>
        /// <param name="count">要发送的数据的长度</param>
        public void SendSource(byte[] bytes, int offset, int count)
        {
            var operation = _pool.Get();
            operation.Set(bytes, offset, count, _recordSendAction);
            ExecuteOperation(operation);
            operation.Release();
        }

        public void SendSourceAsync(byte[] bytes)
        {
            SendSourceAsync(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 发送原始字节数据
        /// </summary>
        /// <param name="bytes">要发送的字节数据</param>
        /// <param name="offset">要发送的数据的起始位置</param>
        /// <param name="count">要发送的数据的长度</param>
        public void SendSourceAsync(byte[] bytes, int offset, int count)
        {
            var operation = _pool.Get();
            operation.Set(bytes, offset, count, _recordSendAction, b => ArrayPool<byte>.Shared.Return(b));
            QueueOperation(operation);
        }

        #endregion

        #region Receive

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
                if (bytesRead == 0)
                {
                    //Operate.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, _receiveCallbcak, receiveBuffer);
                    //return;
                    ArrayPool<byte>.Shared.Return(receiveBuffer);
                    Release();
                    throw new Exception("连接已经断开");
                }

                //记录接收数据长度
                Recorder.RecordReceive(bytesRead);

                receiveQueueBytes.Enqueue((receiveBuffer, bytesRead));

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
        /// 执行接收队列中的字节数据
        /// </summary>
        private void ExecuteReceiveQueueBytes()
        {
            //如果前后两个数据包的数据都不是我自定义的数据包，则直接调用回调函数
            while (true)
            {
                if (!receiveQueueBytes.TryDequeue(out var item))
                {
                    break;
                }

                byte[] receiveBuffer = item.Item1;
                int receiveCount = item.Item2;
                int totalLength = receiveCount + remaingLength;

                //如果上一个数据包有剩余数据，则将本次接受到的数据拷贝到剩余数据后面
                if (remaingLength > 0)
                {
                    for (int i = 0; i < receiveCount; i++)
                    {
                        cacheBytes[remaingLength + i] = receiveBuffer[i];
                    }
                    receiveBuffer = cacheBytes;
                }

                int difference = totalLength;
                //检查是否有数据头部信息
                if (!Utility.FindHead(receiveBuffer, out var startIndex, receiveCount))
                {
                    OnReceive?.Invoke(new ReadOnlyMemory<byte>(receiveBuffer, 0, totalLength));
                    remaingLength = 0;
                    ArrayPool<byte>.Shared.Return(item.Item1);
                    continue;
                }
                else if (startIndex != 0)
                {
                    //如果不是从0开始，可能是直接传输的数据，在上个数据包没传完
                    OnReceive?.Invoke(new ReadOnlyMemory<byte>(receiveBuffer, 0, startIndex));

                    difference -= startIndex;

                    //将数据头开始往前移动
                    for (int i = 0; i < difference; i++)
                    {
                        receiveBuffer[i] = receiveBuffer[startIndex + i];
                    }
                }

                ExtenderBinaryReader reader = new ExtenderBinaryReader(new ReadOnlyMemory<byte>(receiveBuffer, 0, difference));
                var sendHead = _sendHeadFormatter.Deserialize(ref reader);
                remaingLength = PrivateReceive(sendHead, receiveBuffer, difference);

                ArrayPool<byte>.Shared.Return(item.Item1);
            }

            if (receiveQueueBytes.Count > 0)
            {
                ThreadPool.UnsafeQueueUserWorkItem(_ => ExecuteReceiveQueueBytes(), null);
                return;
            }
            Interlocked.Decrement(ref isReceiveing);
        }

        /// <summary>
        /// 私有接收方法
        /// </summary>
        /// <param name="sendHead">发送头信息</param>
        /// <param name="receiveBuffer">接收缓冲区</param>
        /// <param name="receiveCount">接收到的字节数</param>
        /// <returns>剩余数据长度</returns>
        private int PrivateReceive(SendHead sendHead, byte[] receiveBuffer, int receiveCount)
        {
            int sendHeadLength = _sendHeadFormatter.Length;
            int sendLength = sendHead.Length;
            int length = sendHeadLength + sendLength;
            int difference = receiveCount - sendHeadLength - sendLength;

            //如果小于头部长度，说明数据不完整
            if (receiveCount < length)
            {
                if (receiveBuffer == cacheBytes)
                    return receiveCount;

                for (int i = 0; i < receiveCount; i++)
                {
                    cacheBytes[i] = receiveBuffer[i];
                }
                return receiveCount;
            }

            //if (!_registerDicts.TryGetValue(sendHead.TypeCode, out var buffer))
            //{
            //    //没有找到已经注册的类型
            //    OnReceive?.Invoke(new ReadOnlyMemory<byte>(receiveBuffer, sendHeadLength, sendLength));
            //}
            //else
            //{
            //    buffer.Process(new ReadOnlyMemory<byte>(receiveBuffer, sendHeadLength, sendLength));
            //}
            RegisterTypeCallback(sendHead.TypeCode, receiveBuffer, sendHeadLength, sendLength);

            //检查是否有多余的数据,如果有，将多余的数据往缓存里移动
            if (difference <= 0)
            {
                return 0;
            }
            else if (difference <= sendHeadLength)
            {
                for (int i = 0; i < difference; i++)
                {
                    cacheBytes[i] = receiveBuffer[i + length];
                }
                return difference;
            }

            //将前面已使用的数据删除，将后面的数据往前移动
            for (int i = 0; i < difference; i++)
            {
                receiveBuffer[i] = receiveBuffer[length + i];
            }

            //检查是否有数据头部信息
            if (!Utility.FindHead(receiveBuffer, out var startIndex, difference))
            {
                return difference;
            }

            ExtenderBinaryReader reader = new ExtenderBinaryReader(new ReadOnlyMemory<byte>(receiveBuffer, 0, difference));
            sendHead = _sendHeadFormatter.Deserialize(ref reader);
            return PrivateReceive(sendHead, receiveBuffer, difference);
        }

        /// <summary>
        /// 注册类型回调方法
        /// </summary>
        /// <param name="typeCode">类型代码</param>
        /// <param name="receiveBuffer">接收缓冲区</param>
        /// <param name="startIndex">起始索引</param>
        /// <param name="length">长度</param>
        private void RegisterTypeCallback(int typeCode, byte[] receiveBuffer, int startIndex, int length)
        {
            if (!_registerDicts.TryGetValue(typeCode, out var buffer))
            {
                //没有找到已经注册的类型
                OnReceive?.Invoke(new ReadOnlyMemory<byte>(receiveBuffer, startIndex, length));
            }
            else
            {
                buffer.Process(new ReadOnlyMemory<byte>(receiveBuffer, startIndex, length));
            }
        }

        /// <summary>
        /// 接收LinkerDto对象
        /// </summary>
        /// <param name="dto">LinkerDto对象</param>
        private void ReceiveLinkerDto(LinkerDto dto)
        {
            //是否需要心跳机制
            if (dto.NeedHeartbeat)
            {
                Heartbeat.Start();
                Heartbeat.ChangeSendHearbeatInterval(0);
            }

            linkerDto = dto;
            OnReceiveLinkerDto?.Invoke(this, dto);
        }

        /// <summary>
        /// 接收错误数据传输对象
        /// </summary>
        /// <param name="dto">错误数据传输对象</param>
        private void ReceiveErrorDto(ErrorDto dto)
        {
            OnErrored?.Invoke(dto.StatrCode, dto.Message);
        }

        #endregion

        #region Set

        /// <summary>
        /// 设置连接配置
        /// </summary>
        /// <param name="dto">连接配置对象</param>
        /// <exception cref="Exception">如果未连接到目标主机或连接正在关闭，将抛出异常</exception>
        public void Set(LinkerDto dto)
        {
            if (!Connected)
                throw new Exception("还未连接到目标主机，请先连接");
            if (Interlocked.CompareExchange(ref isClosing, 0, 1) == 1)
                throw new Exception("当前连接正在关闭中");

            if (dto.NeedHeartbeat)
                Heartbeat.Start();

            linkerDto = dto;
            Send(dto);
        }

        /// <summary>
        /// 设置连接使用的Socket
        /// </summary>
        /// <param name="socket">Socket对象</param>
        /// <exception cref="Exception">如果当前连接已经有Socket或Socket未连接，将抛出异常</exception>
        public void Set(Socket socket)
        {
            socket.ArgumentNull(nameof(socket));

            if (Data != null)
                throw new Exception("当前连接已经有socket，无法重新添加");

            //if (!socket.Connected)
            //    throw new Exception("当前socket还未连接，无法加入");

            //Start(Policy, operate: socket);
            //Data = socket;

            StartReceive();
        }

        #endregion

        #region Close

        /// <summary>
        /// 关闭当前对象
        /// </summary>
        public void Close()
        {
            if (IsExecuting)
            {
                CanOperate = false;
                Data.IsClose = true;
                Data.CloseCallback = Release;
                return;
            }

            Release();
        }

        #endregion

        public override bool TryReset()
        {
            foreach (var buffer in _registerDicts.Values)
            {
                buffer.Release();
            }
            _registerDicts.Clear();

            if (_heartbeatLazy.IsValueCreated)
            {
                Heartbeat.ChangeSendHearbeatInterval(0);
                Heartbeat.ChangeTimeoutThreshold(0, 0);
            }

            isConnecting = 0;
            isReceiveing = 0;
            isClosing = 0;
            remaingLength = 0;

            ArrayPool<byte>.Shared.Return(cacheBytes);

            OnReceive = null;
            OnReceiveLinkerDto = null;
            OnConnect = null;
            OnClose = null;

            return base.TryReset();
        }

        protected abstract void Release();
    }
}
