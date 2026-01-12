namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// 表示 FFmpeg 解码流程的控制器。
    /// </summary>
    /// <remarks>负责管理解码器集合、解码线程/任务的启动与停止、Seek 行为以及与“世代（Generation）”相关的状态控制。</remarks>
    public interface IFFmpegDecoderController : IDisposable
    {
        /// <summary>
        /// 获取当前控制器管理的解码器集合。
        /// </summary>
        IFFmpegDecoderCollection DecoderCollection { get; }

        /// <summary>
        /// 获取当前媒体的基础信息。
        /// </summary>
        FFmpegInfo Info { get; }

        /// <summary>
        /// 获取用于取消解码流程的令牌。
        /// </summary>
        CancellationToken Token { get; }

        /// <summary>
        /// 获取解码配置。
        /// </summary>
        FFmpegDecoderSettings Settings { get; }

        /// <summary>
        /// 表示解码流程是否已完成。
        /// </summary>
        bool Completed { get; }

        /// <summary>
        /// 获取当前解码器“世代”标识。
        /// </summary>
        /// <returns>用于区分不同解码阶段（例如 Seek 后重建/重置）的整数标识。</returns>
        int GetCurrentGeneration();

        /// <summary>
        /// 将解码位置跳转到指定时间点。
        /// </summary>
        /// <param name="position">目标位置（毫秒）。</param>
        void SeekDecoder(long position);

        /// <summary>
        /// 启动解码流程。
        /// </summary>
        void StartDecode();

        /// <summary>
        /// 异步停止解码流程，并等待相关资源释放/任务退出。
        /// </summary>
        /// <returns>表示停止过程的任务。</returns>
        Task StopDecodeAsync();
    }
}