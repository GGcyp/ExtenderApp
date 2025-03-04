using System.Net;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义一个用于操作链接的接口
    /// </summary>
    public interface ILinker : IResettable
    {
        /// <summary>
        /// 获取当前是否已连接。
        /// </summary>
        /// <value>
        /// 如果已连接，则返回 true；否则返回 false。
        /// </value>
        bool Connected { get; }

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
        /// 发送数据
        /// </summary>
        /// <typeparam name="T">发送数据的类型</typeparam>
        /// <param name="value">要发送的数据</param>
        void Send<T>(T value);

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();
    }
}
