namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一个单条消息帧的封装：包含消息类型标识与对应的字节负载。
    /// </summary>
    /// <remarks>
    /// - 所有权规则：Frame 对 <see cref="Payload"/> 拥有释放（归还）责任。构造时负载的所有权转移给 Frame，调用方在不再使用时应调用 <see cref="Dispose"/>。  
    /// - 值类型注意：Frame 为只读值类型（readonly struct），复制时会进行浅拷贝 —— 复制的副本将引用相同的 <see cref="ByteBlock"/> 数据。请避免在多个副本上重复调用 <see cref="Dispose"/>，以免导致重复归还/释放。  
    /// - 线程安全：<see cref="ByteBlock"/> 本身非线程安全；并发访问需在外部同步或确保单一线程拥有并释放该实例。  
    /// - 推荐用法：建议使用 using/try-finally 模式确保在处理完负载后释放资源，例如：
    ///   <code>
    ///   using var frame = new Frame(messageType, payload);
    ///   // 读取 frame.Payload 中的数据
    ///   </code>
    /// </remarks>
    public readonly struct Frame : IDisposable
    {
        /// <summary>
        /// 消息类型或帧标识，由上层协议/格式定义的整数值。
        /// </summary>
        public readonly int MessageType;

        /// <summary>
        /// 帧负载缓冲，承载已写入的数据。Frame 对此实例拥有释放责任（通过 <see cref="Dispose"/> 归还/释放）。
        /// </summary>
        public readonly ByteBlock Payload;

        /// <summary>
        /// 使用指定消息类型与负载构造一个帧实例。构造后 Frame 对 <paramref name="payload"/> 拥有释放责任。
        /// </summary>
        /// <param name="messageType">消息类型标识。</param>
        /// <param name="payload">负载缓冲，所有权转移到新构造的 Frame。</param>
        public Frame(int messageType, ByteBlock payload)
        {
            MessageType = messageType;
            Payload = payload;
        }

        /// <summary>
        /// 释放帧持有的资源：归还/释放 <see cref="Payload"/>。
        /// </summary>
        /// <remarks>
        /// - 调用后不应再访问 <see cref="Payload"/> 的内容或其方法。  
        /// - 因为 Frame 是值类型且复制为浅拷贝，请确保只有单一拥有者调用 Dispose；重复调用可能导致异常或逻辑错误（例如重复归还池中缓冲）。
        /// </remarks>
        public void Dispose()
        {
            Payload.Dispose();
        }
    }
}
