namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 编码器抽象接口。
    /// <para>提供“帧输入（ <see cref="EnqueueFrame"/>）→ 编码循环（ <see cref="EncodeLoopAsync"/>）→ 包输出（ <see cref="TryDequeuePacket"/>）”的统一模型。</para>
    /// <para>典型用途：录制/转码/推流场景中，将解码得到的原始帧编码为压缩码流包。</para>
    /// </summary>
    public interface IFFmpegEncoder
    {
        /// <summary>
        /// 获取当前编码器所对应的目标流索引（Stream Index）。 用于多路输出（如音频+视频）时区分不同编码器实例。
        /// </summary>
        int StreamIndex { get; }

        /// <summary>
        /// 获取当前编码器处理的媒体类型（例如视频或音频）。
        /// </summary>
        FFmpegMediaType MediaType { get; }

        /// <summary>
        /// 启动编码循环并持续编码，直到收到取消信号或编码结束。
        /// </summary>
        /// <param name="token">取消令牌；用于停止编码循环并触发退出流程。</param>
        /// <returns>表示编码循环生命周期的异步任务。</returns>
        Task EncodeLoopAsync(CancellationToken token);

        /// <summary>
        /// 将一帧原始媒体数据入队，供编码循环消费。
        /// <para>约定：写入成功后帧的资源所有权转移给编码器；写入失败时调用方需自行释放帧（ <see cref="FFmpegFrame.Dispose"/>）。</para>
        /// </summary>
        /// <param name="frame">要编码的原始帧。</param>
        /// <param name="token">取消令牌；用于在入队过程需要阻塞/等待时中断操作。</param>
        void EnqueueFrame(FFmpegFrame frame, CancellationToken token);

        /// <summary>
        /// 尝试从编码输出队列中取出一个编码后的包并出队。
        /// <para>约定：出队成功后包的资源所有权转移给调用方；调用方需在使用完成后通过 <c>FFmpegEngine.Release</c> 归还底层包。</para>
        /// </summary>
        /// <param name="packet">输出编码后的包。</param>
        /// <returns>如果成功取到一个包则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        bool TryDequeuePacket(out FFmpegPacket packet);

        /// <summary>
        /// 尝试预览下一个编码后的包但不出队。
        /// </summary>
        /// <param name="packet">输出预览到的包。</param>
        /// <returns>如果成功预览到一个包则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        bool TryPeekPacket(out FFmpegPacket packet);

        /// <summary>
        /// 更新编码器运行时设置。
        /// </summary>
        /// <param name="settings">要应用的新编码器设置。</param>
        void UpdateSettings(FFmpegEncoderSettings settings);
    }
}