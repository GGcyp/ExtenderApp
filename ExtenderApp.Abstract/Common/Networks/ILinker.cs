using System.Net;
using System.Net.Sockets;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义一个用于操作链接的接口
    /// </summary>
    public interface ILinker : IResettable, IConcurrentOperate
    {
        /// <summary>
        /// 获取当前是否已连接。
        /// </summary>
        /// <value>
        /// 如果已连接，则返回 true；否则返回 false。
        /// </value>
        bool Connected { get; }

        /// <summary>
        /// 当接收到数据时触发的事件。
        /// </summary>
        event Action<ReadOnlyMemory<byte>>? OnReceive;

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
        event Action<int, string> OnErrored;

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
        /// 通过端点连接到服务器
        /// </summary>
        /// <param name="point">端点</param>
        void Connect(EndPoint point);

        /// <summary>
        /// 注册一个回调函数
        /// </summary>
        /// <typeparam name="T">回调函数的参数类型</typeparam>
        /// <param name="callback">回调函数</param>
        void Register<T>(Action<T> callback);

        /// <summary>
        /// 注册一个类型为T和ILinker的回调函数
        /// </summary>
        /// <typeparam name="T">回调函数的参数类型</typeparam>
        /// <param name="callback">回调函数</param>
        /// <exception cref="Exception">当当前连接已经关闭时抛出异常</exception>
        /// <exception cref="Exception">当当前连接正在关闭中时抛出异常</exception>
        void Register<T>(Action<T, ILinker> callback);

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <typeparam name="T">发送数据的类型</typeparam>
        /// <param name="value">要发送的数据</param>
        void Send<T>(T value);

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <typeparam name="T">发送的数据类型</typeparam>
        /// <param name="value">要发送的数据</param>
        /// <exception cref="Exception">如果当前连接已经关闭、正在关闭中或还未连接，则抛出异常</exception>
        void SendAsync<T>(T value);

        /// <summary>
        /// 发送源数据
        /// </summary>
        /// <param name="bytes">源数据字节数组</param>
        void SendSource(byte[] bytes);

        /// <summary>
        /// 发送源数据
        /// </summary>
        /// <param name="bytes">源数据字节数组</param>
        /// <param name="offset">开始发送的字节位置</param>
        /// <param name="count">发送的字节数</param>
        void SendSource(byte[] bytes, int offset, int count);

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();

        /// <summary>
        /// 设置Socket连接
        /// </summary>
        /// <param name="socket">要设置的Socket对象</param>
        void Set(Socket socket);
    }
}
