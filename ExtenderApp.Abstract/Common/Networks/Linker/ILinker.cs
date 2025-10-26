using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示一个可进行“连接 / 断开 / 发送 / 接收”的链路抽象。
    /// </summary>
    /// <remarks>
    /// 接口聚合了运行时信息（<see cref="ILinkInfo"/>）、连接管理（<see cref="ILinkConnect"/>）、
    /// 发送（<see cref="ILinkSender"/>）与接收（<see cref="ILinkReceiver"/>）能力，并要求实现对资源负责的释放（通过 <see cref="IDisposable.Dispose"/>）。
    ///
    /// 设计与使用约定（实现者与调用者应遵守或在实现文档中明确）：
    /// - 线程安全：应保证并发调用的正确性。常见做法是对同类操作（发送/接收）进行内部串行化或使用并发安全的数据结构，避免竞态与资源泄露。
    /// - 生命周期与资源释放：实现必须在 <see cref="IDisposable.Dispose"/> 中释放底层套接字、计时器、租约等资源；对已释放对象的调用应抛出 <see cref="System.ObjectDisposedException"/>。
    /// - 异常与返回语义：建议将可预期的网络/协议错误通过 <see cref="SocketOperationResult"/> 返回（对于发送/接收/操作结果使用返回值表示失败原因），仅在参数非法或对象已释放等不可恢复编程错误时抛出异常。
    /// - 异步与取消：异步方法应尊重并响应传入的 <see cref="System.Threading.CancellationToken"/>；在取消时可选择抛出 <see cref="System.OperationCanceledException"/> 或以已取消的语义完成任务（应在实现中记录并告知调用方）。
    /// - TCP / UDP 特性：
    ///   - 对于面向连接的协议（如 TCP），Connect/Disconnect 应建立与释放连接资源；发送/接收可能出现“部分发送/部分接收”，调用方在需要完整传输时应循环直至缓冲耗尽或发生错误。
    ///   - 对于无连接协议（如 UDP），实现可选择对底层套接字调用 Connect（将默认远端绑定为 <paramref name="remoteEndPoint"/>），或在每次发送/接收时指定远端；未 Connect 的 UDP 场景下，<see cref="ILinkInfo.RemoteEndPoint"/> 可能为 null。
    ///   - 属性如 <c>NoDelay</c>（若由实现暴露）仅对 TCP 有意义，UDP 可忽略。
    /// - 统计与限流：通过 <see cref="CapacityLimiter"/>、<see cref="ValueCounter"/> 向上层提供限流与统计支持。实现应保证这些实例在运行期非 null 并为线程安全，调用方应在发送/接收前按需申请容量并在完成后释放租约（Lease）。
    /// - 性能建议：实现应避免在高并发场景下引入不必要的线程阻塞；建议使用内存复用（例如 MemoryPool）与异步 I/O（SocketAsyncEventArgs / IOCP）以降低分配与上下文切换开销。
    ///
    /// 语义提示：
    /// - 对于发送/接收方法，返回的 <see cref="SocketOperationResult.BytesTransferred"/>、<see cref="SocketOperationResult.SocketError"/>、
    ///   <see cref="SocketOperationResult.RemoteEndPoint"/> 等字段应尽量完整地提供诊断信息，便于调用方决定重试、记录或降级处理。
    /// - 文档化行为：实现应在其具体文档中明确对重复 Connect/Disconnect 的处理策略、在取消/超时情形下是以异常还是返回特定结果码作为指示等细节。
    /// </remarks>
    public interface ILinker : IDisposable, ILinkInfo, ILinkReceiver, ILinkSender, ILinkConnect
    {
    }
}