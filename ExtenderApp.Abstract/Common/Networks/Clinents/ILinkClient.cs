
namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示客户端侧的链路最小抽象，暴露运行时链路信息（来自 <see cref="ILinkInfo"/>）。
    /// </summary>
    /// <typeparam name="TLinker">
    /// 关联的底层链路实现类型，必须实现 <see cref="ILinker"/>。
    /// </typeparam>
    /// <remarks>
    /// 该接口刻意保持精简，仅作为“只读运行时信息”的语义契约（连接状态、本地/远端终结点、限流与统计等）。
    /// 
    /// 与扩展能力的关系：
    /// - 序列化/反序列化（格式化器）、插件托管与泛型发送等能力应拆分为独立能力接口（例如 ILinkClientFormatting、
    ///   ILinkClientPluginHost{TLinkClient}、ILinkClientSender），以便 HTTP 或其它只需基础信息的场景仅依赖最小接口。
    /// - 实现类可按需额外实现上述能力接口；若需要兼容旧实现，也可让本接口继承这些能力接口。
    /// 
    /// 示例（仅示意，视项目实际定义调整）：
    /// public interface ILinkClient<TLinkClient> : ILinkInfo, ILinkClientFormatting, ILinkClientPluginHost<TLinkClient>, ILinkClientSender where TLinkClient : ILinker { }
    /// </remarks>
    public interface ILinkClient : ILinkInfo
    {
    }
}