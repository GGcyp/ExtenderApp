using System.Net;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 客户端插件：用于拦截连接生命周期与收发数据管道（例如为数据添加/剥离网络头、统计或加解密等）。 
    /// 插件通过实现本接口可在连接的各个阶段注入自定义逻辑或在发送/接收数据时修改/解析消息。
    /// </summary>
    public interface ILinkClientPlugin<TLinkClient>
        where TLinkClient : ILinkClientAwareSender<TLinkClient>
    {
        /// <summary>
        /// 当插件附加到客户端时被调用。用于初始化插件状态、解析配置或从依赖注入容器获取服务等。
        /// 调用时机：通常在客户端构建或启动阶段、实际建立连接之前。
        /// </summary>
        /// <param name="client">被附加的客户端实例。</param>
        void OnAttach(TLinkClient client);

        /// <summary>
        /// 当插件从客户端分离时被调用。用于释放托管/非托管资源、取消订阅事件或清理状态。
        /// 调用后插件应不再持有对客户端的活动引用（除非外部明确保留）。
        /// </summary>
        /// <param name="client">被分离的客户端实例。</param>
        void OnDetach(TLinkClient client);

        /// <summary>
        /// 在发起连接之前调用（还未开始网络连接操作）。插件可以在此检查或修改目标终结点、记录连接尝试等。
        /// 注意：此回调不是连接完成的通知，仅表示即将开始连接流程。
        /// </summary>
        /// <param name="client">客户端实例。</param>
        /// <param name="remoteEndPoint">目标远端终结点（目标地址/端口）。</param>
        void OnConnecting(TLinkClient client, EndPoint remoteEndPoint);

        /// <summary>
        /// 当连接流程结束后调用（无论成功或失败）。若连接成功，<paramref name="exception"/> 为 null；若连接失败，携带相应异常。
        /// 插件可在此根据结果执行后续初始化或重试策略记录错误等。
        /// </summary>
        /// <param name="client">客户端实例。</param>
        /// <param name="remoteEndPoint">目标远端终结点（可能为 null，取决于实现）。</param>
        /// <param name="exception">连接失败时的异常；连接成功时为 null。</param>
        void OnConnected(TLinkClient client, EndPoint remoteEndPoint, Exception? exception);

        /// <summary>
        /// 在发起断开（即将断开）之前调用。适合执行优雅断开所需的预处理（如发送告知报文、停止定时器等）。
        /// </summary>
        /// <param name="client">客户端实例。</param>
        void OnDisconnecting(TLinkClient client);

        /// <summary>
        /// 断开完成后调用。若断开由错误导致，<paramref name="error"/> 会携带异常信息；若为正常断开则为 null。
        /// 插件可在此释放与连接相关的临时资源或记录断开原因。
        /// </summary>
        /// <param name="client">客户端实例。</param>
        /// <param name="error">断开原因的异常（出错断开）或 null（正常断开）。</param>
        void OnDisconnected(TLinkClient client, Exception? error);

        /// <summary>
        /// 发送管线：在最终发送到底层之前调用，允许插件对要发送的消息进行包装或替换（例如添加帧头、压缩或加密）。
        /// 说明与约束：
        /// - 参数 <see cref="LinkClientPluginSendMessage"/> 为 <c>ref struct</c>（栈上类型），不得被捕获、装箱或在异步边界外存储。  
        /// - 插件可以通过修改 <see cref="LinkClientPluginSendMessage.FirstMessageBuffer"/> 或 <see cref="LinkClientPluginSendMessage.OutMessageBuffer"/> 来前置或替换消息内容；也可以不修改直接使用原始消息。  
        /// - 插件不应在返回后继续使用或保留对其中缓冲的引用；若需要跨越上下文传递数据，应复制到托管堆上的副本并自行管理其生命周期。  
        /// - 最终发送的字节由调用方通过 <see cref="LinkClientPluginSendMessage.ToBlock"/> 合并并发送，调用方负责释放返回的 <see cref="ByteBlock"/>。
        /// </summary>
        /// <param name="client">客户端实例。</param>
        /// <param name="message">发送消息的临时封装（ref struct），插件可在其中读/写缓冲以调整最终发送内容）。</param>
        void OnSend(TLinkClient client, ref LinkClientPluginSendMessage message);

        /// <summary>
        /// 接收管线：当收到原始字节块时调用，插件负责对原始数据进行解析/解包并将解析出的业务帧放入 <see cref="LinkClientPluginReceiveMessage.OutMessageFrames"/>。
        /// 说明与约束：
        /// - 参数 <see cref="LinkClientPluginReceiveMessage"/> 为 <c>ref struct</c>，只能在当前调用栈内使用，不可装箱或跨异步边界保存。  
        /// - <see cref="LinkClientPluginReceiveMessage.ResultMessage"/> 是对本次接收数据的只读视图，可能引用池化内存；请勿在回调返回后继续持有该引用。  
        /// - 若插件向 <see cref="OutMessageFrames"/> 添加了帧，调用者会在适当时机调用其 <see cref="PooledFrameList.Dispose"/> 或对帧逐一 Dispose；插件无需在返回后手动释放这些帧（调用方将负责），但应确保按文档语义正确填充。  
        /// - 插件应尽量避免抛出未处理异常；若发生异常应以日志/指标记录，并让上层决定是否断开连接或重试。
        /// </summary>
        /// <param name="client">客户端实例。</param>
        /// <param name="message">接收消息的临时封装（ref struct），包含本次原始字节与用于输出解析后帧的集合）。</param>
        void OnReceive(TLinkClient client, ref LinkClientPluginReceiveMessage message);
    }
}
