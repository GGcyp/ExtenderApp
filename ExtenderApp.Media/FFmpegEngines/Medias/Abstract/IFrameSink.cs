namespace ExtenderApp.FFmpegEngines.Medias
{
    /// <summary>
    /// 帧消费端抽象： <see cref="IFrameProcessController"/> 将“可输出帧”交给 Sink，Sink 决定写到哪里（播放/编码/推流/录制等）。
    /// </summary>
    internal interface IFrameSink
    {
        /// <summary>
        /// 写入一帧。
        /// </summary>
        /// <param name="mediaType">帧所属媒体类型。</param>
        /// <param name="frame">媒体帧（调用方在返回后仍会 Dispose；Sink 不应持有该实例）。</param>
        void WriteFrame(FFmpegMediaType mediaType, FFmpegFrame frame);
    }
}