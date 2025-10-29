using System.Net;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// ITcpLinker 接口继承自 ILinker 接口，代表一个 TCP 链接器接口。
    /// </summary>
    public interface ITcpLinker : ILinker
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
        /// <remarks>
        /// - 对于面向连接的协议（例如 TCP），应尝试对 <paramref name="addresses"/> 中的地址建立连接，通常按顺序重试。
        /// - 对于无连接协议（例如 UDP），实现可选择将套接字的默认远端设置为成功的地址/端口或以等效方式配置。
        /// - 若当前已处于连接状态，重复调用应抛出异常或由实现明确文档化其行为。
        /// - 参数非法或对象已释放应抛出相应异常（例如 <see cref="System.ArgumentNullException"/>、<see cref="System.ArgumentOutOfRangeException"/>、<see cref="System.ObjectDisposedException"/>）。
        /// </remarks>
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
        /// <remarks>
        /// - 实现可以选择按顺序尝试地址、并行尝试或使用其他重试策略；应在实现文档中说明所采用的策略。
        /// - 对于面向连接的协议，成功完成表示已建立到某个地址的连接；对于无连接协议，成功完成表示已完成必要的配置（例如设置默认远端）。
        /// - 参数非法或对象已释放应抛出相应异常。
        /// </remarks>
        ValueTask ConnectAsync(IPAddress[] addresses, int port, CancellationToken token = default);
    }
}