using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.NetWorks;
using ExtenderApp.Data.File;

namespace ExtenderApp.Common
{
    ///// <summary>
    ///// TCP连接操作类，实现IDisposable接口，支持异步和同步TCP连接操作。
    ///// </summary>
    //public class TcpLinkOperate : LinkOperate<>
    //{
    //    private class BuffersRent
    //    {
    //        private readonly Action<byte[]> _callback;
    //        private readonly NetworkStream _stream;
    //        /// <summary>
    //        /// 字节数组池，用于缓存字节数组以提高性能。
    //        /// </summary>
    //        private readonly ArrayPool<byte> _bufferPool;
    //        /// <summary>
    //        /// 当前连接的TCP信息。
    //        /// </summary>
    //        public readonly TcpLinkInfo _currentInfo;

    //        /// <summary>
    //        /// 异步回调方法，用于处理接收到的数据。
    //        /// </summary>
    //        private AsyncCallback receiveCallback;
    //        public BuffersRent(Action<byte[]> action, NetworkStream stream, TcpLinkInfo currentInfo)
    //        {
    //            _callback = action;
    //            _stream = stream;
    //            _bufferPool = ArrayPool<byte>.Shared;
    //            _currentInfo = currentInfo;
    //            receiveCallback = new AsyncCallback(ReceiveCallback);
    //        }

    //        /// <summary>
    //        /// 接收数据的回调函数。
    //        /// </summary>
    //        /// <param name="ar">异步操作的结果。</param>
    //        private void ReceiveCallback(IAsyncResult ar)
    //        {
    //            int bytesRead = _stream!.EndRead(ar);
    //            if (bytesRead <= 0) return;

    //            byte[] buffer = _bufferPool.Rent(_currentInfo.ReceiveBufferSize);
    //            _stream.BeginRead(buffer, 0, buffer.Length, receiveCallback, buffer);
    //            //if (bytesRead > 0)
    //            //{
    //            //    callback?.Invoke(buffer);
    //            //}
    //        }
    //    }

    //    /// <summary>
    //    /// 当前连接的TCP信息。
    //    /// </summary>
    //    public TcpLinkInfo CurrentInfo { get; private set; }

    //    /// <summary>
    //    /// TCP客户端对象。
    //    /// </summary>
    //    private readonly TcpClient _client;

    //    /// <summary>
    //    /// 网络流对象，用于读写数据。
    //    /// </summary>
    //    private NetworkStream? stream;


    //    /// <summary>
    //    /// 构造函数，初始化TCP客户端对象和网络流对象。
    //    /// </summary>
    //    /// <param name="pool">TCP连接对象池。</param>
    //    internal TcpLinkOperate()
    //    {
    //        _client = new TcpClient();
    //    }

    //    #region Connect

    //    /// <summary>
    //    /// 同步连接TCP服务器。
    //    /// </summary>
    //    /// <param name="linkInfo">TCP连接信息。</param>
    //    public void Connect(TcpLinkInfo linkInfo)
    //    {
    //        if (_client.Connected)
    //            throw new Exception(string.Format("已连接的TCP链接不可以重复使用：{0}", _client.Client.RemoteEndPoint));

    //        SetupClient(linkInfo);
    //        _client.Connect(linkInfo.IP);
    //        CurrentInfo = linkInfo;
    //        stream = _client.GetStream();
    //    }

    //    /// <summary>
    //    /// 异步连接TCP服务器。
    //    /// </summary>
    //    /// <param name="linkInfo">TCP连接信息。</param>
    //    public async Task ConnectAsync(TcpLinkInfo linkInfo)
    //    {
    //        if (_client.Connected)
    //            throw new Exception(string.Format("已连接的TCP链接不可以重复使用：{0}", _client.Client.RemoteEndPoint));

    //        SetupClient(linkInfo);
    //        await _client.ConnectAsync(linkInfo.IP);
    //        CurrentInfo = linkInfo;
    //        stream = _client.GetStream();
    //    }

    //    #endregion

    //    #region Receive

    //    /// <summary>
    //    /// 开始接收数据，并指定回调函数处理接收到的数据。
    //    /// </summary>
    //    /// <param name="callback">处理接收到的数据的回调函数。</param>
    //    public void BeginReceiveData(Action<byte[]> callback)
    //    {
    //        if (stream == null)
    //            throw new InvalidOperationException("还未连接");

    //        //this.callback = callback;
    //        //stream.BeginRead(buffer, 0, buffer.Length, receiveCallback, buffer);
    //    }

    //    #endregion

    //    #region Send

    //    /// <summary>
    //    /// 同步发送数据。
    //    /// </summary>
    //    /// <param name="send">要发送的数据。</param>
    //    public void Send(byte[] send)
    //    {
    //        if (stream == null)
    //            throw new InvalidOperationException("还未连接");

    //        stream.Write(send);
    //    }

    //    /// <summary>
    //    /// 异步发送数据。
    //    /// </summary>
    //    /// <param name="send">要发送的数据。</param>
    //    /// <exception cref="InvalidOperationException">如果当前未连接，则抛出此异常。</exception>
    //    public void SendAsync(byte[] send)
    //    {
    //        if (stream == null)
    //            throw new InvalidOperationException("还未连接");

    //        stream.WriteAsync(send);
    //    }

    //    #endregion

    //    /// <summary>
    //    /// 设置TCP客户端的配置信息。
    //    /// </summary>
    //    /// <param name="linkInfo">TCP连接信息。</param>
    //    private void SetupClient(TcpLinkInfo linkInfo)
    //    {
    //        if (_client.ReceiveBufferSize < linkInfo.ReceiveBufferSize)
    //        {
    //            _client.ReceiveBufferSize = linkInfo.ReceiveBufferSize;

    //            //if (buffer is null)
    //            //    buffer = new byte[linkInfo.ReceiveBufferSize];
    //            //else
    //            //    buffer = buffer.Length < linkInfo.ReceiveBufferSize ? new byte[linkInfo.ReceiveBufferSize] : buffer;
    //        }

    //        if (_client.SendBufferSize < linkInfo.SendBufferSize)
    //            _client.SendBufferSize = linkInfo.SendBufferSize;

    //        _client.ReceiveTimeout = linkInfo.ReceiveTimeout;
    //        _client.SendTimeout = linkInfo.SendTimeout;
    //    }

    //    /// <summary>
    //    /// 关闭TCP连接，并将当前对象释放回对象池。
    //    /// </summary>
    //    public void Close()
    //    {
    //        _client.Close();
    //        CurrentInfo = TcpLinkInfo.Empty;
    //        stream = null;
    //    }

    //    /// <summary>
    //    /// 释放资源。
    //    /// </summary>
    //    public void Dispose()
    //    {
    //        CurrentInfo = TcpLinkInfo.Empty;
    //        _client.Dispose();
    //        stream?.Dispose();
    //        stream = null;
    //    }
    //}

    public class TcpLinker : Linker<TcpLinkerPolicy, TcpLinkerData>
    {
        private readonly static TcpLinkerPolicy tcpLinkPolicy = new TcpLinkerPolicy();

        public TcpLinker(IBinaryParser binaryParser, SequencePool<byte> sequencePool) : base( binaryParser, sequencePool)
        {
            Policy = tcpLinkPolicy;
        }
    }
}
