using System.Net;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// Tcp 链接客户端接口：表示一个可进行 TCP 连接的客户端抽象。
    /// </summary>
    public interface ITcpLinkClient : ILinkClientAwareSender<ITcpLinkClient>
    {
        /// <summary>
        /// 是否禁用 Nagle 算法（仅对 TCP 有效）。
        /// </summary>
        /// <remarks>
        /// - true：小包将尽快发送，降低延迟但可能增加包数量；<br/>
        /// - false：允许合并小包，降低包数量但可能增加延迟。
        /// </remarks>
        bool NoDelay { get; set; }

        /// <summary>
        /// 同步连接到指定 IP 地址集合和端口。
        /// </summary>
        /// <param name="addresses">
        /// 要尝试连接的远端 IP 地址数组，不能为 <c>null</c> 或空数组。实现通常按数组顺序尝试连接，直到成功或耗尽所有地址。
        /// </param>
        /// <param name="port">
        /// 目标端口号，应在有效端口范围（0-65535）内。非法端口应导致 <see cref="System.ArgumentOutOfRangeException"/>。
        /// </param>
        void Connect(IPAddress[] addresses, int port);

        /// <summary>
        /// 异步连接到指定 IP 地址集合和端口。
        /// </summary>
        /// <param name="addresses">
        /// 要尝试连接的远端 IP 地址数组，不能为 <c>null</c> 或空数组。实现应按一定策略（例如按顺序或并行）尝试这些地址。
        /// </param>
        /// <param name="port">
        /// 目标端口号，应在有效端口范围内。非法端口应导致 <see cref="System.ArgumentOutOfRangeException"/>。
        /// </param>
        /// <param name="token">
        /// 用于取消连接操作的 <see cref="CancellationToken"/>。实现应在取消时尽快中止尝试并释放相关临时资源。
        /// </param>
        /// <returns>
        /// 一个表示异步连接操作的 <see cref="ValueTask"/>。在取消时可以抛出 <see cref="OperationCanceledException"/> 或以已取消的语义完成任务（应在实现文档中说明）。
        /// </returns>
        ValueTask ConnectAsync(IPAddress[] addresses, int port, CancellationToken token = default);
    }
}