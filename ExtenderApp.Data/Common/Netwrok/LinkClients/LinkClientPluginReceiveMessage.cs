namespace ExtenderApp.Data
{
    /// <summary>
    /// 插件接收消息的临时封装，用于在接收回调中传递本次接收的原始结果、原始字节视图以及已解析出的帧集合。
    /// </summary>
    /// <remarks>
    /// - 本类型为 <c>ref struct</c>（栈上类型），不能装箱或跨异步边界/线程存储；生命周期应仅限于当前调用栈。  
    /// - <see cref="ResultMessage"/> 可能引用来自缓冲池的内存片段（例如 <see cref="ArrayPool{T}"/> 或 ByteBlock 内部缓冲），请勿在离开当前调用上下文后继续持有该引用。  
    /// - <see cref="OutMessageFrames"/> 为可池化的帧集合，若其中包含帧则调用 <see cref="Dispose"/> 会释放集合与内部帧所持有的资源（例如归还 ByteBlock）；调用方应在处理完成后确保调用 <see cref="Dispose"/> 以避免资源泄漏。  
    /// </remarks>
    public ref struct LinkClientPluginReceiveMessage
    {
        /// <summary>
        /// 底层接收操作的结果，包含已接收字节数、远端终结点、底层错误信息及统一的结果码等。
        /// </summary>
        public SocketOperationResult Result;

        /// <summary>
        /// 本次接收的原始字节视图（只读）。视该内存为临时数据，不要在超出当前调用上下文后继续使用或保存引用。
        /// </summary>
        public ReadOnlyMemory<byte> ResultMessage;

        /// <summary>
        /// 输出的已解析帧集合。解析器/插件可在此填充若干 <see cref="Frame"/> 以便上层消费。
        /// </summary>
        /// <remarks>
        /// - 当集合被填充后，调用 <see cref="Dispose"/> 会逐一释放集合内的 <see cref="Frame"/>（从而释放帧内的 <see cref="ByteBlock"/>）。  
        /// - 不要在未调用 <see cref="Dispose"/> 的情况下将本结构或其内部集合传递给长期持有方。
        /// </remarks>
        public PooledFrameList OutMessageFrames;

        /// <summary>
        /// 使用接收结果与原始字节视图构造临时消息封装。<see cref="OutMessageFrames"/> 默认为空集合（等待插件填充）。
        /// </summary>
        /// <param name="result">底层接收操作结果。</param>
        /// <param name="resultMessage">本次接收的原始只读字节数据视图。</param>
        public LinkClientPluginReceiveMessage(SocketOperationResult result, ReadOnlyMemory<byte> resultMessage)
        {
            Result = result;
            ResultMessage = resultMessage;
            OutMessageFrames = default;
        }

        /// <summary>
        /// 释放本封装持有的输出帧集合资源。若 <see cref="OutMessageFrames"/> 中包含帧，则会对每个帧调用 <see cref="Frame.Dispose"/> 并归还内部数组池。
        /// </summary>
        public void Dispose()
        {
            OutMessageFrames.Dispose();
        }
    }
}
