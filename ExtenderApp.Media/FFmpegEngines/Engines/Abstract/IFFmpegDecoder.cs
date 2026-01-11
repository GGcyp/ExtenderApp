namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 解码器抽象接口。
    /// 提供“包输入（<see cref="EnqueuePacket"/>）→ 解码循环（<see cref="DecodeLoopAsync"/>）→ 帧输出（<see cref="TryDequeueFrame"/>）”的统一模型，
    /// 以支持不同媒体类型（视频/音频等）的解码实现。
    /// </summary>
    public interface IFFmpegDecoder
    {
        /// <summary>
        /// 获取当前解码器所对应的媒体流索引（Stream Index）。
        /// 通常与容器内的流索引一致，用于在多流场景中区分不同解码器实例。
        /// </summary>
        int StreamIndex { get; }

        /// <summary>
        /// 获取当前解码器处理的媒体类型（例如视频或音频）。
        /// </summary>
        FFmpegMediaType MediaType { get; }

        /// <summary>
        /// 启动解码循环并持续解码，直到收到取消信号或解码结束。
        /// </summary>
        /// <param name="token">
        /// 取消令牌；用于停止解码循环并触发退出流程。
        /// </param>
        /// <returns>表示解码循环生命周期的异步任务。</returns>
        Task DecodeLoopAsync(CancellationToken token);

        /// <summary>
        /// 将一个待解码的包（Packet）入队，供解码循环消费。
        /// </summary>
        /// <param name="packet">要入队的 FFmpeg 包。</param>
        /// <param name="token">取消令牌；用于在入队过程需要阻塞/等待时中断操作。</param>
        void EnqueuePacket(FFmpegPacket packet, CancellationToken token);

        /// <summary>
        /// 尝试从解码器输出队列中取出一帧并出队。
        /// </summary>
        /// <param name="frame">当返回 <c>true</c> 时，输出取到的媒体帧。</param>
        /// <returns>如果成功取到一帧则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        bool TryDequeueFrame(out FFmpegFrame frame);

        /// <summary>
        /// 尝试从解码器输出队列中窥视一帧但不出队。
        /// 适用于需要根据 PTS 等信息决定是否消费该帧的场景。
        /// </summary>
        /// <param name="frame">当返回 <c>true</c> 时，输出窥视到的媒体帧。</param>
        /// <returns>如果成功窥视到一帧则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        bool TryPeekFrame(out FFmpegFrame frame);

        /// <summary>
        /// 更新解码器运行时设置。
        /// </summary>
        /// <param name="settings">要应用的新解码器设置。</param>
        void UpdateSettings(FFmpegDecoderSettings settings);
    }
}