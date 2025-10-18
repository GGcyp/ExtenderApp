using System.Net.Sockets;


namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 监听器链接器工厂接口：提供多种方式创建 <see cref="IListenerLinker{T}"/>。
    /// </summary>
    /// <typeparam name="T">链接器类型，必须实现 <see cref="ILinker"/>。</typeparam>
    public interface IListenerLinkerFactory<T> where T : ILinker
    {
        /// <summary>
        /// 使用默认地址族（IPv4，<see cref="AddressFamily.InterNetwork"/>）创建监听器链接器。
        /// </summary>
        /// <returns>监听器链接器实例。</returns>
        IListenerLinker<T> CreateListenerLinker();

        /// <summary>
        /// 使用给定的 <see cref="Socket"/> 创建监听器链接器。
        /// </summary>
        /// <param name="socket">已按协议构造的套接字。</param>
        /// <returns>监听器链接器实例。</returns>
        IListenerLinker<T> CreateListenerLinker(Socket socket);

        /// <summary>
        /// 使用指定地址族创建监听器链接器。
        /// </summary>
        /// <param name="addressFamily">地址族，例如 <see cref="AddressFamily.InterNetwork"/> 或 <see cref="AddressFamily.InterNetworkV6"/>。</param>
        /// <returns>监听器链接器实例。</returns>
        IListenerLinker<T> CreateListenerLinker(AddressFamily addressFamily);
    }
}
