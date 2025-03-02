using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Error;
using ExtenderApp.Common.NetWorks.LinkOperates;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Common.ObjectPools.Policy;
using ExtenderApp.Data;
namespace ExtenderApp.Common.NetWorks
{
    public abstract class LinkOperate<TPolicy, TData> : ConcurrentOperate<TPolicy, Socket, TData>
        where TPolicy : LinkOperatePolicy<TData>
        where TData : LinkOperateData
    {
        private const int MaxSendLegth = 4096;
        private const int DefalutReceiveLegth = 32 * 1024;

        private static ObjectPool<LinkOperation> _pool =
            ObjectPool.Create(new SelfResetPooledObjectPolicy<LinkOperation>());

        private readonly IBinaryFormatter<int> _intFormatter;
        private readonly IBinaryParser _binaryParser;
        private readonly ConcurrentDictionary<int, DataBuffer<IBinaryFormatter, Delegate>> _registerDicts;
        private readonly MethodInfo _deserializeMethodInfo;
        private readonly AsyncCallback _receiveCallbcak;
        private readonly SHA256 _sha256;

        private byte[] receiveBuffer;
        public event Action<byte[]> OnReceive;

        public LinkOperate(TPolicy policy, IBinaryParser binaryParser, SHA256 sha256, Socket? socket)
        {
            _binaryParser = binaryParser;
            _registerDicts = new();
            _receiveCallbcak = new AsyncCallback(ReceiveCallbcak);
            _intFormatter = _binaryParser.GetFormatter<int>();
            _deserializeMethodInfo = typeof(IBinaryFormatter<>).GetMethod("Deserialize");
            _sha256 = sha256;

            Policy = policy;
            Start(Policy, operate: socket);
            receiveBuffer = ArrayPool<byte>.Shared.Rent(DefalutReceiveLegth);
            if (socket != null && socket.Connected)
            {
                Operate.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, _receiveCallbcak, null);
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
            }
        }

        #endregion

        public void Register<T>(Action<T> callback)
        {
            Type type = typeof(T);
            callback.ArgumentNull(type.Name);
            int code = GetTypeCode<T>();

            if (_registerDicts.ContainsKey(code))
            {
                throw new InvalidOperationException(string.Format("不可以重复注册：{0}", type.Name));
            }

            var buffer = new DataBuffer<IBinaryFormatter, Delegate>();
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

            _registerDicts.TryAdd(code, buffer);
        }

        public void Send<T>(T value)
        {
            var valueBytes = _binaryParser.SerializeForArrayPool(value, out int length);
            int typeCode = GetTypeCode<T>();

            NetworkPacket packet = new NetworkPacket(typeCode, valueBytes);
            var sendBytes = ArrayPool<byte>.Shared.Rent(_intFormatter.Length + length);
            _binaryParser.Serialize(typeCode, sendBytes);
            for (int i = _intFormatter.Length; i < length; i++)
            {
                sendBytes[i] = valueBytes[i - _intFormatter.Length];
            }

            var operation = _pool.Get();
            operation.Set(sendBytes, 0, length);
            ExecuteOperation(operation);
            operation.Release();
            ArrayPool<byte>.Shared.Return(valueBytes);
            ArrayPool<byte>.Shared.Return(sendBytes);
        }

        #region Receive

        private void ReceiveCallbcak(IAsyncResult ar)
        {
            try
            {
                int bytesRead = Operate.EndReceive(ar);
                if (bytesRead > 0)
                {
                    ExtenderBinaryReader reader = new ExtenderBinaryReader(receiveBuffer);
                    int code = _intFormatter.Deserialize(ref reader);

                    if (!_registerDicts.TryGetValue(code, out var buffer))
                    {
                        //没有找到已经注册的类型
                        OnReceive?.Invoke(receiveBuffer);
                    }
                    else
                    {
                        buffer.Process(new ArraySegment<byte>(receiveBuffer, _intFormatter.Length, bytesRead - _intFormatter.Length));
                    }

                    // 继续接收数据
                    Operate.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, _receiveCallbcak, null);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion

        public int GetTypeCode<T>()
        {
            Type type = typeof(T);
            string typename = type.FullName ?? type.Name;
            // 使用 SHA256 计算哈希值
            byte[] hashBytes = _sha256.ComputeHash(Encoding.UTF8.GetBytes(typename));
            // 将前四个字节转换为整数
            int typeCode = BitConverter.ToInt32(hashBytes, 0);
            return typeCode;
        }

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
    }
}
