using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 客户端侧链路抽象。基于 <see cref="ILinkInfo"/> 提供链路运行时信息，并向上层暴露发送、格式化器与插件管理入口。
    /// </summary>
    /// <typeparam name="TLinker">关联的底层链路实现类型，必须实现 <see cref="ILinker"/>。</typeparam>
    /// <remarks>
    /// 设计目标：
    /// - 将网络链路与业务逻辑解耦：向业务层提供简单的发送入口与只读运行时信息（终结点、统计、限流器等）。  
    /// - 支持扩展点：通过可选的格式化器管理器与插件管理器实现序列化/反序列化、报文包装/解包、日志、限流与度量等功能。  
    ///
    /// 使用约定：
    /// - <see cref="FormatterManager"/> 与 <see cref="PluginManager"/> 均为可空；在调用依赖它们的 API（例如泛型 <see cref="SendAsync{T}"/>）前应确保已正确设置。  
    /// - 调用 <see cref="SetClientFormatterManager"/> 与 <see cref="SetClientPluginManager"/> 应在首次使用前完成初始化；重复设置的策略由具体实现决定（覆盖或抛出异常）。  
    /// - 泛型发送方法应使用 <see cref="FormatterManager"/> 获取格式化器并将业务对象序列化为字节缓冲后发送；若格式化器缺失，实现在文档中应明确是抛出异常还是通过返回的 <see cref="SocketOperationResult"/> 表达错误。  
    /// - 线程安全：接口实现应在并发场景下明确线程安全语义；建议对发送/接收做内部串行化或使用并发安全结构以避免竞态。  
    /// - 生命周期：实现通常参与更大的链路生命周期管理；必要时在实现文档中说明 Dispose/Detach 的时序与语义。
    /// </remarks>
    public interface ILinkClient<TLinker> : ILinkInfo where TLinker : ILinker
    {
        /// <summary>
        /// 可选的格式化器管理器，用于按类型或数据类型哈希路由序列化/反序列化器。
        /// </summary>
        /// <remarks>
        /// - 若为 <c>null</c>，表示当前客户端未配置格式化器管理器，依赖格式化器的操作（如泛型发送）将不可用或失败。  
        /// - 建议实现与管理器均为线程安全，以支持运行期并发发送/接收。
        /// </remarks>
        ILinkClientFormatterManager? FormatterManager { get; }

        /// <summary>
        /// 可选的插件管理器，用于注册与托管客户端侧插件以拦截连接生命周期、发送与接收事件。
        /// </summary>
        /// <remarks>
        /// - 插件可用于数据二次包装/解包、日志/度量、限流或自定义协议处理。  
        /// - 若为 <c>null</c>，表示未启用插件扩展点；实现应在插件为空时跳过回调以保证性能。
        /// </remarks>
        ILinkClientPluginManager<TLinker>? PluginManager { get; }

        /// <summary>
        /// 将业务对象序列化并异步发送到远端。
        /// </summary>
        /// <typeparam name="T">要发送的业务对象的类型。</typeparam>
        /// <param name="data">要发送的业务对象实例。</param>
        /// <returns>
        /// 一个 <see cref="ValueTask{SocketOperationResult}"/>，表示异步发送操作的完成情况并携带发送结果（已发送字节数、底层错误信息、统一结果码等）。
        /// </returns>
        /// <remarks>
        /// - 实现通常通过 <see cref="FormatterManager"/> 获取类型对应的格式化器将 <paramref name="data"/> 序列化为字节缓冲后发送。  
        /// - 若 <see cref="FormatterManager"/> 为 <c>null</c> 或找不到对应格式化器，行为由实现决定（可抛出 <see cref="InvalidOperationException"/> 或返回带错误信息的 <see cref="SocketOperationResult"/>）。  
        /// - 调用方应以返回的 <see cref="SocketOperationResult"/> 决定是否重试、记录或降级处理。  
        /// - 方法应尽量避免在常见网络错误下抛出异常，而是通过返回值提供错误信息；仅在参数非法或对象已释放等编程错误时抛出异常。
        /// </remarks>
        ValueTask<SocketOperationResult> SendAsync<T>(T data);

        /// <summary>
        /// 设置客户端格式化器管理器实例。
        /// </summary>
        /// <param name="formatterManager">要设置的 <see cref="ILinkClientFormatterManager"/> 实例，不能为空。</param>
        /// <exception cref="System.ArgumentNullException">当 <paramref name="formatterManager"/> 为 <c>null</c> 时抛出。</exception>
        void SetClientFormatterManager(ILinkClientFormatterManager formatterManager);

        /// <summary>
        /// 设置客户端插件管理器实例。
        /// </summary>
        /// <param name="pluginManager">要设置的 <see cref="ILinkClientPluginManager{TLinker}"/> 实例，不能为空。</param>
        /// <exception cref="System.ArgumentNullException">当 <paramref name="pluginManager"/> 为 <c>null</c> 时抛出。</exception>
        void SetClientPluginManager(ILinkClientPluginManager<TLinker> pluginManager);
    }
}