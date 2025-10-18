using System.Net.Sockets;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 监听器链接器工厂抽象基类。
    /// 负责根据指定的 <see cref="AddressFamily"/> 或 <see cref="Socket"/> 创建协议一致的 <see cref="IListenerLinker{T}"/>。
    /// </summary>
    /// <typeparam name="T">链接器类型，必须实现 <see cref="ILinker"/>。</typeparam>
    public class ListenerLinkerFactory<T> : IListenerLinkerFactory<T> where T : ILinker
    {
        private readonly ILinkerFactory<T> _linkerFactory;

        /// <summary>
        /// 链接器所需的 <see cref="SocketType"/>（如 <see cref="SocketType.Stream"/> 表示 TCP）。
        /// </summary>
        protected SocketType LinkerSocketType => _linkerFactory.LinkerSocketType;

        /// <summary>
        /// 链接器所需的 <see cref="ProtocolType"/>（如 <see cref="ProtocolType.Tcp"/>）。
        /// </summary>
        protected ProtocolType LinkerProtocolType => _linkerFactory.LinkerProtocolType;

        /// <summary>
        /// 使用指定的链接器工厂构造监听器链接器工厂。
        /// </summary>
        /// <param name="linkerFactory">用于将已接入的 <see cref="Socket"/> 包装为 <typeparamref name="T"/> 的工厂。</param>
        /// <exception cref="ArgumentNullException"><paramref name="linkerFactory"/> 为 null。</exception>
        public ListenerLinkerFactory(ILinkerFactory<T> linkerFactory)
        {
            _linkerFactory = linkerFactory;
        }

        /// <summary>
        /// 使用默认地址族（IPv4，<see cref="AddressFamily.InterNetwork"/>）创建监听器链接器。
        /// </summary>
        /// <returns>监听器链接器实例。</returns>
        public IListenerLinker<T> CreateListenerLinker()
        {
            return CreateListenerLinker(AddressFamily.InterNetwork);
        }

        /// <summary>
        /// 使用指定地址族创建监听器链接器。
        /// </summary>
        /// <param name="addressFamily">地址族，例如 <see cref="AddressFamily.InterNetwork"/> 或 <see cref="AddressFamily.InterNetworkV6"/>。</param>
        /// <returns>监听器链接器实例。</returns>
        public IListenerLinker<T> CreateListenerLinker(AddressFamily addressFamily)
        {
            return CreateListenerLinker(new Socket(addressFamily, LinkerSocketType, LinkerProtocolType));
        }

        /// <summary>
        /// 使用给定的 <see cref="Socket"/> 创建监听器链接器。
        /// </summary>
        /// <param name="socket">已按协议构造的套接字。</param>
        /// <returns>监听器链接器实例。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="socket"/> 为 null。</exception>
        /// <exception cref="ArgumentException">传入的 <paramref name="socket"/> 的 <see cref="Socket.SocketType"/> 或 <see cref="Socket.ProtocolType"/> 与当前工厂要求不一致。</exception>
        public IListenerLinker<T> CreateListenerLinker(Socket socket)
        {
            if (socket is null)
                throw new ArgumentNullException(nameof(socket));
            if (socket.SocketType != LinkerSocketType || socket.ProtocolType != LinkerProtocolType)
                throw new ArgumentException(string.Format(string.Format("生成监听器链接器的套字节类型和协议类型不一致:{0}", typeof(T).Name)), nameof(socket));

            return new SocketListenerLinker<T>(socket, _linkerFactory);
        }
    }
}
