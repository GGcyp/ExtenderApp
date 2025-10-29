using System.Net.Sockets;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 链路客户端工厂（非泛型）：描述工厂可创建的底层套接字类型与协议。
    /// </summary>
    /// <remarks>
    /// 实现应表明其创建的链路所使用的 <see cref="LinkerSocketType"/> 与 <see cref="LinkerProtocolType"/>，
    /// 以便调用方在构建或匹配链路时做出正确选择（例如 TCP/UDP、流/数据报）。
    /// </remarks>
    public interface ILinkClientFactory
    {
        /// <summary>
        /// 工厂创建的链路所使用的协议类型（例如 <see cref="ProtocolType.Tcp"/>、<see cref="ProtocolType.Udp"/> 等）。
        /// </summary>
        ProtocolType ProtocolType { get; }

        /// <summary>
        /// 工厂创建的链路所使用的套接字类型（例如 <see cref="SocketType.Stream"/>、<see cref="SocketType.Dgram"/> 等）。
        /// </summary>
        SocketType SocketType { get; }
    }

    /// <summary>
    /// 链路客户端工厂（泛型）：用于创建具体 <typeparamref name="TLinkClient"/> 实例的工厂契约。
    /// </summary>
    /// <typeparam name="TLinkClient">要创建的客户端类型，必须实现 <see cref="ILinkClient"/>。</typeparam>
    public interface ILinkClientFactory<TLinkClient> : ILinkClientFactory where TLinkClient : ILinkClient
    {
        /// <summary>
        /// 创建一个新的、未关联底层 <see cref="Socket"/> 的 <typeparamref name="TLinkClient"/> 实例。
        /// </summary>
        /// <returns>新创建的 <typeparamref name="TLinkClient"/> 实例，尚未与外部套接字绑定（实现可选择延后创建底层 Socket 或在构造时内部创建）。</returns>
        TLinkClient CreateLinkClient();

        /// <summary>
        /// 使用已有的 <see cref="Socket"/> 创建并包装为 <typeparamref name="TLinkClient"/> 实例。
        /// </summary>
        /// <param name="socket">用于构造链路的已创建 <see cref="Socket"/>（不得为 null）。</param>
        /// <returns>包装后的 <typeparamref name="TLinkClient"/> 实例，通常由工厂负责将该 socket 与实现关联。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="socket"/> 为 null。</exception>
        TLinkClient CreateLinkClient(Socket socket);

        /// <summary>
        /// 按指定地址族创建一个新的 <typeparamref name="TLinkClient"/> 实例。
        /// 实现通常会基于 <paramref name="addressFamily"/> 与工厂声明的 <see cref="LinkerSocketType"/> / <see cref="LinkerProtocolType"/> 创建并绑定底层套接字。
        /// </summary>
        /// <param name="addressFamily">套接字的地址族（例如 <see cref="AddressFamily.InterNetwork"/>、<see cref="AddressFamily.InterNetworkV6"/>）。</param>
        /// <returns>新创建并已初始化为给定地址族的 <typeparamref name="TLinkClient"/> 实例。</returns>
        TLinkClient CreateLinkClient(AddressFamily addressFamily);

        /// <summary>
        /// 通过已有的 <see cref="ILinker"/> 实例创建并包装为 <typeparamref name="TLinkClient"/> 实例。
        /// </summary>
        /// <param name="linker">已有的<see cref="ILinker"/> 实例。</param>
        /// <returns><typeparamref name="TLinkClient"/> 实例。</returns>
        TLinkClient CreateLinkClient(ILinker linker);
    }
}