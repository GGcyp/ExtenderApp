namespace ExtenderApp.Data
{
    /// <summary>
    /// 发送管线中用于插件交互的临时封装结构（栈上类型）。  
    /// 插件可通过修改 <see cref="FirstMessageBuffer"/> 或 <see cref="OutMessageBuffer"/> 来对原始消息进行前置或替换处理，
    /// 最终由 <see cref="ToBlock"/> 将这些缓冲合并成用于发送的 <see cref="ByteBlock"/>。
    /// </summary>
    /// <remarks>
    /// - 本类型为 <c>ref struct</c>，只能在栈上使用，不能装箱、不能跨异步边界或存入堆上结构。  
    /// - 字段中包含的 <see cref="ByteBuffer"/> 类型也为 <c>ref struct</c>，均为非线程安全的快照/租约缓冲；使用完成后必须调用 <see cref="Dispose"/> 回收/归还租约。  
    /// - 所有者责任：构造时传入的 <see cref="OriginalMessageBuffer"/> 的生命周期由本结构接管；调用方应在插件处理完成后调用 <see cref="Dispose"/>。  
    /// - <see cref="ToBlock"/> 会创建并返回一个新的 <see cref="ByteBlock"/>（拷贝当前各缓冲的可读数据），返回的 <see cref="ByteBlock"/> 由调用方负责释放（调用其 <see cref="ByteBlock.Dispose"/>）。
    /// </remarks>
    public ref struct LinkClientPluginSendMessage
    {
        /// <summary>
        /// 原始消息缓冲（只读字段），通常由格式化器或上层提供。构造后所有权转移到本实例，由 <see cref="Dispose"/> 负责释放。
        /// </summary>
        public readonly ByteBuffer OriginalMessageBuffer;

        /// <summary>
        /// 消息类型标识，由上层协议或格式产生。插件可根据该字段决定如何处理消息。
        /// </summary>
        public int MessageType;

        /// <summary>
        /// 输出缓冲：插件可在此写入替代或附加的完整消息内容。若该缓冲非空，最终输出会以此为主；否则使用 <see cref="OriginalMessageBuffer"/>。
        /// </summary>
        public ByteBuffer OutMessageBuffer;

        /// <summary>
        /// 获取最终用于发送的消息缓冲（只读属性）。
        /// </summary>
        public ByteBuffer ResultOutMessageBuffer => OutMessageBuffer.Remaining > 0 ? OutMessageBuffer : OriginalMessageBuffer;

        /// <summary>
        /// 使用原始消息缓冲与消息类型构造一个插件消息封装实例。会为 <see cref="FirstMessageBuffer"/> 与 <see cref="OutMessageBuffer"/> 分配新缓冲。
        /// </summary>
        /// <param name="dataBuffer">原始消息缓冲，所有权转移到新实例。</param>
        /// <param name="Message">消息类型标识。</param>
        public LinkClientPluginSendMessage(ByteBuffer dataBuffer, int Message)
        {
            OriginalMessageBuffer = dataBuffer;
            MessageType = Message;

            OutMessageBuffer = ByteBuffer.CreateBuffer();
        }

        /// <summary>
        /// 释放本封装持有的所有缓冲租约。调用后不应再使用任何字段的实例。
        /// </summary>
        public void Dispose()
        {
            OriginalMessageBuffer.Dispose();
            OutMessageBuffer.Dispose();
        }
    }
}