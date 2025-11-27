namespace ExtenderApp.FFmpegEngines.Decoders
{
    /// <summary>
    /// 一个静态工厂类，用于根据媒体类型创建具体的 FFmpeg 解码器实例。
    /// </summary>
    internal static class FFmpegDecoderFactory
    {
        /// <summary>
        /// 根据解码器上下文创建并返回一个具体的解码器实例（视频或音频）。
        /// </summary>
        /// <param name="engine">FFmpeg 引擎实例，用于底层操作。</param>
        /// <param name="context">解码器上下文，包含媒体类型等关键信息。</param>
        /// <param name="info">媒体文件的元数据信息。</param>
        /// <param name="settings">解码器配置设置。</param>
        /// <returns>
        /// 如果上下文有效，则返回 <see cref="FFmpegVideoDecoder"/> 或 <see cref="FFmpegAudioDecoder"/> 的实例。
        /// 如果上下文为空，则返回 <c>null</c>。
        /// </returns>
        /// <exception cref="NotSupportedException">当上下文中的媒体类型不是受支持的类型（如视频、音频）时抛出。</exception>
        public static FFmpegDecoder CreateDecoder(FFmpegEngine engine, FFmpegDecoderContext context, FFmpegInfo info, FFmpegDecoderSettings settings)
        {
            return context.MediaType switch
            {
                FFmpegMediaType.VIDEO => new FFmpegVideoDecoder(engine, context, info, settings),
                FFmpegMediaType.AUDIO => new FFmpegAudioDecoder(engine, context, info, settings),
                _ => throw new NotSupportedException($"不支持的解码器类型: {context.MediaType}"),
            };
        }
    }
}
