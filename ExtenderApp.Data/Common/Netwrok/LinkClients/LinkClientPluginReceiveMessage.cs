namespace ExtenderApp.Data
{
    /// <summary>
    /// 插件接收消息的临时封装，用于在接收回调中传递本次接收的原始结果、原始字节视图以及已解析出的帧集合。
    /// </summary>
    public ref struct LinkClientPluginReceiveMessage
    {
        /// <summary>
        /// 底层接收操作的结果，包含已接收字节数、远端终结点、底层错误信息及统一的结果码等。
        /// </summary>
        public SocketOperationValue Result;

        /// <summary>
        /// 输出的已解析帧集合。解析器/插件可在此填充若干 <see cref="Frame"/> 以便上层消费。
        /// </summary>
        public PooledFrameList OutMessageFrames;

        /// <summary>
        /// 使用接收结果与原始字节视图构造临时消息封装。<see cref="OutMessageFrames"/> 默认为空集合（等待插件填充）。
        /// </summary>
        /// <param name="result">底层接收操作结果。</param>
        /// <param name="resultMessage">本次接收的原始只读字节数据视图。</param>
        public LinkClientPluginReceiveMessage(SocketOperationValue result, PooledFrameList frames = default)
        {
            Result = result;
            OutMessageFrames = frames;
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
