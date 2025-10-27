namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 客户端格式化器管理器。
    /// </summary>
    /// <remarks>
    /// 职责：
    /// - 维护“数据类型哈希（MessageType）→ IClientFormatter 实例”的映射；<br/>
    /// - 支持以类型哈希或泛型类型两种方式获取已注册的格式化器；<br/>
    /// - 供网络管道在反序列化前进行快速路由与分发。<br/>
    /// 约定：
    /// - MessageType 应与 <see cref="IClientFormatter{T}"/> 内部约定保持一致（例如基于类型名的稳定哈希）；<br/>
    /// - 建议实现为线程安全（读多写少）以适配运行期收发并发。
    /// </remarks>
    /// <seealso cref="ILinkClientFormatter"/>
    /// <seealso cref="IClientFormatter{T}"/>
    public interface ILinkClientFormatterManager
    {
        /// <summary>
        /// 注册一个类型 <typeparamref name="T"/> 的客户端格式化器。
        /// </summary>
        /// <typeparam name="T">消息/数据的强类型。</typeparam>
        /// <param name="formatter">要注册的格式化器实例，其 <see cref="ILinkClientFormatter.MessageType"/> 用作键。</param>
        /// <remarks>
        /// 冲突策略（同一 MessageType 已存在时的行为）由具体实现决定：可选择抛出、覆盖或忽略。
        /// </remarks>
        void AddFormatter<T>(IClientFormatter<T> formatter);

        /// <summary>
        /// 按数据类型哈希获取已注册的格式化器。
        /// </summary>
        /// <param name="dataTypeHash">数据类型的稳定哈希（与发送端/解码端保持一致）。</param>
        /// <returns>匹配的格式化器；若未找到返回 null。</returns>
        ILinkClientFormatter? GetFormatter(int dataTypeHash);

        /// <summary>
        /// 获取类型 <typeparamref name="T"/> 对应的已注册格式化器。
        /// </summary>
        /// <typeparam name="T">消息/数据的强类型。</typeparam>
        /// <returns>匹配的强类型格式化器；若未找到返回 null。</returns>
        /// <remarks>
        /// 具体实现通常会基于与 <see cref="IClientFormatter{T}"/> 相同的规则计算 <c>MessageType</c> 后再查找。
        /// </remarks>
        IClientFormatter<T>? GetFormatter<T>();

        /// <summary>
        /// 删除指定类型 <typeparamref name="T"/> 的已注册格式化器。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        void RemoveFormatter<T>();
    }
}
