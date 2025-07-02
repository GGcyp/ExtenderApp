using System.Net;
using System.Net.Sockets;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义一个用于操作链接的接口
    /// </summary>
    public interface ILinker : IDisposable
    {
        /// <summary>
        /// 获取当前是否已连接。
        /// </summary>
        /// <value>
        /// 如果已连接，则返回 true；否则返回 false。
        /// </value>
        bool Connected { get; }

        /// <summary>
        /// 当连接关闭时触发的事件。
        /// </summary>
        event Action<ILinker>? OnClose;

        /// <summary>
        /// 当连接建立时触发的事件。
        /// </summary>
        event Action<ILinker>? OnConnect;

        /// <summary>
        /// 当发生错误时触发的事件。
        /// </summary>
        event Action<string> OnErrored;

        /// <summary>
        /// 当接收到数据时触发的事件
        /// </summary>
        event Action<byte[], int>? OnReceive;

        /// <summary>
        /// 已发送流量事件，当流量发送完成时触发。
        /// </summary>
        /// <remarks>
        /// 参数表示已发送的流量大小（以字节为单位）。
        /// </remarks>
        event Action<int>? OnSendedTraffic;

        /// <summary>
        /// 接收流量事件，当开始接收流量时触发。
        /// </summary>
        /// <remarks>
        /// 参数表示开始接收的流量大小（以字节为单位）。
        /// </remarks>
        event Action<int>? OnReceiveingTraffic;

        /// <summary>
        /// 已接收流量事件，当流量接收处理完成时触发。
        /// </summary>
        /// <remarks>
        /// 参数表示已处理完成的流量大小（以字节为单位）。
        /// </remarks>
        event Action<int>? OnReceivedTraffic;

        /// <summary>
        /// 通过主机名和端口号连接到服务器
        /// </summary>
        /// <param name="host">主机名</param>
        /// <param name="port">端口号</param>
        void Connect(string host, int port);

        /// <summary>
        /// 通过IP地址和端口号连接到服务器
        /// </summary>
        /// <param name="address">IP地址</param>
        /// <param name="port">端口号</param>
        void Connect(IPAddress address, int port);

        /// <summary>
        /// 连接到指定的URI。
        /// </summary>
        /// <param name="uri">要连接的URI。</param>
        void Connect(Uri uri);

        /// <summary>
        /// 异步连接到指定的URI。
        /// </summary>
        /// <param name="uri">要连接的URI。</param>
        void ConnectAsync(Uri uri);

        /// <summary>
        /// 异步连接到指定的主机和端口。
        /// </summary>
        /// <param name="host">主机名或IP地址。</param>
        /// <param name="port">端口号。</param>
        void ConnectAsync(string host, int port);

        /// <summary>
        /// 异步连接到指定的IP地址和端口。
        /// </summary>
        /// <param name="address">IP地址。</param>
        /// <param name="port">端口号。</param>
        void ConnectAsync(IPAddress address, int port);

        /// <summary>
        /// 异步连接到指定的终结点。
        /// </summary>
        /// <param name="point">终结点。</param>
        void ConnectAsync(EndPoint point);

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">要发送的数据</param>
        void Send(byte[] data);

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">要发送的数据</param>
        /// <param name="start">数据的起始位置</param>
        /// <param name="length">要发送的数据长度</param>
        void Send(byte[] data, int start, int length);

        /// <summary>
        /// 发送内存数据
        /// </summary>
        /// <param name="memory">要发送的内存数据</param>
        void Send(Memory<byte> memory);

        /// <summary>
        /// 同步发送数据。
        /// </summary>
        /// <param name="writer">用于写入数据的二进制写入器。</param>
        void SendWriter(ExtenderBinaryWriter writer);

        /// <summary>
        /// 异步发送数据。
        /// </summary>
        /// <param name="writer">用于写入数据的二进制写入器。</param>
        void SendAsyncWriter(ExtenderBinaryWriter writer);

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="data">要发送的数据</param>
        public void SendAsync(byte[] data);

        /// <summary>
        /// 异步发送数据。
        /// </summary>
        /// <param name="data">要发送的数据字节数组。</param>
        /// <param name="start">开始发送的字节位置。</param>
        /// <param name="length">要发送的字节长度。</param>
        void SendAsync(byte[] data, int start, int length);

        /// <summary>
        /// 异步发送数据。
        /// </summary>
        /// <param name="memory">要发送的数据。</param>
        void SendAsync(Memory<byte> memory);

        /// <summary>
        /// 关闭连接。
        /// </summary>
        /// <param name="requireFullTransmission">是否要求完整传输。默认为false。</param>
        /// <param name="requireFullDataProcessing">是否要求完整数据处理。默认为false。</param>
        void Close(bool requireFullTransmission = false, bool requireFullDataProcessing = false);
    }
}
