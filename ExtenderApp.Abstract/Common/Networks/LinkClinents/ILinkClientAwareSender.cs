using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 支持格式化器与插件的链路客户端扩展能力接口。
    /// 表示客户端既具备基础的连接/字节发送能力（<see cref="ILinkConnect"/>, <see cref="ILinkSender"/>），
    /// 又支持按“数据类型”路由的可扩展序列化/反序列化（可自定义接收/发送的数据格式类型）与插件管线。
    /// </summary>
    /// <remarks>
    /// 设计意图：
    /// - 通过 <see cref="FormatterManager"/> 按数据类型标识（MessageType）路由反序列化器/序列化器，从而支持发送端与接收端协商/扩展多种消息格式；  
    /// - 通过 <see cref="PluginManager"/> 插件在发送前/接收后拦截与变换消息帧（例如添加/移除网络头、分包/合包等）；  
    /// - FormatterManager 与 PluginManager 均为可选依赖，调用方应在使用泛型发送/按类型反序列化前确保已设置对应管理器（通过 SetClientFormatterManager / SetClientPluginManager）。  
    /// 实现说明：
    /// - SendAsync<TLinkClient> 使用 FormatterManager.GetFormatter<TLinkClient>() 进行序列化并经 Plugin 管线加工后发送；  
    /// - 接收端先通过 Plugin 管线对原始字节解包，插件可将结果填充到 <c>LinkClientPluginReceiveMessage.OutMessageFrames</c>（包含 MessageType），
    ///   然后由 FormatterManager 按 MessageType 路由到对应的格式化器进行反序列化与分发。
    /// </remarks>
    public interface ILinkClientAwareSender<TLinkClient> : ILinkClient, ILinkConnect, ILinkSender
        where TLinkClient : ILinkClientAwareSender<TLinkClient>
    {
        /// <summary>
        /// 可选的插件管理器，用于注册/托管客户端插件并聚合分发连接/收发事件回调。
        /// 插件可在 OnSend/OnReceive 中对消息进行包装或解包，影响最终发送内容或交付给格式化器的业务负载。
        /// </summary>
        ILinkClientPluginManager<TLinkClient>? PluginManager { get; }

        /// <summary>
        /// 可选的格式化器管理器，用于按数据类型标识（MessageType）路由序列化/反序列化器。
        /// - 发送：用于根据泛型类型 TLinkClient 查找序列化器并生成带有稳定数据类型标识的字节负载；  
        /// - 接收：用于根据插件解析出的 MessageType 查找对应反序列化器并将业务对象交付给订阅方。
        /// </summary>
        ILinkClientFormatterManager? FormatterManager { get; }

        /// <summary>
        /// 连接客户端消息打包/解包器实例（可为 null）。
        /// </summary>
        ILinkClientFramer? Framer { get; }

        /// <summary>
        /// 设置客户端插件管理器实例（不得为 null）。建议在调用泛型发送或启用插件逻辑前先设置。
        /// </summary>
        /// <param name="pluginManager">要设置的插件管理器实例（不能为 null）。</param>
        void SetClientPluginManager(ILinkClientPluginManager<TLinkClient> pluginManager);

        /// <summary>
        /// 设置客户端格式化器管理器实例（不得为 null）。若要使用泛型 SendAsync&lt;TLinkClient&gt; 或自动按 MessageType 分发接收消息，必须先设置该管理器。
        /// </summary>
        /// <param name="formatterManager">要设置的格式化器管理器实例（不能为 null）。</param>
        void SetClientFormatterManager(ILinkClientFormatterManager formatterManager);

        /// <summary>
        /// 设置客户端消息打包/解包器实例（可为 null）。建议在启用自定义帧格式化逻辑前先设置。
        /// </summary>
        /// <param name="framer">客户端消息打包/解包器实例</param>
        void SetClientFramer(ILinkClientFramer framer);

        /// <summary>
        /// 将业务对象序列化并异步发送到远端。实现应：
        /// 1) 通过 <see cref="FormatterManager"/> 获取针对 <typeparamref name="T"/> 的格式化器并序列化为字节帧；  
        /// 2) 构造并通过 <see cref="PluginManager"/> 的 OnSend 流程允许插件包装/替换帧；  
        /// 3) 将最终字节发送到底层链路并返回操作结果。
        /// </summary>
        /// <typeparam name="T">要发送的业务对象类型。</typeparam>
        /// <param name="data">要发送的业务对象实例。</param>
        /// <param name="token">取消令牌</param>
        ValueTask<Result<SocketOperationValue>> SendAsync<T>(T data, CancellationToken token = default);
    }
}