using System.Net;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 客户端插件接口。
    /// 用于在链接生命周期与收发数据管线中注入自定义逻辑（例如添加/剥离报文头、统计、压缩/加解密等）。
    /// 插件实现可在客户端关键事件点运行自定义代码，且应遵循所有权与线程边界的约定（详见方法说明）。
    /// </summary>
    public interface ILinkClientPlugin : IDisposable
    {
        /// <summary>
        /// 当插件被附加到客户端实例时调用。
        /// 在此方法中插件应执行初始化工作（例如解析配置、申请资源、从 DI 容器获取依赖等）。
        /// </summary>
        /// <param name="client">正在附加该插件的客户端实例。</param>
        void OnAttach(ILinkClientAwareSender client);

        /// <summary>
        /// 当插件从客户端分离时调用。应在此释放托管/非托管资源并取消订阅事件。
        /// 调用后插件应不再持有对客户端的活动引用（除非外部另行保留）。
        /// </summary>
        void OnDetach();

        /// <summary>
        /// 在发起连接之前调用（仅表示将要开始连接流程，不代表连接已建立）。
        /// 插件可以在此检查或修改目标终结点、记录连接尝试或准备相关上下文数据。
        /// </summary>
        /// <param name="remoteEndPoint">目标远端终结点（目标地址/端口）。实现可查看或替换该值以影响后续连接流程（视客户端实现而定）。</param>
        void OnConnecting(EndPoint remoteEndPoint);

        /// <summary>
        /// 连接流程结束后调用（无论成功或失败）。
        /// 若连接成功，<paramref name="exception"/> 为 null；否则携带失败原因。插件可据此初始化连接相关资源或记录结果。
        /// </summary>
        /// <param name="remoteEndPoint">目标远端终结点（可能为 null，取决于实现）。</param>
        /// <param name="exception">连接失败时的异常；连接成功时为 null。</param>
        void OnConnected(EndPoint remoteEndPoint, Exception? exception);

        /// <summary>
        /// 在发起断开操作之前调用（即将断开）。
        /// 可在此执行优雅断开所需的预处理（例如发送告知报文、停止定时器、持久化状态等）。
        /// </summary>
        void OnDisconnecting();

        /// <summary>
        /// 断开完成后调用。若断开因错误导致，<paramref name="error"/> 将包含异常信息；正常断开则为 null。
        /// 插件可在此释放与连接相关的临时资源或记录断开原因与指标。
        /// </summary>
        /// <param name="error">断开原因的异常（出错断开）或 null（正常断开）。</param>
        void OnDisconnected(Exception? error);

        /// <summary>
        /// 发送管线回调：在最终写入底层套接字/发送队列之前调用。
        /// 插件可以在此修改或替换要发送的帧（例如添加协议头、压缩或加密）。
        /// </summary>
        /// <remarks>
        /// - 使用 <see cref="FrameContext"/> 作为临时封装；调用方与插件应约定缓冲所有权与释放责任。  
        /// - 插件不得将对栈上或易失性缓冲的引用保留到异步边界之外；若需跨异步边界传递数据请复制到堆上。  
        /// - 避免长时间阻塞或抛出未捕获异常；如需报告错误请记录并让上层决定是否断开连接。
        /// </remarks>
        /// <param name="frame">发送消息的临时封装（插件可读取或替换其内部负载）。</param>
        void OnSend(ref FrameContext frame);

        /// <summary>
        /// 接收管线回调：当客户端收到原始数据或完成一次套接字接收操作时调用。
        /// 插件在此负责对原始字节进行解析/解包，并将解析出的业务帧或处理结果放回到提供的 <see cref="FrameContext"/>（或按调用约定交付给上层）。
        /// </summary>
        /// <remarks>
        /// - <paramref name="operationValue"/> 包含本次套接字操作的元数据（如接收字节数、远端地址、错误码等）。  
        /// - 插件不应在回调返回后保留对外部池化内存的引用；如需长期保存请复制数据到托管堆。  
        /// - 若解析出多条业务帧，请使用约定的集合/回调上报并确保按约定释放（或由上层释放）。  
        /// - 插件应尽量避免抛出未处理异常；若发生异常请记录并让上层决定是否断开或重试。
        /// </remarks>
        /// <param name="operationValue">与当前套接字接收操作相关的值/元数据。</param>
        /// <param name="frame">用于承载或返回解析结果的临时封装；插件可修改其内部负载或用新的负载替换之。</param>
        void OnReceive(SocketOperationValue operationValue, ref FrameContext frame);
    }
}