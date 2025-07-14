using System.Buffers;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Data;


namespace ExtenderApp.Common.Networks.UDP
{
    /// <summary>
    /// UDP连接类，继承自Linker类，并实现IUdpLinker接口。
    /// </summary>
    internal class UdpLinker : Linker, IUdpLinker
    {
        /// <summary>
        /// 获取数据包长度，默认为1KB。
        /// </summary>
        protected override int PacketLength { get; } = 1 * 1024; // 1KB

        /// <summary>
        /// 远程端点信息。
        /// </summary>
        private EndPoint? remoteEndPoint;

        public UdpLinker(ResourceLimiter resourceLimit) : base(resourceLimit)
        {
        }

        public UdpLinker(Socket socket, ResourceLimiter resourceLimit) : base(socket, resourceLimit)
        {
        }

        public UdpLinker(AddressFamily addressFamily, ResourceLimiter resourceLimit) : base(addressFamily, resourceLimit)
        {
        }

        protected override LinkOperateData CreateLinkOperateData(Socket socket)
        {
            if (socket.ProtocolType != ProtocolType.Udp)
                throw new ArgumentException("套字节设置错误");

            return new LinkOperateData(socket);
        }

        protected override LinkOperateData CreateLinkOperateData(AddressFamily addressFamily)
        {
            return new LinkOperateData(addressFamily, SocketType.Dgram, ProtocolType.Udp);
        }

        #region Connect

        public override void Connect(EndPoint point)
        {
            remoteEndPoint = point ?? throw new ArgumentNullException(nameof(point));
            StartReceive();
            OnConnectCallback?.Invoke(this);
        }

        public override void Connect(Uri uri)
        {
            if (uri.Scheme != "udp")
                throw new ArgumentException("此URI目标地址不是UDP协议", nameof(uri));
            if (uri.Host == null)
                throw new ArgumentException("URI目标地址不能为空", nameof(uri));
            var ips = Dns.GetHostAddresses(uri.Host);
            var ip = ips[0];
            remoteEndPoint = new IPEndPoint(ip, uri.Port);

            StartReceive();
            OnConnectCallback?.Invoke(this);
        }

        public override void ConnectAsync(EndPoint point)
        {
            remoteEndPoint = point ?? throw new ArgumentNullException(nameof(point));
            StartReceive();
            OnConnectCallback?.Invoke(this);
        }

        public override void ConnectAsync(Uri uri)
        {
            if (uri.Scheme != "udp")
                throw new ArgumentException("此URI目标地址不是UDP协议", nameof(uri));
            if (uri.Host == null)
                throw new ArgumentException("URI目标地址不能为空", nameof(uri));
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port);
            StartReceive();
            OnConnectCallback?.Invoke(this);
        }

        #endregion

        #region Send

        public override void Send(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            PrivateSend(data, 0, data.Length);
        }

        public override void Send(byte[] data, int start)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (start < 0 || start > data.Length)
                throw new ArgumentOutOfRangeException(nameof(start), "start is out of range");
            PrivateSend(data, start, data.Length - start);
        }

        public override void Send(byte[] data, int start, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (start < 0 || length < 0 || start + length > data.Length)
                throw new ArgumentOutOfRangeException("start or length is out of range");


            PrivateSend(data, start, length);
        }

        public override void Send(ExtenderBinaryWriter writer)
        {
            CheckStatus();
            var operation = _operationPool.Get();
            operation.Set(writer, OnSendedTrafficCallback, remoteEndPoint);

            Execute(operation);
        }

        public override void SendAsync(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            PrivateSendAsync(data, 0, data.Length);
        }

        public override void SendAsync(byte[] data, int start, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (start < 0 || length < 0 || start + length > data.Length)
                throw new ArgumentOutOfRangeException("start or length is out of range");

            PrivateSendAsync(data, start, length);
        }

        public override void SendAsync(ExtenderBinaryWriter writer)
        {
            CheckStatus();
            var operation = _operationPool.Get();
            operation.Set(writer, OnSendedTrafficCallback, remoteEndPoint);

            ExecuteAsync(operation);
        }

        private void PrivateSend(byte[] data, int start, int length)
        {
            CheckStatus();
            var operation = _operationPool.Get();
            operation.Set(data, start, length, OnSendedTrafficCallback, end: remoteEndPoint);

            Execute(operation);
        }

        private void PrivateSendAsync(byte[] data, int start, int length)
        {
            CheckStatus();
            var operation = _operationPool.Get();
            operation.Set(data, start, length, OnSendedTrafficCallback, end: remoteEndPoint);

            ExecuteAsync(operation);
        }

        public override void Send(Memory<byte> memory)
        {
            CheckStatus();
            var operation = _operationPool.Get();
            operation.Set(memory, OnSendedTrafficCallback, remoteEndPoint);

            Execute(operation);
        }

        public override void SendAsync(Memory<byte> memory)
        {
            CheckStatus();
            var operation = _operationPool.Get();
            operation.Set(memory, OnSendedTrafficCallback, remoteEndPoint);
            ExecuteAsync(operation);
        }

        #endregion

        #region SendTo

        public void SendTo(byte[] data, EndPoint endPoint)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            PrivateSendTo(data, 0, data.Length, endPoint);
        }

        public void SendTo(byte[] data, int start, EndPoint endPoint)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (start < 0 || start > data.Length)
                throw new ArgumentOutOfRangeException(nameof(start), "start is out of range");

            PrivateSendTo(data, start, data.Length - start, endPoint);
        }

        public void SendTo(byte[] data, int start, int length, EndPoint endPoint)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (start < 0 || length < 0 || start + length > data.Length)
                throw new ArgumentOutOfRangeException("start or length is out of range");


            PrivateSendTo(data, start, length, endPoint);
        }

        public void SendToWriter(ExtenderBinaryWriter writer, EndPoint endPoint)
        {
            remoteEndPoint = endPoint;
            CheckStatus();
            var operation = _operationPool.Get();
            operation.Set(writer, OnSendedTrafficCallback, remoteEndPoint);

            Execute(operation);
        }

        public void SendToAsync(byte[] data, EndPoint endPoint)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            PrivateSendToAsync(data, 0, data.Length, endPoint);
        }

        public void SendToAsync(byte[] data, int start, int length, EndPoint endPoint)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (start < 0 || length < 0 || start + length > data.Length)
                throw new ArgumentOutOfRangeException("start or length is out of range");

            PrivateSendToAsync(data, start, length, endPoint);
        }

        public void SendToAsyncWriter(ExtenderBinaryWriter writer, EndPoint endPoint)
        {
            remoteEndPoint = endPoint;
            CheckStatus();
            var operation = _operationPool.Get();
            operation.Set(writer, OnSendedTrafficCallback, remoteEndPoint);

            ExecuteAsync(operation);
        }

        private void PrivateSendTo(byte[] data, int start, int length, EndPoint endPoint)
        {
            remoteEndPoint = endPoint;
            CheckStatus();
            var operation = _operationPool.Get();
            operation.Set(data, start, length, OnSendedTrafficCallback, end: remoteEndPoint);

            Execute(operation);
        }

        private void PrivateSendToAsync(byte[] data, int start, int length, EndPoint endPoint)
        {
            remoteEndPoint = endPoint;
            CheckStatus();
            var operation = _operationPool.Get();
            operation.Set(data, start, length, OnSendedTrafficCallback, end: remoteEndPoint);

            ExecuteAsync(operation);
        }

        public void SendTo(Memory<byte> memory, EndPoint endPoint)
        {
            remoteEndPoint = endPoint;
            CheckStatus();
            var operation = _operationPool.Get();
            operation.Set(memory, OnSendedTrafficCallback, remoteEndPoint);
            Execute(operation);
        }

        #endregion

        #region Receive

        protected override void StartReceive()
        {
            if (!Data.Socket.IsBound)
            {
                Data.Socket.Bind(new IPEndPoint(IPAddress.Any, 0)); // 0 表示自动分配端口
            }

            byte[] newReceiveQueueBytes = ArrayPool<byte>.Shared.Rent(PacketLength);
            // 继续接收数据
            Data.Socket.BeginReceiveFrom(newReceiveQueueBytes, 0, newReceiveQueueBytes.Length, SocketFlags.None, ref remoteEndPoint, OnReceiveCallbcak, newReceiveQueueBytes);
        }

        protected override int SocketEndReceive(IAsyncResult ar)
        {
            return Data.Socket.EndReceiveFrom(ar, ref remoteEndPoint);
        }

        #endregion

        private void CheckStatus()
        {
            if (remoteEndPoint == null)
            {
                throw new InvalidOperationException("使用UPD链接的目标地址不能为空");
            }
        }

    }
}
