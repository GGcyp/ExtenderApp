using System.Net;
using System.Net.Sockets;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示网络链路的运行时信息与相关控制/统计入口。
    /// </summary>
    /// <remarks>
    /// 该接口提供对链路当前状态（连接性、本地/远端终结点）以及用于限流与统计的可用组件的访问。
    /// - 语义上偏向“运行时状态/元数据”，而非单次操作行为。 <br/>
    /// - CapacityLimiter 与 ValueCounter 为线程安全类型，调用方可在并发场景中安全使用它们提供的功能。
    /// </remarks>
    public interface ILinkInfo
    {
        /// <summary>
        /// 指示链路是否处于已连接状态。
        /// </summary>
        /// <remarks>
        /// - 对于面向连接的协议（如 TCP），该值通常反映底层套接字的连接状态。 <br/>
        /// - 对于无连接的协议（如 UDP），若实现未对 socket 调用
        /// Connect，则该值的含义由实现决定（可始终为 true、始终为
        /// false，或表示逻辑上的“就绪”状态）。调用方不应对 UDP 的
        /// Connected 做严格假定，需参考实现文档。
        /// </remarks>
        bool Connected { get; }

        /// <summary>
        /// 获取链路所使用的协议类型。
        /// </summary>
        public ProtocolType ProtocolType { get; }

        /// <summary>
        /// 获取链路所使用的套接字类型。
        /// </summary>
        public SocketType SocketType { get; }

        /// <summary>
        /// 获取链路所使用的地址族。
        /// </summary>
        AddressFamily AddressFamily { get; }

        /// <summary>
        /// 获取或设置接收缓冲区大小（字节）。
        /// </summary>
        int ReceiveBufferSize { get; set; }

        /// <summary>
        /// 获取或设置发送缓冲区大小（字节）。
        /// </summary>
        int SendBufferSize { get; set; }

        /// <summary>
        /// 获取或设置接收超时时间（毫秒）。
        /// </summary>
        int ReceiveTimeout { get; set; }

        /// <summary>
        /// 获取或设置发送超时时间（毫秒）。
        /// </summary>
        int SendTimeout { get; set; }

        /// <summary>
        /// 本地终结点（本地地址与端口），若不可用则为 null。
        /// </summary>
        /// <remarks>
        /// - 对于尚未绑定的套接字可能为 null；实现应在可用时返回实际 <see cref="EndPoint"/>。
        /// </remarks>
        EndPoint? LocalEndPoint { get; }

        /// <summary>
        /// 远端终结点（对端地址与端口），若未确定则为 null。
        /// </summary>
        /// <remarks>
        /// - 对于已 Connect 的
        /// socket（或面向连接协议）应返回对端地址；对未 Connect 的
        /// UDP 可能为 null。
        /// </remarks>
        EndPoint? RemoteEndPoint { get; }

        /// <summary>
        /// 容量闸门（配额/限流组件），用于按字节或权重对发送/接收进行限流与排队。
        /// </summary>
        /// <remarks>
        /// - 不应返回 null；实现可根据需要暴露全局或 per-link
        /// 的限流器。 <br/>
        /// - CapacityLimiter 是线程安全的，调用方应使用其
        /// Lease/Acquire API 在发送/接收前申请容量并在完成后释放，避免超限。
        /// </remarks>
        CapacityLimiter CapacityLimiter { get; }

        /// <summary>
        /// 发送方向的统计计数器（支持周期性结算与快照）。
        /// </summary>
        /// <remarks>
        /// - ValueCounter 为线程安全类型；通过 <see cref="ValueCounter.Increment(long)"/>
        /// 累计已发送字节数或消息数。 <br/>
        /// - 实现应确保该实例长期可用且不会为 null，调用方可订阅或轮询其周期快照以获得统计数据。
        /// </remarks>
        ValueCounter SendCounter { get; }

        /// <summary>
        /// 接收方向的统计计数器（支持周期性结算与快照）。
        /// </summary>
        /// <remarks>
        /// - 语义与 <see cref="SendCounter"/> 对应，用于累计接收字节数或消息数并提供周期性统计信息。
        /// </remarks>
        ValueCounter ReceiveCounter { get; }

        /// <summary>
        /// 为链接器设置指定的选项。
        /// </summary>
        /// <param name="optionLevel">选项所属的协议层级（例如 IP/Tcp/Socket）。</param>
        /// <param name="optionName">要设置的选项名称。</param>
        /// <param name="optionValue">选项值。</param>
        void SetOption(LinkOptionLevel optionLevel, LinkOptionName optionName, DataBuffer optionValue);
    }
}