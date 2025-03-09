using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Error;
using ExtenderApp.Common.NetWorks.LinkOperates;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Common.ObjectPools.Policy;
using ExtenderApp.Data;
using ExtenderApp.Data.File;

namespace ExtenderApp.Common.NetWorks
{
    public abstract class Linker<TPolicy, TData> : ConcurrentOperate<TPolicy, Socket, TData>, ILinker
        where TPolicy : LinkOperatePolicy<TData>
        where TData : LinkerData
    {
        private const int MaxSendLength = 4096;
        private const int DefalutReceiveLegth = 32 * 1024;

        private static ObjectPool<LinkerOperation> _pool =
            ObjectPool.Create(new SelfResetPooledObjectPolicy<LinkerOperation>());

        private readonly IBinaryFormatter<SendHead> _sendHeadFormatter;
        private readonly SequencePool<byte> _sequencePool;
        private readonly IBinaryParser _binaryParser;
        private readonly AsyncCallback _receiveCallbcak;
        private readonly ConcurrentDictionary<int, DataBuffer<IBinaryFormatter, Delegate>> _registerDicts;

        public FlowRecorder Recorder { get; }

        #region Lazy

        private readonly Lazy<Heartbeat> _heartbeatLazy;
        public Heartbeat Heartbeat => _heartbeatLazy.Value;

        #endregion

        #region Event

        public event Action<ArraySegment<byte>>? OnReceive;
        public event Action<ILinker, LinkerDto>? OnReceiveLinkerDto;
        public event Action<ILinker>? OnClose;

        #endregion

        private byte[] receiveBuffer;
        private LinkerDto linkerDto;
        private int remaingLength;

        public bool NeedHeartBeat => linkerDto.NeedHeartbeat;
        public bool Connected => Operate.Connected;

        public Linker(IBinaryParser binaryParser, SequencePool<byte> sequencePool)
        {
            _heartbeatLazy = new(() => new Heartbeat(this), true);

            _registerDicts = new();
            _binaryParser = binaryParser;
            _receiveCallbcak = new AsyncCallback(ReceiveCallbcak);
            _sendHeadFormatter = _binaryParser.GetFormatter<SendHead>();
            _sequencePool = sequencePool;
            Recorder = new();

            receiveBuffer = ArrayPool<byte>.Shared.Rent(DefalutReceiveLegth);
        }

        protected override void ProtectedStart()
        {
            Register<LinkerDto>(ReceiveLinkerDto);

            if (_heartbeatLazy.IsValueCreated)
            {
                Heartbeat.Start();
            }
        }

        #region Connect

        /// <summary>
        /// 连接到指定的主机和端口。
        /// </summary>
        /// <param name="host">主机名或IP地址。</param>
        /// <param name="port">端口号。</param>
        public void Connect(string host, int port)
        {
            lock (Operate)
            {
                Operate.Connect(host, port);
                Operate.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, _receiveCallbcak, null);
                ConnectProcess();
            }
        }

        /// <summary>
        /// 连接到指定的IP地址和端口。
        /// </summary>
        /// <param name="address">IP地址。</param>
        /// <param name="port">端口号。</param>
        public void Connect(IPAddress address, int port)
        {
            lock (Operate)
            {
                Operate.Connect(address, port);
                Operate.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, _receiveCallbcak, null);
                ConnectProcess();
            }
        }

        /// <summary>
        /// 连接到指定的终结点。
        /// </summary>
        /// <param name="point">终结点对象。</param>
        public void Connect(EndPoint point)
        {
            lock (Operate)
            {
                Operate.Connect(point);
                Operate.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, _receiveCallbcak, null);
                ConnectProcess();
            }
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

        public void Register<T>(Action<T> callback)
        {
            Type type = typeof(T);
            callback.ArgumentNull(type.Name);

            string typeName = type.FullName ?? type.Name;
            int typeCode = Utility.GetSimpleConsistentHash(typeName);
            if (_registerDicts.TryGetValue(typeCode, out var buffer))
            {
                //if (buffer.Item2 is Action<T> action)
                //{
                //    action += callback;
                //    buffer.Item2 = action; // 更新 buffer.Item2
                //}
                //else
                //{
                //    throw new InvalidOperationException($"注册的委托类型不匹配：{type.Name}");
                //}

                //只接受最后一个回调函数
                buffer.Item2 = callback;
                return;
                //throw new Exception(string.Format("不允许重复注册:{0}", typeName));
            }

            buffer = DataBuffer<IBinaryFormatter, Delegate>.GetDataBuffer();
            buffer.Item1 = _binaryParser.GetFormatter<T>();
            buffer.Item2 = callback;
            buffer.SetProcessAction<ArraySegment<byte>>((d, a) =>
            {
                var formatter = d.Item1 as IBinaryFormatter<T>;
                var reader = new ExtenderBinaryReader(a);
                T result = formatter.Deserialize(ref reader);
                var callback = d.Item2 as Action<T>;
                callback?.Invoke(result);
            });

            _registerDicts.TryAdd(typeCode, buffer);
        }

        #endregion

        #region Send

        public void Send<T>(T value)
        {
            var valueBytes = _binaryParser.SerializeForArrayPool(value, out int length);

            int startIndex = _sendHeadFormatter.Length;
            int totalLength = startIndex + length;

            Type type = typeof(T);
            string typeName = type.FullName ?? type.Name;
            int typeCode = Utility.GetSimpleConsistentHash(typeName);
            var sendBytes = ArrayPool<byte>.Shared.Rent(totalLength);

            SendHead sendHead = new SendHead(true, typeCode, length);
            ExtenderBinaryWriter writer = new ExtenderBinaryWriter(_sequencePool, sendBytes);
            _sendHeadFormatter.Serialize(ref writer, sendHead);
            for (int i = 0; i < length; i++)
            {
                sendBytes[i + startIndex] = valueBytes[i];
            }

            //检测发送数据大于4KB，需要分包发送

            var operation = _pool.Get();
            operation.Set(sendBytes, 0, totalLength, Recorder.RecordSend);
            ExecuteOperation(operation);
            ArrayPool<byte>.Shared.Return(valueBytes);
            ArrayPool<byte>.Shared.Return(sendBytes);
            operation.Release();
        }

        public void SendSource(byte[] bytes)
        {
            SendSource(bytes, 0, bytes.Length);
        }

        public void SendSource(byte[] bytes, int offset, int count)
        {
            var operation = _pool.Get();
            operation.Set(bytes, offset, count, Recorder.RecordSend);
            ExecuteOperation(operation);
            operation.Release();
        }

        #endregion

        #region Receive

        private void ReceiveCallbcak(IAsyncResult ar)
        {
            try
            {
                int bytesRead = Operate.EndReceive(ar);
                if (bytesRead <= 0)
                {
                    Operate.BeginReceive(receiveBuffer, remaingLength, receiveBuffer.Length, SocketFlags.None, _receiveCallbcak, null);
                    return;
                }

                //记录接收数据长度
                Recorder.RecordReceive(bytesRead);

                int receiveCount = remaingLength + bytesRead;
                int difference = receiveCount;
                if (receiveCount <= Utility.HEAD_LENGTH)
                {
                    remaingLength = receiveCount;
                    Operate.BeginReceive(receiveBuffer, remaingLength, receiveBuffer.Length - remaingLength, SocketFlags.None, _receiveCallbcak, null);
                    return;
                }

                //检查是否有数据头部信息
                if (!Utility.FindHead(receiveBuffer, out var startIndex, receiveCount))
                {
                    OnReceive?.Invoke(new ArraySegment<byte>(receiveBuffer, 0, remaingLength + bytesRead));
                    remaingLength = 0;
                    Operate.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, _receiveCallbcak, null);
                    return;
                }
                else if (startIndex != 0)
                {
                    //如果不是从0开始，可能是直接传输的数据
                    OnReceive?.Invoke(new ArraySegment<byte>(receiveBuffer, 0, startIndex));

                    difference -= startIndex;

                    //将数据头开始往前移动
                    for (int i = 0; i < difference; i++)
                    {
                        receiveBuffer[i] = receiveBuffer[startIndex + i];
                    }
                }

                ExtenderBinaryReader reader = new ExtenderBinaryReader(new ArraySegment<byte>(receiveBuffer, 0, difference));
                var sendHead = _sendHeadFormatter.Deserialize(ref reader);
                PrivateReceive(sendHead, difference);

                // 继续接收数据
                Operate.BeginReceive(receiveBuffer, remaingLength, receiveBuffer.Length - remaingLength, SocketFlags.None, _receiveCallbcak, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void PrivateReceive(SendHead sendHead, int receiveCount)
        {
            int sendHeadLength = _sendHeadFormatter.Length;
            int sendLength = sendHead.Length;

            //如果小于头部长度，说明数据不完整
            if (receiveCount < sendLength + sendHeadLength)
            {
                remaingLength = receiveCount;
                return;
            }

            if (!_registerDicts.TryGetValue(sendHead.TypeCode, out var buffer))
            {
                //没有找到已经注册的类型
                OnReceive?.Invoke(new ArraySegment<byte>(receiveBuffer, sendHeadLength, sendLength));
            }
            else
            {
                buffer.Process(new ArraySegment<byte>(receiveBuffer, sendHeadLength, sendLength));
            }

            //检查是否有多余的数据,如果有，将多余的数据往前移动
            int difference = receiveCount - sendHeadLength - sendLength;
            if (difference <= 0)
            {
                remaingLength = 0;
                return;
            }

            int length = sendHeadLength + sendLength;
            for (int i = 0; i < difference; i++)
            {
                receiveBuffer[i] = receiveBuffer[length + i];
            }

            if (difference <= sendHeadLength)
            {
                remaingLength = difference;
                return;
            }

            //检查是否有数据头部信息
            if (!Utility.FindHead(receiveBuffer, out var startIndex, difference))
            {
                remaingLength = difference;
                return;
            }

            ExtenderBinaryReader reader = new ExtenderBinaryReader(new ArraySegment<byte>(receiveBuffer, 0, difference));
            sendHead = _sendHeadFormatter.Deserialize(ref reader);
            PrivateReceive(sendHead, difference);
        }

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

        #endregion

        #region Set

        public void Set(LinkerDto dto)
        {
            if (!Connected)
                throw new Exception("还未连接到目标主机，请先连接");

            if (dto.NeedHeartbeat)
                Heartbeat.Start();

            linkerDto = dto;
            Send(dto);
        }

        public void Set(Socket socket)
        {
            socket.ArgumentNull(nameof(socket));

            if (Operate != null)
                throw new Exception("当前连接已经有socket，无法重新添加");

            //if (!socket.Connected)
            //    throw new Exception("当前socket还未连接，无法加入");

            //Start(Policy, operate: socket);
            Operate = socket;

            Operate.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, _receiveCallbcak, null);
        }

        #endregion

        #region Close

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

        ///// <summary>
        ///// 发送文件
        ///// </summary>
        ///// <param name="info">文件信息</param>
        //public void SendFile(LocalFileInfo info, int datalength)
        //{
        //    //ThrowNotConnected();

        //    //检查大小，小的文件直接传输，大的文件需要分块传输
        //    var splitterInfo = _splitterParser.Create(info, datalength, false);
        //    NetworkPacket<SplitterInfo> sendData = new NetworkPacket<SplitterInfo>(splitterInfo);
        //    var readBytes = _binaryParser.SerializeForArrayPool(sendData, out int length);
        //    LinkOperation sendOperation = _pool.Get();
        //    sendOperation.Set(readBytes, 0, length);
        //    ExecuteOperation(sendOperation);
        //    sendOperation.Release();

        //    var fileOperateInfo = info.CreateFileOperate(fileAccess: FileAccess.ReadWrite);
        //    var fileOperate = _binaryParser.GetOperate(fileOperateInfo);

        //    sendOperation = _pool.Get();
        //    if (readBytes.Length < datalength)
        //    {
        //        ArrayPool<byte>.Shared.Return(readBytes);
        //        readBytes = ArrayPool<byte>.Shared.Rent(datalength);
        //    }

        //    byte[] sendBytes = ArrayPool<byte>.Shared.Rent((int)(_binaryParser.GetDefaulLength<SplitterDto>() + datalength));
        //    for (uint i = 0; i < splitterInfo.ChunkCount; i++)
        //    {
        //        readBytes = _binaryParser.Read(fileOperateInfo, i * datalength, datalength, fileOperate, readBytes);
        //        _binaryParser.Serialize(new SplitterDto(i, readBytes), sendBytes, out int sendLength);
        //        int sendChunkCount = datalength / MaxSendLegth;

        //        for (int j = 0; j < sendChunkCount; j++)
        //        {
        //            sendOperation.Set(sendBytes, j * MaxSendLegth, MaxSendLegth);
        //            ExecuteOperation(sendOperation);
        //        }

        //        int alreadySendLength = sendChunkCount * MaxSendLegth;
        //        if (alreadySendLength < sendLength)
        //        {
        //            sendOperation.Set(sendBytes, alreadySendLength, sendLength - alreadySendLength);
        //            ExecuteOperation(sendOperation);
        //        }
        //    }

        //    ArrayPool<byte>.Shared.Return(readBytes);
        //    ArrayPool<byte>.Shared.Return(sendBytes);
        //    sendOperation.Release();
        //    fileOperate.Release();
        //}

        /// <summary>
        /// 抛出未连接异常
        /// </summary>
        private void ThrowNotConnected()
        {
            if (!Operate.Connected)
            {
                throw new Exception("还未连接主机");
            }
        }

        public override bool TryReset()
        {
            ArrayPool<byte>.Shared.Return(receiveBuffer);
            OnReceive = null;
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

            return base.TryReset();
        }
    }
}
