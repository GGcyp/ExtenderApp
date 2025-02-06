using System.Net.Sockets;
using System.Text;
using ExtenderApp.Common.NetWorks;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common
{
    /// <summary>
    /// TCP连接操作类，实现IDisposable接口，支持异步和同步TCP连接操作。
    /// </summary>
    public class TcpLinkOperate : IDisposable
    {
        /// <summary>
        /// TCP客户端对象。
        /// </summary>
        private readonly TcpClient _client;

        /// <summary>
        /// 网络流对象，用于读写数据。
        /// </summary>
        private NetworkStream? stream;

        /// <summary>
        /// 回调函数，用于处理接收到的数据。
        /// </summary>
        private Action<byte[]>? callback;

        /// <summary>
        /// 数据缓冲区。
        /// </summary>
        private byte[] buffer;

        /// <summary>
        /// 异步回调方法，用于处理接收到的数据。
        /// </summary>
        private AsyncCallback receiveCallback;

        /// <summary>
        /// 当前连接的TCP信息。
        /// </summary>
        public TcpLinkInfo CurrentInfo { get; private set; }

        /// <summary>
        /// 构造函数，初始化TCP客户端对象和网络流对象。
        /// </summary>
        /// <param name="pool">TCP连接对象池。</param>
        internal TcpLinkOperate()
        {
            _client = new TcpClient();
            receiveCallback = new AsyncCallback(ReceiveCallback);
        }

        #region Connect

        /// <summary>
        /// 同步连接TCP服务器。
        /// </summary>
        /// <param name="linkInfo">TCP连接信息。</param>
        public void Connect(TcpLinkInfo linkInfo)
        {
            if (_client.Connected)
                throw new Exception(string.Format("已连接的TCP链接不可以重复使用：{0}", _client.Client.RemoteEndPoint));

            SetupClient(linkInfo);
            _client.Connect(linkInfo.IP);
            CurrentInfo = linkInfo;
            stream = _client.GetStream();
        }

        /// <summary>
        /// 异步连接TCP服务器。
        /// </summary>
        /// <param name="linkInfo">TCP连接信息。</param>
        public async Task ConnectAsync(TcpLinkInfo linkInfo)
        {
            if (_client.Connected)
                throw new Exception(string.Format("已连接的TCP链接不可以重复使用：{0}", _client.Client.RemoteEndPoint));

            SetupClient(linkInfo);
            await _client.ConnectAsync(linkInfo.IP);
            CurrentInfo = linkInfo;
            stream = _client.GetStream();
        }

        #endregion

        #region Receive

        /// <summary>
        /// 开始接收数据，并指定回调函数处理接收到的数据。
        /// </summary>
        /// <param name="callback">处理接收到的数据的回调函数。</param>
        public void BeginReceiveData(Action<byte[]> callback)
        {
            if (stream == null)
                throw new InvalidOperationException("还未连接");

            this.callback = callback;
            stream.BeginRead(buffer, 0, buffer.Length, receiveCallback, buffer);
        }

        /// <summary>
        /// 接收数据的回调函数。
        /// </summary>
        /// <param name="ar">异步操作的结果。</param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            int bytesRead = stream!.EndRead(ar);
            if (bytesRead > 0)
            {
                callback?.Invoke(buffer);
            }
            stream.BeginRead(buffer, 0, buffer.Length, receiveCallback, buffer);
        }

        #endregion

        /// <summary>
        /// 同步发送数据。
        /// </summary>
        /// <param name="send">要发送的数据。</param>
        public void Send(byte[] send)
        {
            if (stream == null)
                throw new InvalidOperationException("还未连接");

            stream.Write(send);
        }

        /// <summary>
        /// 异步发送数据。
        /// </summary>
        /// <param name="send">要发送的数据。</param>
        /// <exception cref="InvalidOperationException">如果当前未连接，则抛出此异常。</exception>
        public void SendAsync(byte[] send)
        {
            if (stream == null)
                throw new InvalidOperationException("还未连接");

            stream.WriteAsync(send);
        }

        /// <summary>
        /// 设置TCP客户端的配置信息。
        /// </summary>
        /// <param name="linkInfo">TCP连接信息。</param>
        private void SetupClient(TcpLinkInfo linkInfo)
        {
            if (_client.ReceiveBufferSize < linkInfo.ReceiveBufferSize)
            {
                _client.ReceiveBufferSize = linkInfo.ReceiveBufferSize;

                if (buffer is null)
                    buffer = new byte[linkInfo.ReceiveBufferSize];
                else
                    buffer = buffer.Length < linkInfo.ReceiveBufferSize ? new byte[linkInfo.ReceiveBufferSize] : buffer;
            }

            if (_client.SendBufferSize < linkInfo.SendBufferSize)
                _client.SendBufferSize = linkInfo.SendBufferSize;

            _client.ReceiveTimeout = linkInfo.ReceiveTimeout;
            _client.SendTimeout = linkInfo.SendTimeout;
        }

        /// <summary>
        /// 关闭TCP连接，并将当前对象释放回对象池。
        /// </summary>
        public void Close()
        {
            _client.Close();
            CurrentInfo = TcpLinkInfo.Empty;
            stream = null;
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            CurrentInfo = TcpLinkInfo.Empty;
            _client.Dispose();
            stream?.Dispose();
            stream = null;
        }
    }
}
