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
        private const int HeadLength = 4;
        private const int MaxSendLength = 4096;
        private const int DefalutReceiveLegth = 32 * 1024;

        private static ObjectPool<LinkerOperation> _pool =
            ObjectPool.Create(new SelfResetPooledObjectPolicy<LinkerOperation>());

        private readonly IBinaryFormatter<int> _intFormatter;
        private readonly SequencePool<byte> _sequencePool;
        private readonly IBinaryParser _binaryParser;
        private readonly AsyncCallback _receiveCallbcak;
        private readonly ConcurrentDictionary<int, DataBuffer<IBinaryFormatter, Delegate>> _registerDicts;

        #region Lazy

        private readonly Lazy<Heartbeat> _heartbeatLazy;
        public Heartbeat Heartbeat => _heartbeatLazy.Value;

        #endregion

        #region Event

        public event Action<ArraySegment<byte>>? OnReceive;
        public event Action<Linker<TPolicy, TData>, LinkerDto>? OnReceiveLinkerDto;

        #endregion

        private byte[] receiveBuffer;
        private LinkerDto linkerDto;

        public bool NeedHeartBeat => linkerDto.NeedHeartbeat;
        public bool Connected => Operate.Connected;

        public Linker(IBinaryParser binaryParser, SequencePool<byte> sequencePool)
        {
            _heartbeatLazy = new(() => new Heartbeat(this), true);

            _registerDicts = new();
            _binaryParser = binaryParser;
            _receiveCallbcak = new AsyncCallback(ReceiveCallbcak);
            _intFormatter = _binaryParser.GetFormatter<int>();
            _sequencePool = sequencePool;

            receiveBuffer = ArrayPool<byte>.Shared.Rent(DefalutReceiveLegth);
            Register<LinkerDto>(ReceiveLinkerDto);
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
                //throw new InvalidOperationException(string.Format("不可以重复注册：{0}", type.Name));
                var action = buffer.Item2 as Action<T>;
                action += callback;
                return;
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
            Type type = typeof(T);
            string typeName = type.FullName ?? type.Name;

            int typeCode = Utility.GetSimpleConsistentHash(typeName);
            int stratIndex = _intFormatter.Length + HeadLength;
            int totalLength = stratIndex + length;
            var sendBytes = ArrayPool<byte>.Shared.Rent(totalLength);

            ExtenderBinaryWriter writer = new ExtenderBinaryWriter(_sequencePool, sendBytes);
            _intFormatter.Serialize(ref writer, typeCode);
            WriteSendHead(sendBytes);
            for (int i = stratIndex; i < totalLength; i++)
            {
                sendBytes[i] = valueBytes[i - stratIndex];
            }

            var operation = _pool.Get();
            operation.Set(sendBytes, 0, totalLength);
            ExecuteOperation(operation);
            ArrayPool<byte>.Shared.Return(valueBytes);
            ArrayPool<byte>.Shared.Return(sendBytes);
            operation.Release();
        }

        private void WriteSendHead(byte[] bytes)
        {
            for (int i = _intFormatter.Length - 1; i >= 0; i--)
            {
                bytes[i + HeadLength] = bytes[i];
            }

            //四位字节的数据头，标识是体系内的数据
            //buffer[0] = 11;
            //buffer[1] = 22;
            //buffer[2] = 33;
            //buffer[3] = 44;
            for (int i = 0; i < HeadLength; i++)
            {
                bytes[i] = (byte)((i + 1) * 11);
            }
        }

        #endregion

        #region Receive

        private void ReceiveCallbcak(IAsyncResult ar)
        {
            try
            {
                int bytesRead = Operate.EndReceive(ar);
                if (bytesRead <= 0) return;

                if (!CheckHead(receiveBuffer))
                {
                    OnReceive?.Invoke(new ArraySegment<byte>(receiveBuffer, 0, bytesRead));
                    return;
                }

                ExtenderBinaryReader reader = new ExtenderBinaryReader(new ArraySegment<byte>(receiveBuffer, HeadLength, bytesRead));
                int typeCode = _intFormatter.Deserialize(ref reader);

                if (!_registerDicts.TryGetValue(typeCode, out var buffer))
                {
                    //没有找到已经注册的类型
                    OnReceive?.Invoke(new ArraySegment<byte>(receiveBuffer, HeadLength, bytesRead));
                }
                else
                {
                    int startIndex = _intFormatter.Length + HeadLength;
                    buffer.Process(new ArraySegment<byte>(receiveBuffer, startIndex, bytesRead - startIndex));
                }

                // 继续接收数据
                Operate.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, _receiveCallbcak, null);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private bool CheckHead(byte[] bytes)
        {
            for (int i = 0; i < HeadLength; i++)
            {
                int code = bytes[i] - (i + 1) * 11;
                if (code != 0)
                    return false;
            }
            return true;
        }

        private void ReceiveLinkerDto(LinkerDto dto)
        {
            //是否需要心跳机制
            if (dto.NeedHeartbeat)
            {
                Heartbeat.ChangeSendHearbeatInterval(0);
                //Heartbeat.ChangeTimeoutThreshold();
                //Heartbeat.TimeoutActionEvent += t =>
                //{
                //    Close();
                //};
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

            Start(Policy, operate: socket);

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
