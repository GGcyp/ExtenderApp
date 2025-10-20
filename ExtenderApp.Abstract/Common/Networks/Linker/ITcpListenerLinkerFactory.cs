using System.Net.Sockets;


namespace ExtenderApp.Abstract
{
    /// <summary>
    /// TCP 监听器链接器工厂。
    /// 提供基于默认地址族、指定地址族或给定 <see cref="Socket"/> 的创建方法。
    /// </summary>
    public interface ITcpListenerLinkerFactory
    {
        /// <summary>
        /// 使用默认地址族（IPv4，<see cref="AddressFamily.InterNetwork"/>）创建监听器链接器。
        /// </summary>
        /// <returns>监听器链接器实例。</returns>
        ITcpListenerLinker CreateListenerLinker();

        /// <summary>
        /// 使用给定的 <see cref="Socket"/> 创建监听器链接器。
        /// </summary>
        /// <param name="socket">已按协议构造的套接字。</param>
        /// <returns>监听器链接器实例。</returns>
        ITcpListenerLinker CreateListenerLinker(Socket socket);

        /// <summary>
        /// 使用指定地址族创建监听器链接器。
        /// </summary>
        /// <param name="addressFamily">地址族，例如 <see cref="AddressFamily.InterNetwork"/> 或 <see cref="AddressFamily.InterNetworkV6"/>。</param>
        /// <returns>监听器链接器实例。</returns>
        ITcpListenerLinker CreateListenerLinker(AddressFamily addressFamily);
    }
}
