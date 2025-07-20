using System.Net;
using System.Net.Sockets;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 监听器连接器接口
    /// </summary>
    /// <typeparam name="T">ILinker 接口的实现类型</typeparam>
    public interface IListenerLinker<T> : IDisposable where T : ILinker
    {
        /// <summary>
        /// 接受连接并返回 ILinker 接口的实现类型对象
        /// </summary>
        /// <returns>ILinker 接口的实现类型对象</returns>
        T Accept();

        /// <summary>
        /// 开始接受连接
        /// </summary>
        /// <param name="callback">连接成功后的回调函数</param>
        void BeginAccept(Action<T> callback);

        /// <summary>
        /// 绑定 IP 地址和端口号
        /// </summary>
        /// <param name="address">IP 地址</param>
        /// <param name="port">端口号</param>
        void Bind(IPAddress address, int port);

        /// <summary>
        /// 绑定 IP 端点
        /// </summary>
        /// <param name="iPEndPoint">IP 端点</param>
        void Bind(IPEndPoint iPEndPoint);

        /// <summary>
        /// 开始监听连接
        /// </summary>
        /// <param name="backlog">连接请求队列的最大长度，默认为 10</param>
        void Listen(int backlog = 10);

        /// <summary>
        /// 初始化方法
        /// </summary>
        /// <param name="addressFamily">地址族</param>
        /// <remarks>
        /// 初始化网络通讯相关的设置，指定使用的地址族。
        /// </remarks>
        void Init(AddressFamily addressFamily);

        /// <summary>
        /// 初始化方法（针对互联网协议）
        /// </summary>
        /// <remarks>
        /// 初始化网络通讯相关的设置，专门用于互联网协议（IPv4）。
        /// </remarks>
        void InitInterNetwork();
    }
}
