using System.Net.Sockets;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 用于创建 <see cref="ILinker"/> 实例的工厂接口。
    /// </summary>
    /// <remarks>
    /// 实现应返回与工厂所表示协议/套接字类型相匹配的 <see cref="ILinker"/> 实例。
    /// 该接口为非泛型入口，适合在不需要具体实现类型的场景下使用（例如通过依赖注入解析工厂实例）。
    /// </remarks>
    public interface ILinkerFactory
    {
        /// <summary>
        /// 链接器所使用的协议类型（例如 <see cref="ProtocolType.Tcp"/> 或 <see cref="ProtocolType.Udp"/>）。
        /// </summary>
        ProtocolType ProtocolType { get; }

        /// <summary>
        /// 链接器所使用的套接字类型（例如 <see cref="SocketType.Stream"/> 或 <see cref="SocketType.Dgram"/>）。
        /// </summary>
        SocketType SocketType { get; }
    }

    /// <summary>
    /// 生成特定 <typeparamref name="TLinker"/> 类型 <see cref="ILinker"/> 的工厂接口。
    /// </summary>
    /// <typeparam name="TLinker">实现了 <see cref="ILinker"/> 的具体类型。</typeparam>
    /// <remarks>
    /// 该接口在保留 <see cref="ILinkerFactory"/> 行为的同时，提供返回具体类型的方法重载以免频繁转换。
    /// 使用 <c>new</c> 关键字隐藏基接口的同名成员，返回更具体的类型。
    /// </remarks>
    public interface ILinkerFactory<TLinker> : ILinkerFactory where TLinker : ILinker
    {
        /// <summary>
        /// 创建一个类型为 <typeparamref name="TLinker"/> 的 <see cref="ILinker"/> 实例。
        /// </summary>
        /// <returns>返回类型为 <typeparamref name="TLinker"/> 的实例。</returns>
        TLinker CreateLinker();

        /// <summary>
        /// 使用指定 <see cref="Socket"/> 创建一个类型为 <typeparamref name="TLinker"/> 的链接器实例。
        /// </summary>
        /// <param name="socket">用于创建链接器的 <see cref="Socket"/>。</param>
        /// <returns>返回类型为 <typeparamref name="TLinker"/> 的实例。</returns>
        TLinker CreateLinker(Socket socket);

        /// <summary>
        /// 为指定的地址族创建一个类型为 <typeparamref name="TLinker"/> 的链接器实例。
        /// </summary>
        /// <param name="addressFamily">要使用的地址族。</param>
        /// <returns>返回类型为 <typeparamref name="TLinker"/> 的实例。</returns>
        TLinker CreateLinker(AddressFamily addressFamily);
    }
}
